import os
from dotenv import load_dotenv
from functools import lru_cache

load_dotenv()

class Config:
    # OpenAI Configuration (for both LLM and Embeddings)
    DEEPSEEK_API_KEY = os.getenv("DEEPSEEK_API_KEY")
    XAI_API_KEY = os.getenv("XAI_API_KEY")
    OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
    OPENAI_MODEL = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
    OPENAI_EMBEDDING_MODEL = os.getenv("OPENAI_EMBEDDING_MODEL", "text-embedding-3-small")
    
    # API URLs
    API_GATEWAY_URL = os.getenv("API_GATEWAY_URL", "http://localhost:5000")
    PRODUCT_API_URL = os.getenv("PRODUCT_API_URL", "http://localhost:6002")
    BASKET_API_URL = os.getenv("BASKET_API_URL", "http://localhost:6004")
    ORDER_API_URL = os.getenv("ORDER_API_URL", "http://localhost:6005")
    
    # Redis
    REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
    REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))
    REDIS_DB = int(os.getenv("REDIS_DB", "1"))
    
    # App
    APP_NAME = os.getenv("APP_NAME", "E-commerce Chatbot MCP Service")
    APP_VERSION = os.getenv("APP_VERSION", "3.0.0")
    LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")
    
    # MCP gRPC
    MCP_GRPC_URL = os.getenv("MCP_GRPC_URL", "localhost:50051")

config = Config()