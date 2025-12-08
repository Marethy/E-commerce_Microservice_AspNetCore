import asyncio
import logging
from typing import Dict, Set, Optional
from fastapi import WebSocket, WebSocketDisconnect
from dataclasses import dataclass, asdict
from datetime import datetime

logger = logging.getLogger(__name__)


@dataclass
class UIState:
    """Client UI state information"""
    url: str
    title: str
    elements: list
    timestamp: str


class ConnectionManager:
    """Manages WebSocket connections for multiple users"""
    
    def __init__(self):
        # Map of user_id -> WebSocket connections
        self.active_connections: Dict[str, Set[WebSocket]] = {}
        # Map of user_id -> latest UI state
        self.ui_states: Dict[str, UIState] = {}
        # Pending action results
        self.action_results: Dict[str, asyncio.Future] = {}
    
    async def connect(self, websocket: WebSocket, user_id: str):
        """Accept and register a new WebSocket connection"""
        await websocket.accept()
        
        if user_id not in self.active_connections:
            self.active_connections[user_id] = set()
        
        self.active_connections[user_id].add(websocket)
        logger.info(f"WebSocket connected for user {user_id}. Total connections: {len(self.active_connections[user_id])}")
    
    def disconnect(self, websocket: WebSocket, user_id: str):
        """Remove a WebSocket connection"""
        if user_id in self.active_connections:
            self.active_connections[user_id].discard(websocket)
            if not self.active_connections[user_id]:
                del self.active_connections[user_id]
                # Clean up UI state when no connections left
                self.ui_states.pop(user_id, None)
        
        logger.info(f"WebSocket disconnected for user {user_id}")
    
    async def send_message(self, user_id: str, message: dict):
        """Send message to all connections for a user"""
        if user_id not in self.active_connections:
            logger.warning(f"No active connections for user {user_id}")
            return False
        
        disconnected = set()
        for connection in self.active_connections[user_id]:
            try:
                await connection.send_json(message)
            except Exception as e:
                logger.error(f"Failed to send message to {user_id}: {e}")
                disconnected.add(connection)
        
        # Clean up disconnected websockets
        for conn in disconnected:
            self.disconnect(conn, user_id)
        
        return len(self.active_connections.get(user_id, [])) > 0
    
    def update_ui_state(self, user_id: str, state: dict):
        """Update stored UI state for a user"""
        self.ui_states[user_id] = UIState(
            url=state.get("url", ""),
            title=state.get("title", ""),
            elements=state.get("elements", []),
            timestamp=datetime.now().isoformat()
        )
        logger.debug(f"Updated UI state for {user_id}: {state.get('url', 'N/A')}")
    
    def get_ui_state(self, user_id: str) -> Optional[dict]:
        """Get latest UI state for a user"""
        state = self.ui_states.get(user_id)
        if state:
            return asdict(state)
        return None
    
    def is_connected(self, user_id: str) -> bool:
        """Check if user has active connections"""
        return user_id in self.active_connections and len(self.active_connections[user_id]) > 0
    
    async def execute_action(self, user_id: str, action_id: str, action: dict, timeout: float = 30.0) -> dict:
        """
        Execute an action on the client and wait for result.
        Returns the action result or raises TimeoutError.
        """
        if not self.is_connected(user_id):
            raise ConnectionError(f"User {user_id} is not connected")
        
        # Create future for result
        future = asyncio.get_event_loop().create_future()
        self.action_results[action_id] = future
        
        # Send action to client
        message = {
            "type": "action",
            "id": action_id,
            "payload": action
        }
        
        success = await self.send_message(user_id, message)
        if not success:
            del self.action_results[action_id]
            raise ConnectionError(f"Failed to send action to user {user_id}")
        
        try:
            # Wait for result with timeout
            result = await asyncio.wait_for(future, timeout=timeout)
            return result
        except asyncio.TimeoutError:
            logger.error(f"Action {action_id} timed out for user {user_id}")
            raise TimeoutError(f"Action execution timed out after {timeout}s")
        finally:
            # Clean up
            self.action_results.pop(action_id, None)
    
    def set_action_result(self, action_id: str, result: dict):
        """Set the result of an action execution"""
        future = self.action_results.get(action_id)
        if future and not future.done():
            future.set_result(result)
            logger.debug(f"Action {action_id} completed with result: {result.get('success', False)}")


# Global connection manager instance
manager = ConnectionManager()


async def handle_client_message(user_id: str, data: dict):
    """Process messages received from client"""
    message_type = data.get("type")
    
    if message_type == "ui_state":
        # Client sending UI state update
        manager.update_ui_state(user_id, data.get("payload", {}))
    
    elif message_type == "action_result":
        # Client sending action execution result
        action_id = data.get("id")
        result = data.get("payload", {})
        if action_id:
            manager.set_action_result(action_id, result)
    
    elif message_type == "ping":
        # Heartbeat
        return {"type": "pong"}
    
    else:
        logger.warning(f"Unknown message type from {user_id}: {message_type}")
    
    return None


async def websocket_endpoint(websocket: WebSocket, user_id: str):
    """WebSocket endpoint handler"""
    await manager.connect(websocket, user_id)
    
    try:
        while True:
            # Receive message from client
            data = await websocket.receive_json()
            
            # Process message
            response = await handle_client_message(user_id, data)
            
            # Send response if needed
            if response:
                await websocket.send_json(response)
    
    except WebSocketDisconnect:
        logger.info(f"Client {user_id} disconnected normally")
    except Exception as e:
        logger.error(f"WebSocket error for {user_id}: {e}")
    finally:
        manager.disconnect(websocket, user_id)


# Export manager and endpoint
__all__ = ["manager", "websocket_endpoint", "ConnectionManager"]
