from fastapi import APIRouter, HTTPException, Header
from pydantic import BaseModel
from typing import Optional
from database import db_service
from utils.jwt_helper import extract_username, extract_user_id
import logging
import time
import random
import string

router = APIRouter()
logger = logging.getLogger(__name__)


class CreateSessionRequest(BaseModel):
    user_token: str


class CreateSessionResponse(BaseModel):
    session_id: str
    title: str
    created_at: str


class UpdateSessionRequest(BaseModel):
    title: str


@router.post("/sessions", response_model=CreateSessionResponse)
async def create_session(request: CreateSessionRequest):
    user_id = extract_user_id(request.user_token)
    username = extract_username(request.user_token)
    
    if not user_id:
        raise HTTPException(status_code=401, detail="Invalid token")
    
    random_suffix = ''.join(random.choices(string.ascii_lowercase + string.digits, k=10))
    session_id = f"session_{int(time.time() * 1000)}_{random_suffix}"
    
    session = db_service.create_session(session_id, user_id, username)
    
    return CreateSessionResponse(
        session_id=session.id,
        title=session.title or "New Conversation",
        created_at=session.created_at.isoformat()
    )


@router.get("/sessions")
async def list_sessions(authorization: Optional[str] = Header(None), limit: int = 50):
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="Authentication required")
    
    token = authorization.replace("Bearer ", "")
    user_id = extract_user_id(token)
    
    if not user_id:
        raise HTTPException(status_code=401, detail="Invalid token")
    
    sessions = db_service.list_sessions(user_id=user_id, limit=limit)
    return {"sessions": sessions, "count": len(sessions)}


def _extract_user_id_from_header(authorization: Optional[str]) -> str:
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="Authentication required")
    token = authorization.replace("Bearer ", "")
    user_id = extract_user_id(token)
    if not user_id:
        raise HTTPException(status_code=401, detail="Invalid token")
    return user_id


@router.get("/sessions/{session_id}")
async def get_session(session_id: str, authorization: Optional[str] = Header(None)):
    user_id = _extract_user_id_from_header(authorization)
    
    is_valid, error = db_service.validate_session_owner(session_id, user_id)
    if not is_valid:
        raise HTTPException(status_code=403, detail=error)
    
    session = db_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found")
    
    message_count = db_service.get_message_count(session_id)
    return session.to_dict(message_count=message_count)


@router.get("/sessions/{session_id}/messages")
async def get_session_messages(session_id: str, authorization: Optional[str] = Header(None), limit: int = 100, before: Optional[str] = None):
    user_id = _extract_user_id_from_header(authorization)
    
    is_valid, error = db_service.validate_session_owner(session_id, user_id)
    if not is_valid:
        raise HTTPException(status_code=403, detail=error)
    
    session = db_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found")
    
    messages = db_service.get_messages(session_id, limit=limit, before_timestamp=before)
    
    message_count = db_service.get_message_count(session_id)
    
    return {
        "session": session.to_dict(message_count=message_count),
        "messages": messages,
        "count": len(messages)
    }


@router.patch("/sessions/{session_id}")
async def update_session(session_id: str, request: UpdateSessionRequest, authorization: Optional[str] = Header(None)):
    user_id = _extract_user_id_from_header(authorization)
    
    is_valid, error = db_service.validate_session_owner(session_id, user_id)
    if not is_valid:
        raise HTTPException(status_code=403, detail=error)
    
    success = db_service.update_session_title(session_id, request.title)
    if not success:
        raise HTTPException(status_code=404, detail="Session not found")
    return {"success": True, "title": request.title}


@router.delete("/sessions/{session_id}")
async def delete_session(session_id: str, authorization: Optional[str] = Header(None)):
    user_id = _extract_user_id_from_header(authorization)
    
    is_valid, error = db_service.validate_session_owner(session_id, user_id)
    if not is_valid:
        raise HTTPException(status_code=403, detail=error)
    
    success = db_service.delete_session(session_id)
    if not success:
        raise HTTPException(status_code=404, detail="Session not found")
    return {"success": True}
