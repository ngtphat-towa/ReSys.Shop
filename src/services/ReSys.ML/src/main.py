import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor

from .settings import settings
from .telemetry import setup_telemetry
from .engine import engine
from .routers import router

# Setup Logging & Telemetry
logger = logging.getLogger(__name__)
setup_telemetry()

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Load ML model
    engine.load_model()
    yield
    # Cleanup
    logger.info("Shutting down...")

app = FastAPI(
    title="ReSys ML Service",
    description="Microservice for generating Example embeddings and similarity search.",
    version="1.0.0",
    lifespan=lifespan,
    docs_url="/docs",
    redoc_url="/redoc",
    root_path=settings.ROOT_PATH
)

# Include Routes
app.include_router(router)

# Instrument FastAPI
if not getattr(app, "_is_instrumented_by_opentelemetry", False):
    FastAPIInstrumentor.instrument_app(app)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=settings.PORT, reload=True)
