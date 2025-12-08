import grpc
from concurrent import futures
import asyncio
import logging
import json
import sys
import os
import uvicorn
from fastapi import FastAPI, WebSocket
from fastapi.middleware.cors import CORSMiddleware

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from config import config
from protos import mcp_pb2, mcp_pb2_grpc
from playwright_server import get_playwright_mcp_server
from api_server import get_api_tools_server
from websocket_handler import websocket_endpoint, manager as ws_manager

logging.basicConfig(level=getattr(logging, config.LOG_LEVEL))
logger = logging.getLogger(__name__)


# FastAPI app for WebSocket
app = FastAPI(title="MCP WebSocket Server")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify exact origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.websocket("/ws/mcp")
async def websocket_mcp_endpoint(websocket: WebSocket, userId: str):
    await websocket_endpoint(websocket, userId)


@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "service": "MCP Service",
        "active_connections": len(ws_manager.active_connections)
    }


class MCPServicer(mcp_pb2_grpc.MCPServiceServicer):
    
    async def GetRelevantTools(self, request, context):
        try:
            # Get tools from both servers
            playwright_server = await get_playwright_mcp_server()
            api_server = await get_api_tools_server()
            
            # Extract auth token
            auth_token = request.auth_token if hasattr(request, 'auth_token') else None
            
            # Discover browser tools
            browser_result = await playwright_server.discover_tools(request.user_id, request.query)
            
            # Discover API tools (pass token for context)
            api_result = await api_server.discover_tools(request.user_id, request.query, auth_token)
            
            # Combine tools
            all_tools = []
            
            # Add browser tools
            for tool_data in browser_result.get("tools", []):
                all_tools.append(mcp_pb2.Tool(
                    name=tool_data.get("name", ""),
                    description=tool_data.get("description", ""),
                    parameters_schema=json.dumps(tool_data.get("parameters", {}))
                ))
            
            # Add API tools
            for tool_data in api_result.get("tools", []):
                all_tools.append(mcp_pb2.Tool(
                    name=tool_data.get("name", ""),
                    description=tool_data.get("description", ""),
                    parameters_schema=json.dumps(tool_data.get("parameters", {}))
                ))
            
            # Combine context
            combined_context = {
                "browser": browser_result.get("context", {}),
                "api": api_result.get("context", {}),
                "total_tools": len(all_tools)
            }
            
            logger.info(f"Discovered {len(all_tools)} tools ({len(browser_result.get('tools', []))} browser + {len(api_result.get('tools', []))} API)")
            
            return mcp_pb2.GetToolsResponse(
                success=True,
                tools=all_tools,
                context=json.dumps(combined_context)
            )
        except Exception as e:
            logger.error(f"GetRelevantTools error: {e}")
            return mcp_pb2.GetToolsResponse(success=False, tools=[], context=str(e))
    
    async def ExecuteTool(self, request, context):
        try:
            arguments = json.loads(request.arguments_json) if request.arguments_json else {}
            tool_name = request.tool_name
            auth_token = request.auth_token if hasattr(request, 'auth_token') else None
            
            # Inject token into arguments if not present
            if auth_token and 'token' not in arguments:
                arguments['token'] = auth_token
            
            # Determine which server to use based on tool name
            # API tools: search_products, get_cart, etc.
            # Browser tools: browser_click, browser_fill, etc.
            
            if tool_name.startswith("browser_") or tool_name in ["click", "fill", "navigate", "scroll"]:
                # Execute browser tool
                server = await get_playwright_mcp_server()
                result = await server.execute_tool(
                    request.user_id,
                    tool_name,
                    arguments
                )
            else:
                # Execute API tool
                server = await get_api_tools_server()
                result = await server.execute_tool(
                    request.user_id,
                    tool_name,
                    arguments,
                    auth_token
                )
            
            return mcp_pb2.ExecuteToolResponse(
                success=result.get("success", False),
                result=json.dumps(result),
                error=result.get("error", "")
            )
        except Exception as e:
            logger.error(f"ExecuteTool error: {e}")
            return mcp_pb2.ExecuteToolResponse(
                success=False,
                result="",
                error=str(e)
            )


async def serve_grpc():
    server = grpc.aio.server(futures.ThreadPoolExecutor(max_workers=10))
    mcp_pb2_grpc.add_MCPServiceServicer_to_server(MCPServicer(), server)
    
    listen_addr = f"[::]:{config.GRPC_PORT}"
    server.add_insecure_port(listen_addr)
    
    logger.info(f"MCP gRPC Server starting on {listen_addr}")
    await server.start()
    
    try:
        await server.wait_for_termination()
    except KeyboardInterrupt:
        logger.info("Shutting down gRPC server...")
        await server.stop(5)


async def serve_http():
    http_port = config.HTTP_PORT if hasattr(config, 'HTTP_PORT') else 8001
    
    logger.info(f"MCP HTTP/WebSocket Server starting on port {http_port}")
    
    config_uvicorn = uvicorn.Config(
        app,
        host="0.0.0.0",
        port=http_port,
        log_level=config.LOG_LEVEL.lower()
    )
    server = uvicorn.Server(config_uvicorn)
    await server.serve()


async def main():
    logger.info("Starting MCP Service with gRPC and WebSocket support...")
    
    try:
        await asyncio.gather(
            serve_grpc(),
            serve_http()
        )
    except KeyboardInterrupt:
        logger.info("Shutting down MCP Service...")
    finally:
        # Cleanup
        mcp_server = await get_playwright_mcp_server()
        await mcp_server.shutdown()


if __name__ == "__main__":
    asyncio.run(main())

