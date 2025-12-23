from fastapi import APIRouter, HTTPException, BackgroundTasks
from fastapi.responses import StreamingResponse
from models.chat import ChatRequest
from utils.jwt_helper import extract_username, extract_user_id
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
    if not request.user_token:
        raise HTTPException(status_code=401, detail="Authentication required")
    
    user_id = extract_user_id(request.user_token)
    username = extract_username(request.user_token)
    
    if not user_id:
        raise HTTPException(status_code=401, detail="Invalid token")
    
    is_valid, error = db_service.validate_session_owner(request.session_id, user_id)
    if not is_valid:
        raise HTTPException(status_code=403, detail=error)
    
    try:
        history = _get_history(request.session_id)
        
        background_tasks.add_task(
            db_service.add_message, request.session_id, "user", request.message, user_id
        )
        history.append({"role": "user", "content": request.message})

        async def generate():
            content_parts = []
            tool_calls_log = []
            
            try:
                async for event in stream_chat(user_id, username, request.message, history[:-1], request.user_token, request.session_id):
                    yield f"data: {json.dumps(event, ensure_ascii=False)}\n\n"
                    
                    if event.get("type") == "executing":
                        tool_calls_log.append({"tool": event.get("tool"), "args": event.get("args")})
                    elif event.get("type") == "content":
                        content_parts.append(event.get("content", ""))
                
                yield f"data: {json.dumps({'type': 'done'})}\n\n"
                
                final_content = "".join(content_parts)
                if final_content:
                    background_tasks.add_task(
                        db_service.add_message,
                        request.session_id,
                        "assistant",
                        final_content,
                        user_id,
                        tool_calls=json.dumps(tool_calls_log, ensure_ascii=False) if tool_calls_log else None
                    )
                    history.append({"role": "assistant", "content": final_content})
                    
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