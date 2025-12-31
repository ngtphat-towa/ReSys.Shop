import os
import random
import requests
from contextlib import asynccontextmanager
from typing import List, Optional

from fastapi import FastAPI
from pydantic import BaseModel
from opentelemetry import trace
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor

# Initialize OpenTelemetry
otlp_endpoint = os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT")
if otlp_endpoint:
    resource = Resource.create({"service.name": "ml"})
    provider = TracerProvider(resource=resource)
    
    # Use HTTP exporter for better compatibility in dev
    # The endpoint for HTTP usually needs /v1/traces
    endpoint = f"{otlp_endpoint}/v1/traces" if not otlp_endpoint.endswith("/v1/traces") else otlp_endpoint
    
    # Disable certificate validation for development (self-signed certs)
    import requests
    session = requests.Session()
    session.verify = False
    exporter = OTLPSpanExporter(endpoint=endpoint, session=session)
    
    processor = BatchSpanProcessor(exporter)
    provider.add_span_processor(processor)
    trace.set_tracer_provider(provider)

# Note: We use a lightweight model by default. 
MODEL_NAME = "all-MiniLM-L6-v2"

# Global model variable
model = None

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Load the ML model on startup
    global model
    
    # Check if we should use real model or mock
    USE_MOCK = os.getenv("USE_MOCK_ML", "false").lower() == "true"
    
    if not USE_MOCK:
        print(f"Loading model: {MODEL_NAME}...")
        try:
            from sentence_transformers import SentenceTransformer
            model = SentenceTransformer(MODEL_NAME)
            print("Model loaded successfully.")
        except ImportError:
            print("Warning: sentence-transformers not found. Falling back to MOCK mode.")
    else:
        print("Running in MOCK mode.")
        
    yield
    # Clean up on shutdown
    print("Shutting down...")

app = FastAPI(
    title="ReSys ML Service",
    description="Microservice for generating product embeddings and similarity search.",
    version="1.0.0",
    lifespan=lifespan,
    docs_url="/docs",
    redoc_url="/redoc",
    root_path=os.getenv("ROOT_PATH", "")
)

# Instrument the app if not already instrumented (to avoid warnings on reload)
if not getattr(app, "is_instrumented", False):
    FastAPIInstrumentor.instrument_app(app)
    app.is_instrumented = True

class EmbeddingRequest(BaseModel):
    image_url: str 
    product_id: str

class EmbeddingResponse(BaseModel):
    embedding: List[float]

@app.post("/embed", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    if model:
        embedding = model.encode(request.image_url).tolist()
    else:
        # Mock embedding (384 dimensions to match MiniLM)
        dimension = 384
        embedding = [random.random() for _ in range(dimension)]
    
    return {"embedding": embedding}

@app.get("/health")
async def health():
    return {"status": "ok", "model": MODEL_NAME if model else "mock"}

if __name__ == "__main__":
    import uvicorn
    port = int(os.getenv("PORT", "8000"))
    uvicorn.run("main:app", host="0.0.0.0", port=port, reload=True)