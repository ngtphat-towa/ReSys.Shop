from typing import List
from pydantic import BaseModel

class EmbeddingRequest(BaseModel):
    image_url: str 
    product_id: str

class EmbeddingResponse(BaseModel):
    embedding: List[float]
