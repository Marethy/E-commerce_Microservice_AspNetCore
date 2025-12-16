import grpc
import json
import logging
from typing import Optional
from config import config

from protos import mcp_pb2, mcp_pb2_grpc

logger = logging.getLogger(__name__)

_channel: Optional[grpc.aio.Channel] = None
_stub = None


async def get_mcp_stub():
    global _channel, _stub
    if _stub is None:
        _channel = grpc.aio.insecure_channel(config.MCP_GRPC_URL)
        _stub = mcp_pb2_grpc.MCPServiceStub(_channel)
    return _stub


async def get_relevant_tools(user_id: str, query: str, auth_token: str = None) -> dict:
    try:
        stub = await get_mcp_stub()
        response = await stub.GetRelevantTools(
            mcp_pb2.GetToolsRequest(user_id=user_id, query=query, auth_token=auth_token or "")
        )
        
        tools = []
        for tool in response.tools:
            tools.append({
                "name": tool.name,
                "description": tool.description,
                "parameters": json.loads(tool.parameters_schema) if tool.parameters_schema else {}
            })
        
        # Parse context safely
        context = {}
        if response.context:
            try:
                context = json.loads(response.context)
            except json.JSONDecodeError:
                logger.warning(f"Failed to parse context: {response.context}")
        
        return {
            "success": response.success,
            "tools": tools,
            "context": context
        }
    except Exception as e:
        logger.error(f"get_relevant_tools error: {e}")
        return {"success": False, "tools": [], "error": str(e)}


async def execute_tool(user_id: str, tool_name: str, arguments: dict, auth_token: str = None) -> dict:
    try:
        stub = await get_mcp_stub()
        response = await stub.ExecuteTool(
            mcp_pb2.ExecuteToolRequest(
                user_id=user_id,
                tool_name=tool_name,
                arguments_json=json.dumps(arguments),
                auth_token=auth_token or ""
            )
        )
        
        result = json.loads(response.result) if response.result else {}
        return {
            "success": response.success,
            "result": result,
            "error": response.error
        }
    except Exception as e:
        logger.error(f"execute_tool error: {e}")
        return {"success": False, "result": {}, "error": str(e)}


async def get_page_elements(user_id: str, auth_token: str = None) -> dict:
    try:
        stub = await get_mcp_stub()
        response = await stub.GetPageElements(
            mcp_pb2.GetPageElementsRequest(user_id=user_id, auth_token=auth_token or "")
        )
        
        elements = []
        for el in response.elements:
            elements.append({
                "id": el.id,
                "type": el.type,
                "description": el.description,
                "text": el.text,
                "context": el.context
            })
        
        return {
            "success": response.success,
            "elements": elements,
            "current_url": response.current_url,
            "page_title": response.page_title,
            "error": response.error
        }
    except Exception as e:
        logger.error(f"get_page_elements error: {e}")
        return {"success": False, "elements": [], "error": str(e)}


async def close():
    global _channel, _stub
    if _channel:
        await _channel.close()
        _channel = None
        _stub = None
