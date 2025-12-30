from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional
import random

app = FastAPI()

class EmbeddingRequest(BaseModel):
    image_url: str
    product_id: str

class EmbeddingResponse(BaseModel):
    embedding: List[float]

@app.post("/embed", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    # Mock embedding generation (384 dimensions to match current DB schema)
    # In a real scenario, load a model like CLIP or ResNet here.
    # dimension = 512 # for CLIP
    dimension = 384 # matching current AppDbContext
    
    embedding = [random.random() for _ in range(dimension)]
    
    return {"embedding": embedding}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
