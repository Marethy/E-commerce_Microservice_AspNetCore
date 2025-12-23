import json
import logging
from langchain.agents import create_agent
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage, ToolMessage
from langchain_core.runnables import RunnableConfig
from config import config
from tools import get_tools

logger = logging.getLogger(__name__)

SYSTEM_PROMPT = """Bạn là trợ lý AI cho website thương mại điện tử.
Dùng get_elements để xem trang, click/fill để tương tác.
Dùng API tools để tìm sản phẩm, quản lý giỏ hàng, đơn hàng."""

model = ChatOpenAI(
    model=config.OPENAI_MODEL,
    temperature=1,
    timeout=60,
    api_key=config.OPENAI_API_KEY,
)

agent = create_agent(
    model=model,
    tools=get_tools()
)


def _build_messages(message: str, history: list[dict]) -> list:
    messages = [SystemMessage(content=SYSTEM_PROMPT)]
    
    for msg in history:
        role = msg.get("role")
        content = msg.get("content", "")
        
        if role == "user" and content:
            messages.append(HumanMessage(content=content))
        elif role == "assistant":
            tool_calls = msg.get("tool_calls")
            if tool_calls:
                lc_tool_calls = [{
                    "name": tc["function"]["name"],
                    "args": json.loads(tc["function"]["arguments"]) if isinstance(tc["function"]["arguments"], str) else tc["function"]["arguments"],
                    "id": tc["id"]
                } for tc in tool_calls]
                messages.append(AIMessage(content=content or "", tool_calls=lc_tool_calls))
            elif content:
                messages.append(AIMessage(content=content))
        elif role == "tool" and content:
            messages.append(ToolMessage(content=content, tool_call_id=msg.get("tool_call_id", "")))
    
    messages.append(HumanMessage(content=message))
    return messages


async def stream_chat(user_id: str, username: str, message: str, history: list[dict], auth_token: str = None, session_id: str = None):
    messages = _build_messages(message, history)
    run_config = RunnableConfig(configurable={"user_id": user_id, "username": username, "auth_token": auth_token, "session_id": session_id})
    logger.info(f"stream_chat - user_id={user_id}, username={username}, session_id={session_id}")
    pending_tool_calls = {}
    
    try:
        async for token, metadata in agent.astream(
            {"messages": messages},
            config=run_config,
            stream_mode="messages",
        ):
            if hasattr(token, 'content') and token.content and not isinstance(token, ToolMessage):
                yield {"type": "content", "content": token.content}
            
            if hasattr(token, 'tool_calls') and token.tool_calls:
                for tool_call in token.tool_calls:
                    if tool_call.get("name"):
                        logger.info(tool_call)
                        tool_id = tool_call.get("id", "")
                        pending_tool_calls[tool_id] = tool_call["name"]
                        yield {"type": "executing", "tool": tool_call["name"], "args": tool_call.get("args", {})}
            
            if isinstance(token, ToolMessage):
                tool_id = token.tool_call_id
                if tool_id in pending_tool_calls:
                    try:
                        result = json.loads(token.content)
                        logger.info(result)
                        success = result.get("isSuccess", result.get("success", False))
                    except:
                        success = False
                    yield {"type": "executed", "tool": pending_tool_calls[tool_id], "success": success}
                    del pending_tool_calls[tool_id]
    
    except Exception as e:
        logger.error(f"Error in stream_chat: {e}", exc_info=True)
        yield {"type": "error", "content": str(e)}
