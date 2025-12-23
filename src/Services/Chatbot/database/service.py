import logging
import os
from datetime import datetime
from sqlalchemy import create_engine, desc, func
from sqlalchemy.orm import sessionmaker, Session as DBSession
from typing import Optional
from .models import Base, Session, Message

logger = logging.getLogger(__name__)

DB_PATH = os.environ.get("SQLITE_DB_PATH", "/app/data/chatbot.db")


class DatabaseService:
    def __init__(self, db_path: str = DB_PATH):
        db_dir = os.path.dirname(db_path)
        if db_dir and not os.path.exists(db_dir):
            os.makedirs(db_dir, exist_ok=True)
        
        self.engine = create_engine(f"sqlite:///{db_path}", echo=False)
        Base.metadata.create_all(self.engine)
        self.SessionLocal = sessionmaker(bind=self.engine)
        logger.info(f"Database initialized at {db_path}")
    
    def get_db(self) -> DBSession:
        return self.SessionLocal()
    
    # ============== SESSION OPERATIONS ==============
    
    def create_session(self, session_id: str, user_id: str, username: Optional[str] = None) -> Session:
        if not session_id or not user_id:
            raise ValueError("Session ID and user ID are required")
        db = self.get_db()
        try:
            session = Session(
                id=session_id,
                user_id=user_id,
                username=username,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            db.add(session)
            db.commit()
            db.refresh(session)
            logger.info(f"Created session: {session_id} for user: {user_id}")
            return session
        finally:
            db.close()
    
    def get_session(self, session_id: str) -> Optional[Session]:
        db = self.get_db()
        try:
            return db.query(Session).filter(Session.id == session_id).first()
        finally:
            db.close()
    
    def get_or_create_session(self, session_id: str, user_id: str, username: Optional[str] = None) -> Session:
        session = self.get_session(session_id)
        if not session_id or not user_id:
            raise ValueError("User ID are required")
        if not session:
            session = self.create_session(session_id, user_id, username)
        return session
    
    def validate_session_owner(self, session_id: str, user_id: str) -> tuple[bool, Optional[str]]:
        if not session_id or not user_id:
            return False, "User ID and session ID are required"
        session = self.get_session(session_id)
        if not session:
            return False, "Session not found"
        if session.user_id and user_id and session.user_id != user_id:
            return False, "Session does not belong to this user"
        return True, None
    
    def list_sessions(self, user_id: str, limit: int = 50) -> list[dict]:
        """List all sessions with at least one message, filtered by user_id"""
        if not user_id:
            return []
        db = self.get_db()
        try:
            query = db.query(Session).join(Message, Session.id == Message.session_id)
            query = query.filter(Session.is_active == True)
            query = query.filter(Session.user_id == user_id)
            query = query.group_by(Session.id)
            query = query.having(func.count(Message.id) > 0)
            query = query.order_by(desc(Session.updated_at))
            
            sessions = query.limit(limit).all()
            return [s.to_dict() for s in sessions]
        finally:
            db.close()
    
    def update_session_title(self, session_id: str, title: str) -> bool:
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
        user_id: str,
        tool_calls: Optional[str] = None,
        mcp_action: Optional[str] = None
    ) -> Message:
        db = self.get_db()
        try:
            session = db.query(Session).filter(Session.id == session_id).first()
            if not session:
                session = Session(id=session_id, user_id=user_id, created_at=datetime.utcnow())
                db.add(session)
                db.commit()
            
            message = Message(
                session_id=session_id,
                role=role,
                content=content,
                tool_calls=tool_calls,
                mcp_action=mcp_action,
                created_at=datetime.utcnow()
            )
            db.add(message)
            
            if role == "user" and not session.title:
                session.title = content[:50] + ("..." if len(content) > 50 else "")
            
            session.updated_at = datetime.utcnow()
            db.commit()
            db.refresh(message)
            logger.debug(f"Added message to session {session_id}: {role}")
            return message
        finally:
            db.close()
    
    def get_messages(self, session_id: str, limit: int = 10, before_timestamp: Optional[str] = None) -> list[dict]:
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
        db = self.get_db()
        try:
            messages = db.query(Message).filter(
                Message.session_id == session_id
            ).order_by(desc(Message.created_at)).limit(limit).all()
            return [m.to_dict() for m in reversed(messages)]
        finally:
            db.close()
    
    def get_message_count(self, session_id: str) -> int:
        """Get count of messages for a session"""
        db = self.get_db()
        try:
            count = db.query(func.count(Message.id)).filter(
                Message.session_id == session_id
            ).scalar()
            return count or 0
        finally:
            db.close()


db_service = DatabaseService()
