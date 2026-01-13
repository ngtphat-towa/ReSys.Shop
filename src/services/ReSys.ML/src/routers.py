from fastapi import APIRouter
from models import EmbeddingRequest, EmbeddingResponse
from engine import engine
from settings import settings

router = APIRouter()

@router.post("/embed", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    embedding = engine.generate_embedding(request.image_url)
    return {"embedding": embedding}

@router.get("/health")
async def health():
    return {
        "status": "ok", 
        "model": settings.MODEL_NAME if not engine.is_mock() else "mock"
    }
