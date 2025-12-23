import os
from dotenv import load_dotenv
from functools import lru_cache

load_dotenv()

class ConfigValidationError(Exception):
    pass

class Config:
    DEEPSEEK_API_KEY = os.getenv("DEEPSEEK_API_KEY")
    XAI_API_KEY = os.getenv("XAI_API_KEY")
    OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
    OPENAI_MODEL = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
    OPENAI_EMBEDDING_MODEL = os.getenv("OPENAI_EMBEDDING_MODEL", "text-embedding-3-small")
    
    API_GATEWAY_URL = os.getenv("API_GATEWAY_URL", "http://localhost:5000")
    PRODUCT_API_URL = os.getenv("PRODUCT_API_URL", "http://localhost:6002")
    BASKET_API_URL = os.getenv("BASKET_API_URL", "http://localhost:6004")
    ORDER_API_URL = os.getenv("ORDER_API_URL", "http://localhost:6005")
    CUSTOMER_API_URL = os.getenv("CUSTOMER_API_URL", "http://localhost:5005")
    
    REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
    REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))
    REDIS_DB = int(os.getenv("REDIS_DB", "1"))
    REDIS_PASSWORD = os.getenv("REDIS_PASSWORD")
    REDIS_ENABLED = os.getenv("REDIS_ENABLED", "false").lower() == "true"
    
    APP_NAME = os.getenv("APP_NAME", "E-commerce Chatbot Service")
    APP_VERSION = os.getenv("APP_VERSION", "4.0.0")
    LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")
    
    BROWSER_HEADLESS = os.getenv("BROWSER_HEADLESS", "true").lower() == "true"
    HTTP_PORT = int(os.getenv("HTTP_PORT", "8001"))
    
    def validate(self):
        required_keys = ["OPENAI_API_KEY"]
        missing = [key for key in required_keys if not getattr(self, key)]
        if missing:
            raise ConfigValidationError(f"Missing required environment variables: {', '.join(missing)}")

@lru_cache()
def get_config() -> Config:
    cfg = Config()
    cfg.validate()
    return cfg

config = get_config()