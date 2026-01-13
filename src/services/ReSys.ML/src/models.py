from typing import List
from pydantic import BaseModel

class EmbeddingRequest(BaseModel):
    image_url: str 
    Example_id: str

class EmbeddingResponse(BaseModel):
    embedding: List[float]
