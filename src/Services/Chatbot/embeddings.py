import logging
import numpy as np
import httpx
from config import config

logger = logging.getLogger(__name__)


class EmbeddingService:
    def __init__(self):
        self._cache: dict[str, list[float]] = {}
        self._api_key = config.OPENAI_API_KEY
        self._model = config.EMBEDDING_MODEL
        self._base_url = "https://api.openai.com/v1"
    
    async def get_embedding(self, text: str) -> list[float]:
        if not text.strip():
            return self._fallback_embedding("")
            
        cache_key = text[:500]  # Limit cache key length
        if cache_key in self._cache:
            return self._cache[cache_key]
        
        try:
            async with httpx.AsyncClient(timeout=30) as client:
                resp = await client.post(
                    f"{self._base_url}/embeddings",
                    headers={"Authorization": f"Bearer {self._api_key}"},
                    json={"model": self._model, "input": text[:8000]},  # OpenAI limit
                )
                resp.raise_for_status()
                embedding = resp.json()["data"][0]["embedding"]
                self._cache[cache_key] = embedding
                return embedding
        except Exception as e:
            logger.warning(f"Embedding API error, using fallback: {e}")
            return self._fallback_embedding(text)
    
    def _fallback_embedding(self, text: str) -> list[float]:
        words = text.lower().split()
        vec = [0.0] * 1536  # OpenAI embedding dimension
        for i, w in enumerate(words[:100]):
            idx = hash(w) % 1536
            vec[idx] += 1.0 / (i + 1)
        norm = np.linalg.norm(vec) or 1.0
        return (np.array(vec) / norm).tolist()
    
    async def get_embeddings_batch(self, texts: list[str]) -> list[list[float]]:
        # Filter empty and get unique texts
        unique_texts = list(set(t for t in texts if t.strip()))
        
        # Check cache first
        uncached = [t for t in unique_texts if t[:500] not in self._cache]
        
        if uncached:
            try:
                async with httpx.AsyncClient(timeout=60) as client:
                    # OpenAI supports batch embeddings
                    resp = await client.post(
                        f"{self._base_url}/embeddings",
                        headers={"Authorization": f"Bearer {self._api_key}"},
                        json={"model": self._model, "input": [t[:8000] for t in uncached]},
                    )
                    resp.raise_for_status()
                    data = resp.json()["data"]
                    for i, item in enumerate(data):
                        self._cache[uncached[i][:500]] = item["embedding"]
            except Exception as e:
                logger.warning(f"Batch embedding error, using fallback: {e}")
                for t in uncached:
                    self._cache[t[:500]] = self._fallback_embedding(t)
        
        # Return embeddings in original order
        return [self._cache.get(t[:500], self._fallback_embedding(t)) for t in texts]
    
    def clear_cache(self):
        """Clear the embedding cache"""
        self._cache.clear()


def cosine_similarity(a: list[float], b: list[float]) -> float:
    a_np, b_np = np.array(a), np.array(b)
    dot = np.dot(a_np, b_np)
    norm_a = np.linalg.norm(a_np)
    norm_b = np.linalg.norm(b_np)
    if norm_a == 0 or norm_b == 0:
        return 0.0
    return float(dot / (norm_a * norm_b))
