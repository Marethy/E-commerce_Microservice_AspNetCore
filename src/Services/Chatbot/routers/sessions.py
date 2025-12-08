from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import Optional
from database import db_service
import logging

router = APIRouter()
logger = logging.getLogger(__name__)


class CreateSessionRequest(BaseModel):
    username: Optional[str] = None


class CreateSessionResponse(BaseModel):
    session_id: str
    title: str
    created_at: str


class UpdateSessionRequest(BaseModel):
    title: str


# ============== SESSION ENDPOINTS ==============

@router.post("/sessions", response_model=CreateSessionResponse)
async def create_session(request: CreateSessionRequest = None):
    """Create a new chat session - session ID generated server-side"""
    import time
    import random
    import string
    
    # Generate session ID server-side
    random_suffix = ''.join(random.choices(string.ascii_lowercase + string.digits, k=10))
    session_id = f"session_{int(time.time() * 1000)}_{random_suffix}"
    
    username = request.username if request else None
    session = db_service.create_session(session_id, username)
    
    return CreateSessionResponse(
        session_id=session.id,
        title=session.title or "New Conversation",
        created_at=session.created_at.isoformat()
    )


@router.get("/sessions")
async def list_sessions(username: Optional[str] = None, limit: int = 50):
    """List all chat sessions"""
    sessions = db_service.list_sessions(username=username, limit=limit)
    return {"sessions": sessions, "count": len(sessions)}


@router.get("/sessions/{session_id}")
async def get_session(session_id: str):
    """Get session details"""
    session = db_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found")
    return session.to_dict()


@router.get("/sessions/{session_id}/messages")
async def get_session_messages(session_id: str, limit: int = 10, before: Optional[str] = None):
    """Get messages paginated (default 5 user-assistant pairs)"""
    session = db_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found")
    
    messages = db_service.get_messages(session_id, limit=limit, before_timestamp=before)
    return {
        "session": session.to_dict(),
        "messages": messages,
        "count": len(messages)
    }


@router.patch("/sessions/{session_id}")
async def update_session(session_id: str, request: UpdateSessionRequest):
    """Update session title"""
    success = db_service.update_session_title(session_id, request.title)
    if not success:
        raise HTTPException(status_code=404, detail="Session not found")
    return {"success": True, "title": request.title}


@router.delete("/sessions/{session_id}")
async def delete_session(session_id: str):
    """Delete a session (soft delete)"""
    success = db_service.delete_session(session_id)
    if not success:
        raise HTTPException(status_code=404, detail="Session not found")
    return {"success": True}
