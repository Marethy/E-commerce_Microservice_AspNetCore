import torch
from transformers import CLIPModel, CLIPProcessor
from elasticsearch import Elasticsearch
from typing import List, Dict, Optional, Tuple
import logging

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
            # Return zero vector for empty text
            return [0.0] * 512
        
        inputs = self.clip_processor(
            text=[text], 
            return_tensors="pt", 
            padding=True,
            truncation=True,
            max_length=77  # CLIP max tokens
        )
        inputs = {k: v.to(self.device) for k, v in inputs.items()}
        
        with torch.no_grad():
            embeddings = self.clip_model.get_text_features(**inputs)
        
        return embeddings[0].cpu().tolist()
    
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
    
    def _generate_weighted_embedding(
        self, 
        name: str, 
        short_desc: Optional[str], 
        description: Optional[str]
    ) -> List[float]:
        import numpy as np
        
        embeddings = []
        weights = []
        
        # Name embedding (weight: 3)
        if name and name.strip():
            name_emb = self.generate_embedding(name)
            embeddings.append(name_emb)
            weights.append(3.0)
        
        # Short description embedding (weight: 2)
        if short_desc and short_desc.strip():
            short_clean = self._strip_html(short_desc)
            if short_clean:
                short_emb = self.generate_embedding(short_clean)
                embeddings.append(short_emb)
                weights.append(2.0)
        
        # Description embedding (weight: 2)
        if description and description.strip():
            desc_clean = self._strip_html(description)
            if desc_clean:
                desc_emb = self.generate_embedding(desc_clean)
                embeddings.append(desc_emb)
                weights.append(2.0)
        
        if not embeddings:
            # Fallback to zero vector
            return [0.0] * 512
        
        # Weighted average
        embeddings_array = np.array(embeddings)
        weights_array = np.array(weights).reshape(-1, 1)
        weighted_sum = np.sum(embeddings_array * weights_array, axis=0)
        weight_sum = np.sum(weights_array)
        
        final_embedding = (weighted_sum / weight_sum).tolist()
        return final_embedding
    
    def index_product(self, product: ProductIndexRequest):
        # Generate weighted embedding from name, shortDescription, description
        embedding = self._generate_weighted_embedding(
            product.name,
            product.shortDescription,
            product.description
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
                product.name,
                product.shortDescription,
                product.description
            )
            
            doc = {
                "id": product.id,
                "name": product.name,
                "description": product.description,
                "shortDescription": product.shortDescription,
                "embedding": embedding
            }
            
            actions.append({
                "_index": ELASTICSEARCH_INDEX,
                "_id": product.id,
                "_source": doc
            })
        
        success, failed = bulk(self.es, actions, raise_on_error=False)
        logger.info(f"Bulk indexed {success} products, {len(failed)} failed")
        
        return {"success": success, "failed": len(failed)}

    
    def delete_product(self, product_id: str):
        self.es.delete(index=ELASTICSEARCH_INDEX, id=product_id, ignore=[404])
        logger.info(f"Deleted product: {product_id}")
    
    def hybrid_search(self, request: SearchRequest) -> Tuple[List[str], int]:
        if not request.query:
            return [], 0
        
        fuzzy_results = self._fuzzy_search(request.query)
        query_embedding = self.generate_embedding(request.query)
        vector_results = self._vector_search(query_embedding)
        
        merged_results = self._rrf_merge(fuzzy_results, vector_results, k=RRF_K)
        
        total = len(merged_results)
        start = request.page * request.size
        end = start + request.size
        paginated_ids = merged_results[start:end]
        
        return paginated_ids, total
    

    
    def _fuzzy_search(self, query: str) -> List[Tuple[str, int]]:
        es_query = {
            "query": {
                "multi_match": {
                    "query": query,
                    "fields": ["name^3", "description^2", "shortDescription^2"],
                    "fuzziness": "AUTO"
                }
            },
            "size": 1000
        }
        
        response = self.es.search(index=ELASTICSEARCH_INDEX, body=es_query)
        
        results = []
        for rank, hit in enumerate(response['hits']['hits']):
            results.append((hit['_source']['id'], rank))
        
        return results
    
    def _vector_search(self, embedding: List[float]) -> List[Tuple[str, int]]:
        es_query = {
            "query": {
                "script_score": {
                    "query": {"match_all": {}},
                    "script": {
                        "source": "cosineSimilarity(params.query_vector, 'embedding') + 1.0",
                        "params": {"query_vector": embedding}
                    }
                }
            },
            "size": 1000
        }
        
        response = self.es.search(index=ELASTICSEARCH_INDEX, body=es_query)
        
        results = []
        for rank, hit in enumerate(response['hits']['hits']):
            results.append((hit['_source']['id'], rank))
        
        return results
    

    
    def _rrf_merge(self, fuzzy_results: List[Tuple[str, int]], vector_results: List[Tuple[str, int]], k: int = 60) -> List[str]:
        scores = {}
        
        for product_id, rank in fuzzy_results:
            scores[product_id] = scores.get(product_id, 0) + (1.0 / (k + rank))
        
        for product_id, rank in vector_results:
            scores[product_id] = scores.get(product_id, 0) + (1.0 / (k + rank))
        
        sorted_ids = sorted(scores.items(), key=lambda x: x[1], reverse=True)
        
        return [product_id for product_id, _ in sorted_ids]
    

