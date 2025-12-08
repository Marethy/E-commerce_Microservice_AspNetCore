"""SQLAlchemy models for conversation storage"""
from datetime import datetime
from sqlalchemy import Column, String, Text, DateTime, Integer, ForeignKey, Boolean
from sqlalchemy.orm import relationship, declarative_base

Base = declarative_base()


class Session(Base):
    """Chat session / conversation"""
    __tablename__ = "sessions"
    
    id = Column(String(100), primary_key=True)  # session_xxx format
    title = Column(String(255), nullable=True)  # Auto-generated from first message
    username = Column(String(100), nullable=True)  # If logged in
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    is_active = Column(Boolean, default=True)
    
    # Relationship
    messages = relationship("Message", back_populates="session", cascade="all, delete-orphan", order_by="Message.created_at")
    
    def to_dict(self):
        return {
            "id": self.id,
            "title": self.title or "New Conversation",
            "username": self.username,
            "created_at": self.created_at.isoformat() if self.created_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None,
            "message_count": len(self.messages) if self.messages else 0,
        }


class Message(Base):
    """Individual message in a conversation"""
    __tablename__ = "messages"
    
    id = Column(Integer, primary_key=True, autoincrement=True)
    session_id = Column(String(100), ForeignKey("sessions.id", ondelete="CASCADE"), nullable=False)
    role = Column(String(20), nullable=False)  # "user" or "assistant"
    content = Column(Text, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)
    
    # Metadata
    tool_calls = Column(Text, nullable=True)  # JSON string of tool calls if any
    mcp_action = Column(Text, nullable=True)  # MCP action performed if any
    
    # Relationship
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
