from pydantic import BaseModel
from typing import Optional, List

class ProductIndexRequest(BaseModel):
    id: str
    name: str
    description: Optional[str] = None
    shortDescription: Optional[str] = None

class BulkIndexRequest(BaseModel):
    products: List[ProductIndexRequest]


class SearchRequest(BaseModel):
    query: Optional[str] = None
    page: int = 0
    size: int = 20

class SearchResponse(BaseModel):
    productIds: List[str]
    total: int
    page: int
    size: int

class EmbeddingRequest(BaseModel):
    text: str

class EmbeddingResponse(BaseModel):
    embedding: List[float]
