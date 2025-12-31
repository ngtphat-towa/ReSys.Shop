import os
import random
from contextlib import asynccontextmanager
from typing import List, Optional

from fastapi import FastAPI
from pydantic import BaseModel

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

app = FastAPI(lifespan=lifespan)

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
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)