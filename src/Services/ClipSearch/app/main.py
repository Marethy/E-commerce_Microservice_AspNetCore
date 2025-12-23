from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from typing import List
import logging

from .models import (
    ProductIndexRequest,
    SearchRequest,
    SearchResponse,
    EmbeddingRequest,
    EmbeddingResponse
)
from .search_engine import SearchEngine

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="CLIP Search Service",
    description="Advanced product search with CLIP embeddings and Elasticsearch",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

search_engine = None

@app.on_event("startup")
async def startup_event():
    global search_engine
    logger.info("Starting CLIP Search Service...")
    search_engine = SearchEngine()
    logger.info("CLIP Search Service started successfully")

@app.get("/")
async def root():
    return {
        "service": "CLIP Search API",
        "version": "1.0.0",
        "status": "running"
    }

@app.get("/health")
async def health_check():
    return {"status": "healthy"}

@app.post("/embeddings", response_model=EmbeddingResponse)
async def generate_embedding(request: EmbeddingRequest):
    try:
        embedding = search_engine.generate_embedding(request.text)
        return EmbeddingResponse(embedding=embedding)
    except Exception as e:
        logger.error(f"Error generating embedding: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/index-product")
async def index_product(product: ProductIndexRequest):
    try:
        search_engine.index_product(product)
        return {"message": f"Product {product.id} indexed successfully"}
    except Exception as e:
        logger.error(f"Error indexing product {product.id}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/bulk-index-products")
async def bulk_index_products(request: List[ProductIndexRequest]):
    try:
        result = search_engine.bulk_index_products(request)
        return {
            "message": f"Bulk indexed {result['success']} products",
            "success": result['success'],
            "failed": result['failed']
        }
    except Exception as e:
        logger.error(f"Error bulk indexing products: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


@app.delete("/index-product/{product_id}")
async def delete_product(product_id: str):
    try:
        search_engine.delete_product(product_id)
        return {"message": f"Product {product_id} deleted successfully"}
    except Exception as e:
        logger.error(f"Error deleting product {product_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/search", response_model=SearchResponse)
async def search_products(request: SearchRequest):
    try:
        product_ids, total = search_engine.hybrid_search(request)
        
        return SearchResponse(
            productIds=product_ids,
            total=total,
            page=request.page,
            size=request.size
        )
    except Exception as e:
        logger.error(f"Error searching products: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
