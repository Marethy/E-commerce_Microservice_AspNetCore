from pydantic import BaseModel
from typing import Optional

class ChatRequest(BaseModel):
    message: str
    session_id: str
    user_token: Optional[str] = None

class ChatResponse(BaseModel):
    reply: str
    suggested_url: Optional[str] = None