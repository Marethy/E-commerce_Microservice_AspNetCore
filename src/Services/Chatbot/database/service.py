"""Database service for conversation storage"""
import logging
import os
from datetime import datetime
from sqlalchemy import create_engine, desc
from sqlalchemy.orm import sessionmaker, Session as DBSession
from typing import Optional
from .models import Base, Session, Message

logger = logging.getLogger(__name__)

# Database path - use /app/data in Docker, local otherwise
DB_PATH = os.environ.get("SQLITE_DB_PATH", "/app/data/chatbot.db")


class DatabaseService:
    """Service for managing conversation persistence"""
    
    def __init__(self, db_path: str = DB_PATH):
        # Ensure directory exists
        db_dir = os.path.dirname(db_path)
        if db_dir and not os.path.exists(db_dir):
            os.makedirs(db_dir, exist_ok=True)
        
        self.engine = create_engine(f"sqlite:///{db_path}", echo=False)
        Base.metadata.create_all(self.engine)
        self.SessionLocal = sessionmaker(bind=self.engine)
        logger.info(f"Database initialized at {db_path}")
    
    def get_db(self) -> DBSession:
        """Get database session"""
        return self.SessionLocal()
    
    # ============== SESSION OPERATIONS ==============
    
    def create_session(self, session_id: str, username: Optional[str] = None) -> Session:
        """Create a new chat session"""
        db = self.get_db()
        try:
            session = Session(
                id=session_id,
                username=username,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            db.add(session)
            db.commit()
            db.refresh(session)
            logger.info(f"Created session: {session_id}")
            return session
        finally:
            db.close()
    
    def get_session(self, session_id: str) -> Optional[Session]:
        """Get session by ID"""
        db = self.get_db()
        try:
            return db.query(Session).filter(Session.id == session_id).first()
        finally:
            db.close()
    
    def get_or_create_session(self, session_id: str, username: Optional[str] = None) -> Session:
        """Get existing session or create new one"""
        session = self.get_session(session_id)
        if not session:
            session = self.create_session(session_id, username)
        return session
    
    def list_sessions(self, username: Optional[str] = None, limit: int = 50) -> list[dict]:
        """List all sessions, optionally filtered by username"""
        db = self.get_db()
        try:
            query = db.query(Session).filter(Session.is_active == True)
            if username:
                query = query.filter(Session.username == username)
            sessions = query.order_by(desc(Session.updated_at)).limit(limit).all()
            return [s.to_dict() for s in sessions]
        finally:
            db.close()
    
    def update_session_title(self, session_id: str, title: str) -> bool:
        """Update session title"""
        db = self.get_db()
        try:
            session = db.query(Session).filter(Session.id == session_id).first()
            if session:
                session.title = title[:255]  # Limit title length
                session.updated_at = datetime.utcnow()
                db.commit()
                return True
            return False
        finally:
            db.close()
    
    def delete_session(self, session_id: str) -> bool:
        """Soft delete session (mark as inactive)"""
        db = self.get_db()
        try:
            session = db.query(Session).filter(Session.id == session_id).first()
            if session:
                session.is_active = False
                db.commit()
                return True
            return False
        finally:
            db.close()
    
    # ============== MESSAGE OPERATIONS ==============
    
    def add_message(
        self,
        session_id: str,
        role: str,
        content: str,
        tool_calls: Optional[str] = None,
        mcp_action: Optional[str] = None
    ) -> Message:
        """Add a message to a session"""
        db = self.get_db()
        try:
            # Ensure session exists
            session = db.query(Session).filter(Session.id == session_id).first()
            if not session:
                # Create session if not exists
                session = Session(id=session_id, created_at=datetime.utcnow())
                db.add(session)
                db.commit()
            
            # Create message
            message = Message(
                session_id=session_id,
                role=role,
                content=content,
                tool_calls=tool_calls,
                mcp_action=mcp_action,
                created_at=datetime.utcnow()
            )
            db.add(message)
            
            # Update session title if first user message
            if role == "user" and not session.title:
                # Use first 50 chars of first message as title
                session.title = content[:50] + ("..." if len(content) > 50 else "")
            
            session.updated_at = datetime.utcnow()
            db.commit()
            db.refresh(message)
            logger.debug(f"Added message to session {session_id}: {role}")
            return message
        finally:
            db.close()
    
    def get_messages(self, session_id: str, limit: int = 10, before_timestamp: Optional[str] = None) -> list[dict]:
        """Get messages paginated by user message pairs (5 user-assistant pairs = 10 messages)"""
        db = self.get_db()
        try:
            query = db.query(Message).filter(Message.session_id == session_id)
            
            if before_timestamp:
                from datetime import datetime
                before_dt = datetime.fromisoformat(before_timestamp)
                query = query.filter(Message.created_at < before_dt)
            
            messages = query.order_by(desc(Message.created_at)).limit(limit).all()
            return [m.to_dict() for m in reversed(messages)]
        finally:
            db.close()
    
    def get_recent_messages(self, session_id: str, limit: int = 20) -> list[dict]:
        """Get most recent messages for context"""
        db = self.get_db()
        try:
            messages = db.query(Message).filter(
                Message.session_id == session_id
            ).order_by(desc(Message.created_at)).limit(limit).all()
            # Reverse to get chronological order
            return [m.to_dict() for m in reversed(messages)]
        finally:
            db.close()


# Global instance
db_service = DatabaseService()
