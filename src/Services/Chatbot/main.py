from fastapi import FastAPI, WebSocket
from contextlib import asynccontextmanager
from fastapi.middleware.cors import CORSMiddleware
from routers.chat import router as chat_router
from routers.sessions import router as sessions_router
from websocket_mcp import handle_mcp_websocket
from config import config
import logging

# Configure logging
logging.basicConfig(
    level=getattr(logging, config.LOG_LEVEL),
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info(f"Starting {config.APP_NAME} v{config.APP_VERSION}")
    logger.info(f"Product API: {config.PRODUCT_API_URL}")
    logger.info(f"Basket API: {config.BASKET_API_URL}")
    logger.info(f"Order API: {config.ORDER_API_URL}")
    
    from utils.redis_manager import redis_manager
    await redis_manager.connect()

    yield

    await redis_manager.disconnect()
    logger.info(f"Shutting down {config.APP_NAME}")

app = FastAPI(
    title=config.APP_NAME,
    version=config.APP_VERSION,
    lifespan=lifespan,
    description="AI-powered chatbot for E-commerce platform"
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include routers
app.include_router(chat_router, prefix="/api/v1", tags=["chat"])
app.include_router(sessions_router, prefix="/api/v1", tags=["sessions"])


# ============== Standard Endpoints ==============
@app.get("/")
async def root():
    return {
        "message": config.APP_NAME,
        "version": config.APP_VERSION,
        "status": "running"
    }


@app.get("/health")
async def health_check():
    """Health check endpoint for Docker and monitoring"""
    return {
        "status": "healthy",
        "service": config.APP_NAME,
        "version": config.APP_VERSION
    }


# ============== WebSocket Endpoint ==============
@app.websocket("/ws/mcp")
async def websocket_mcp_endpoint(websocket: WebSocket, userId: str):
    """WebSocket endpoint for MCP UI state synchronization"""
    await handle_mcp_websocket(websocket, userId)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=80)