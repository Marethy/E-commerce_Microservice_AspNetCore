"""SQLAlchemy models for conversation storage"""
from datetime import datetime
from sqlalchemy import Column, String, Text, DateTime, Integer, ForeignKey, Boolean
from sqlalchemy.orm import relationship, declarative_base

Base = declarative_base()


class Session(Base):
    __tablename__ = "sessions"
    
    id = Column(String(100), primary_key=True)
    user_id = Column(String(100), nullable=True, index=True)
    title = Column(String(255), nullable=True)
    username = Column(String(100), nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    is_active = Column(Boolean, default=True)
    
    messages = relationship("Message", back_populates="session", cascade="all, delete-orphan", order_by="Message.created_at")
    
    def to_dict(self, message_count: int | None = None):
        return {
            "id": self.id,
            "user_id": self.user_id,
            "title": self.title or "New Conversation",
            "username": self.username,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None,
            "message_count": message_count if message_count is not None else 0,
        }


class Message(Base):
    __tablename__ = "messages"
    
    id = Column(Integer, primary_key=True, autoincrement=True)
    session_id = Column(String(100), ForeignKey("sessions.id", ondelete="CASCADE"), nullable=False)
    role = Column(String(20), nullable=False)
    content = Column(Text, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)
    
    tool_calls = Column(Text, nullable=True)
    mcp_action = Column(Text, nullable=True)
    
    session = relationship("Session", back_populates="messages")
    
    def to_dict(self):
        return {
            "id": self.id,
            "session_id": self.session_id,
            "role": self.role,
            "content": self.content,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "tool_calls": self.tool_calls,
            "mcp_action": self.mcp_action,
        }
