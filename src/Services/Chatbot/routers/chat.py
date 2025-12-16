from fastapi import APIRouter, HTTPException, BackgroundTasks
from fastapi.responses import StreamingResponse
from models.chat import ChatRequest
from utils.jwt_helper import extract_username
from database import db_service
from agent import stream_chat
import json
import logging

router = APIRouter()
logger = logging.getLogger(__name__)

chat_sessions: dict[str, list[dict]] = {}


def _get_history(session_id: str) -> list[dict]:
    if session_id not in chat_sessions:
        db_messages = db_service.get_recent_messages(session_id, limit=20)
        chat_sessions[session_id] = [
            {"role": msg["role"], "content": msg["content"]}
            for msg in db_messages
        ]
    return chat_sessions[session_id]


@router.post("/chat")
async def chat_endpoint(request: ChatRequest, background_tasks: BackgroundTasks):
    try:
        history = _get_history(request.session_id)
        
        # For MCP calls (browser automation), always use session_id
        # This must match the userId used in WebSocket connection
        mcp_user_id = request.session_id
        
        # For API calls (if needed), use username if authenticated
        user_id = request.session_id
        if request.user_token:
            username = extract_username(request.user_token)
            if username:
                user_id = username
        
        background_tasks.add_task(db_service.add_message, request.session_id, "user", request.message)
        history.append({"role": "user", "content": request.message})

        async def generate():
            step_messages = []
            
            try:
                # Pass mcp_user_id (session_id) to agent for browser automation
                async for event in stream_chat(mcp_user_id, request.message, history[:-1], request.user_token):
                    yield f"data: {json.dumps(event, ensure_ascii=False)}\n\n"
                    
                    if event.get("type") == "thinking":
                        step_messages.append({
                            "role": "assistant",
                            "content": event.get("content", ""),
                            "step_type": "thinking"
                        })
                    elif event.get("type") == "searching":
                        step_messages.append({
                            "role": "assistant", 
                            "content": event.get("content", ""),
                            "step_type": "searching"
                        })
                    elif event.get("type") == "executing":
                        step_messages.append({
                            "role": "assistant",
                            "content": f"Executing: {event.get('tool')}",
                            "step_type": "executing",
                            "tool_name": event.get("tool"),
                            "tool_args": event.get("args")
                        })
                    elif event.get("type") == "executed":
                        step_messages.append({
                            "role": "assistant",
                            "content": event.get("result", "") if event.get("success") else f"Error: {event.get('error')}",
                            "step_type": "executed",
                            "tool_name": event.get("tool"),
                            "success": event.get("success")
                        })
                    elif event.get("type") == "content":
                        step_messages.append({
                            "role": "assistant",
                            "content": event.get("content", ""),
                            "step_type": "response"
                        })
                
                yield f"data: {json.dumps({'type': 'done'})}\n\n"
                
                for step_msg in step_messages:
                    step_type = step_msg.pop("step_type")
                    metadata = {k: v for k, v in step_msg.items() if k not in ["role", "content"]}
                    
                    background_tasks.add_task(
                        db_service.add_message,
                        request.session_id,
                        "assistant",
                        step_msg["content"],
                        tool_calls=json.dumps({"step_type": step_type, **metadata}, ensure_ascii=False) if metadata or step_type != "response" else None
                    )
                
                final_response = next((m["content"] for m in reversed(step_messages) if m.get("step_type") == "response"), "")
                if final_response:
                    history.append({"role": "assistant", "content": final_response})
            except Exception as e:
                logger.exception("Error in chat stream")
                yield f"data: {json.dumps({'type': 'error', 'content': str(e)})}\n\n"

        return StreamingResponse(
            generate(),
            media_type="text/event-stream",
            headers={"Cache-Control": "no-cache", "Connection": "keep-alive"}
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.delete("/chat/{session_id}")
async def clear_session(session_id: str):
    chat_sessions.pop(session_id, None)
    return {"message": "Session cleared"}