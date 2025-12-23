import redis.asyncio as redis
import json
import logging
from typing import Optional, Dict, Any
from config import config

logger = logging.getLogger(__name__)

class RedisManager:
    def __init__(self):
        self.client: Optional[redis.Redis] = None
        self.enabled = config.REDIS_ENABLED
    
    async def connect(self):
        if not self.enabled:
            logger.info("Redis disabled, using in-memory storage")
            return
        
        try:
            self.client = await redis.Redis(
                host=config.REDIS_HOST,
                port=config.REDIS_PORT,
                db=config.REDIS_DB,
                password=config.REDIS_PASSWORD,
                decode_responses=True
            )
            await self.client.ping()
            logger.info(f"Redis connected: {config.REDIS_HOST}:{config.REDIS_PORT}")
        except Exception as e:
            logger.error(f"Redis connection failed: {e}")
            self.enabled = False
    
    async def disconnect(self):
        if self.client:
            await self.client.close()
    
    async def set_state(self, key: str, value: Dict[str, Any], ttl: int = 3600):
        if not self.enabled or not self.client:
            return
        try:
            await self.client.setex(key, ttl, json.dumps(value))
        except Exception as e:
            logger.error(f"Redis set error: {e}")
    
    async def get_state(self, key: str) -> Optional[Dict[str, Any]]:
        if not self.enabled or not self.client:
            return None
        try:
            data = await self.client.get(key)
            return json.loads(data) if data else None
        except Exception as e:
            logger.error(f"Redis get error: {e}")
            return None
    
    async def delete_state(self, key: str):
        if not self.enabled or not self.client:
            return
        try:
            await self.client.delete(key)
        except Exception as e:
            logger.error(f"Redis delete error: {e}")

redis_manager = RedisManager()
