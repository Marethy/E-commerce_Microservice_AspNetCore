import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    # AI Configuration
    GROK_API_KEY = os.getenv("GROK_KEY")
    GROK_MODEL = os.getenv("GROK_MODEL", "grok-beta")
    GROK_BASE_URL = "https://api.x.ai/v1"
    
    # Service URLs (for inter-service communication)
    API_GATEWAY_URL = os.getenv("API_GATEWAY_URL", "http://localhost:5000")
    PRODUCT_API_URL = os.getenv("PRODUCT_API_URL", "http://localhost:6002")
    BASKET_API_URL = os.getenv("BASKET_API_URL", "http://localhost:6004")
    ORDER_API_URL = os.getenv("ORDER_API_URL", "http://localhost:6005")
    
    # Redis Configuration (for production session storage)
    REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
    REDIS_PORT = int(os.getenv("REDIS_PORT", "6379"))
    REDIS_DB = int(os.getenv("REDIS_DB", "1"))
    
    # Application Settings
    APP_NAME = os.getenv("APP_NAME", "E-commerce Chatbot Service")
    APP_VERSION = os.getenv("APP_VERSION", "1.0.0")
    LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")

config = Config()