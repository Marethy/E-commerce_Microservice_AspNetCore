import torch
from transformers import CLIPModel, CLIPProcessor
from elasticsearch import Elasticsearch
from typing import List, Dict, Optional, Tuple
import logging
import numpy as np

from .config import (
    ELASTICSEARCH_URL,
    ELASTICSEARCH_USER,
    ELASTICSEARCH_PASSWORD,
    ELASTICSEARCH_INDEX,
    CLIP_MODEL_NAME,
    RRF_K
)
from .models import SearchRequest, ProductIndexRequest

logger = logging.getLogger(__name__)

class SearchEngine:
    def __init__(self):
        logger.info(f"Initializing Search Engine with CLIP model: {CLIP_MODEL_NAME}")
        
        self.device = torch.device("cpu")
        logger.info("Using CPU device for CLIP model")
        
        self.clip_model = CLIPModel.from_pretrained(CLIP_MODEL_NAME)
        self.clip_processor = CLIPProcessor.from_pretrained(CLIP_MODEL_NAME)
        self.clip_model.to(self.device)
        self.clip_model.eval()
        
        self.es = Elasticsearch(
            [ELASTICSEARCH_URL],
            basic_auth=(ELASTICSEARCH_USER, ELASTICSEARCH_PASSWORD),
            request_timeout=30
        )
        
        self._create_index_if_not_exists()
        logger.info("Search Engine initialized successfully")
    
    def _create_index_if_not_exists(self):
        if not self.es.indices.exists(index=ELASTICSEARCH_INDEX):
            logger.info(f"Creating Elasticsearch index: {ELASTICSEARCH_INDEX}")
            index_mapping = {
                "mappings": {
                    "properties": {
                        "id": {"type": "keyword"},
                        "name": {"type": "text", "analyzer": "standard"},
                        "description": {"type": "text"},
                        "shortDescription": {"type": "text"},
                        "embedding": {
                            "type": "dense_vector",
                            "dims": 512,
                            "index": True,
                            "similarity": "cosine"
                        }
                    }
                }
            }
            self.es.indices.create(index=ELASTICSEARCH_INDEX, body=index_mapping)
    
    def generate_embedding(self, text: str) -> List[float]:
        if not text or not text.strip():
            return [0.0] * 512
        
        inputs = self.clip_processor(
            text=[text], 
            return_tensors="pt", 
            padding=True,
            truncation=True,
            max_length=77
        )
        inputs = {k: v.to(self.device) for k, v in inputs.items()}
        
        with torch.no_grad():
            embeddings = self.clip_model.get_text_features(**inputs)
        
        return embeddings[0].cpu().tolist()
    
    def generate_image_embedding(self, image_base64: str) -> List[float]:
        import base64
        import io
        from PIL import Image
        
        try:
            image_bytes = base64.b64decode(image_base64)
            image = Image.open(io.BytesIO(image_bytes))
            if image.mode != 'RGB':
                image = image.convert('RGB')
            
            inputs = self.clip_processor(images=image, return_tensors="pt")
            inputs = {k: v.to(self.device) for k, v in inputs.items()}
            
            with torch.no_grad():
                embeddings = self.clip_model.get_image_features(**inputs)
            
            return embeddings[0].cpu().tolist()
        except Exception as e:
            logger.error(f"Error generating image embedding: {e}")
            return [0.0] * 512

    def _strip_html(self, html_text: Optional[str]) -> str:
        if not html_text:
            return ""
        try:
            from bs4 import BeautifulSoup
            soup = BeautifulSoup(html_text, 'html.parser')
            return soup.get_text(separator=' ', strip=True)
        except Exception as e:
            logger.warning(f"Failed to parse HTML, using raw text: {e}")
            return html_text

    def _generate_weighted_embedding(self, name: str, short_desc: Optional[str], description: Optional[str]) -> List[float]:
        embeddings = []
        weights = []
        
        if name and name.strip():
            embeddings.append(self.generate_embedding(name))
            weights.append(3.0)
        
        if short_desc and short_desc.strip():
            short_clean = self._strip_html(short_desc)
            if short_clean:
                embeddings.append(self.generate_embedding(short_clean))
                weights.append(2.0)
        
        if description and description.strip():
            desc_clean = self._strip_html(description)
            if desc_clean:
                embeddings.append(self.generate_embedding(desc_clean))
                weights.append(2.0)
        
        if not embeddings:
            return [0.0] * 512
        
        embeddings_array = np.array(embeddings)
        weights_array = np.array(weights).reshape(-1, 1)
        weighted_sum = np.sum(embeddings_array * weights_array, axis=0)
        weight_sum = np.sum(weights_array)
        
        return (weighted_sum / weight_sum).tolist()

    def index_product(self, product: ProductIndexRequest):
        embedding = self._generate_weighted_embedding(
            product.name, product.shortDescription, product.description
        )
        doc = {
            "id": product.id,
            "name": product.name,
            "description": product.description,
            "shortDescription": product.shortDescription,
            "embedding": embedding
        }
        self.es.index(index=ELASTICSEARCH_INDEX, id=product.id, document=doc)
        logger.info(f"Indexed product: {product.id}")

    def bulk_index_products(self, products: List[ProductIndexRequest]) -> Dict:
        from elasticsearch.helpers import bulk
        actions = []
        for product in products:
            embedding = self._generate_weighted_embedding(
                product.name, product.shortDescription, product.description
            )
            doc = {
                "id": product.id,
                "name": product.name,
                "description": product.description,
                "shortDescription": product.shortDescription,
                "embedding": embedding
            }
            actions.append({"_index": ELASTICSEARCH_INDEX, "_id": product.id, "_source": doc})
        
        success, failed = bulk(self.es, actions, raise_on_error=False)
        logger.info(f"Bulk indexed {success} products, {len(failed)} failed")
        return {"success": success, "failed": len(failed)}

    def delete_product(self, product_id: str):
        self.es.delete(index=ELASTICSEARCH_INDEX, id=product_id, ignore=[404])
        logger.info(f"Deleted product: {product_id}")

    def recreate_index(self):
        logger.info(f"Recreating index: {ELASTICSEARCH_INDEX}")
        if self.es.indices.exists(index=ELASTICSEARCH_INDEX):
            self.es.indices.delete(index=ELASTICSEARCH_INDEX)
        self._create_index_if_not_exists()
        logger.info(f"Recreated index: {ELASTICSEARCH_INDEX}")

    def hybrid_search(self, request: SearchRequest) -> Tuple[List[str], int]:
        if not request.query and not request.image:
            return [], 0
        
        embeddings = []
        weights = []
        if request.query:
            embeddings.append(self.generate_embedding(request.query))
            weights.append(1.0)
        if request.image:
            embeddings.append(self.generate_image_embedding(request.image))
            weights.append(1.0)
        
        if len(embeddings) > 1:
            embeddings_array = np.array(embeddings)
            weights_array = np.array(weights).reshape(-1, 1)
            query_embedding = (np.sum(embeddings_array * weights_array, axis=0) / np.sum(weights_array)).tolist()
        else:
            query_embedding = embeddings[0]

        offset = request.page * request.size
        
        knn_param = [{
            "field": "embedding",
            "query_vector": query_embedding,
            "k": 50,
            "num_candidates": 100
        }]

        query_param = None
        if request.query:
            query_param = {
                "multi_match": {
                    "query": request.query,
                    "fields": ["name^3", "description^2", "shortDescription^2"],
                    "fuzziness": "AUTO"
                }
            }
            
        rank_param = None
        if request.query:
            rank_param = {
                "rrf": {
                    "window_size": 50,
                    "rank_constant": 60
                }
            }

        try:
            response = self.es.search(
                index=ELASTICSEARCH_INDEX,
                knn=knn_param,
                query=query_param,
                rank=rank_param,
                size=request.size,
                from_=offset,
                source=["id"]
            )
            
            hits = response['hits']
            if isinstance(hits['total'], int):
                total = hits['total']
            else:
                total = hits['total']['value']
                
            product_ids = [hit['_source']['id'] for hit in hits['hits']]
            return product_ids, total
            
        except Exception as e:
            logger.error(f"Search failed: {e}")
            import traceback
            logger.error(traceback.format_exc())
            return [], 0