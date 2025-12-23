import json
import asyncio
import logging
from typing import Optional, Dict, Any, List
from fastapi import WebSocket, WebSocketDisconnect
from utils.redis_manager import redis_manager

logger = logging.getLogger(__name__)


class UIStateManager:
    def organize_elements(self, ui_state: dict) -> dict:
        elements = ui_state.get("elements", [])
        
        organized = {
            "url": ui_state.get("url"),
            "title": ui_state.get("title"),
            "timestamp": ui_state.get("timestamp"),
            "actions": {
                "click": [],
                "type": [],
                "scroll": {
                    "can_scroll_up": False,
                    "can_scroll_down": False
                }
            },
            "raw_elements": elements
        }
        
        for el in elements:
            el_type = el.get("type", "")
            
            display_text = self._extract_element_text(el)
            
            metadata = {
                "tag": el_type,
                "id": el.get("id"),
                "classes": el.get("classes", []),
                "attributes": el.get("attributes", {}),
                "visible_text": display_text,
                "is_interactive": el.get("is_interactive", True),
                "index": el.get("index", 0)
            }
            
            if el_type in ["button", "a", "submit", "link"]:
                organized["actions"]["click"].append({
                    "element_id": el.get("element_id"),
                    "stable_id": el.get("stable_id", el.get("id")),
                    "selector": el.get("selector"),
                    "text": display_text,
                    "tag": el_type,
                    "metadata": metadata,
                    "interaction_data": {
                        "clickable": True,
                        "has_text": bool(display_text),
                        "has_title": "title" in el and bool(el.get("title")),
                        "has_aria_label": "aria_label" in el and bool(el.get("aria_label"))
                    }
                })
            elif el_type in ["input", "textarea", "select", "search", "email", "password", "number", "tel"]:
                input_type = el.get("input_type", "text")
                placeholder = el.get("placeholder", "")
                
                organized["actions"]["type"].append({
                    "element_id": el.get("element_id"),
                    "stable_id": el.get("stable_id", el.get("id")),
                    "selector": el.get("selector"),
                    "text": display_text or placeholder,
                    "placeholder": placeholder,
                    "tag": el_type,
                    "input_type": input_type,
                    "value": el.get("value", ""),
                    "metadata": metadata,
                    "interaction_data": {
                        "typeable": True,
                        "has_value": bool(el.get("value")),
                        "has_placeholder": bool(placeholder),
                        "required": el.get("required", False)
                    }
                })
            elif el_type in ["div", "span", "li", "td", "section"]:
                if el.get("is_clickable", False):
                    organized["actions"]["click"].append({
                        "element_id": el.get("element_id"),
                        "stable_id": el.get("stable_id", el.get("id")),
                        "selector": el.get("selector"),
                        "text": display_text,
                        "tag": el_type,
                        "metadata": metadata,
                        "interaction_data": {
                            "clickable": True,
                            "has_text": bool(display_text),
                            "is_custom_element": True
                        }
                    })
        
        scroll_info = ui_state.get("scroll", {})
        organized["actions"]["scroll"]["can_scroll_up"] = scroll_info.get("can_scroll_up", False)
        organized["actions"]["scroll"]["can_scroll_down"] = scroll_info.get("can_scroll_down", False)
        
        
        organized["summary"] = {
            "total_elements": len(elements),
            "clickable_elements": len(organized["actions"]["click"]),
            "typeable_elements": len(organized["actions"]["type"]),
            "scrollable": organized["actions"]["scroll"]["can_scroll_up"] or organized["actions"]["scroll"]["can_scroll_down"]
        }
        
        logger.info(f"organize_elements - Processed {len(elements)} total elements:")
        logger.info(f"  - Click: {len(organized['actions']['click'])} elements")
        logger.info(f"  - Type: {len(organized['actions']['type'])} elements")
        
        if organized["actions"]["type"]:
            logger.info("Sample typeable elements:")
            for el in organized["actions"]["type"][:3]:
                logger.info(f"  - ID: {el.get('stable_id')} | Text: {el.get('text')} | Selector: {el.get('selector')}")
        
        return organized
    
    def _extract_element_text(self, element: dict) -> str:
        """Extract text from multiple sources of element"""
        text_sources = [
            (element.get("text") or "").strip(),
            (element.get("ariaLabel") or "").strip(),
            (element.get("aria_label") or "").strip(),
            (element.get("title") or "").strip(),
            (element.get("placeholder") or "").strip(),
            (element.get("alt") or "").strip(),
            (element.get("value") or "").strip(),
            (element.get("description") or "").strip()
        ]
        
        for text in text_sources:
            if text and len(text) > 0:
                if len(text) > 50:
                    return text[:50] + "..."
                return text
        
        return element.get("type") or "element"


class MCPConnectionManager:
    def __init__(self):
        self.active_connections: dict[str, WebSocket] = {}
        self.ui_states: dict[str, dict] = {}
        self.pending_actions: dict[str, asyncio.Future] = {}
        self.state_manager = UIStateManager()
    
    async def connect(self, user_id: str, websocket: WebSocket):
        await websocket.accept()
        self.active_connections[user_id] = websocket
        logger.info(f"MCP WebSocket connected: {user_id}")
    
    def disconnect(self, user_id: str):
        if user_id in self.active_connections:
            del self.active_connections[user_id]
        if user_id in self.ui_states:
            del self.ui_states[user_id]
        for action_id, future in list(self.pending_actions.items()):
            if not future.done():
                future.set_exception(Exception("WebSocket disconnected"))
        self.pending_actions.clear()
        logger.info(f"MCP WebSocket disconnected: {user_id}")
    
    async def store_ui_state(self, user_id: str, ui_state: dict):
        try:
            organized = self.state_manager.organize_elements(ui_state)
            self.ui_states[user_id] = organized
            
            await redis_manager.set_state(
                f"ui_state:{user_id}", 
                organized,
                ttl=300
            )
            
            summary = organized["summary"]
            logger.info(
                f"Stored UI state for user_id={user_id}: "
                f"{summary['clickable_elements']} clickable, "
                f"{summary['typeable_elements']} typeable, "
                f"{summary['total_elements']} total"
            )
        except Exception as e:
            logger.error(f"Error storing UI state: {e}")
    
    async def get_elements(self, user_id: str, action: Optional[str] = None) -> dict:
        ui_state = self.ui_states.get(user_id)
        
        if not ui_state:
            try:
                redis_state = await redis_manager.get_state(f"ui_state:{user_id}")
                if redis_state:
                    self.ui_states[user_id] = redis_state
                    ui_state = redis_state
                    logger.info(f"Loaded UI state from Redis for user_id={user_id}")
            except Exception as e:
                logger.error(f"Error loading from Redis: {e}")
        
        logger.info(f"get_elements called for user_id={user_id}, available keys: {list(self.ui_states.keys())}")
        
        if not ui_state:
            return {"success": False, "message": "No UI state available"}
        
        if action:
            if action in ui_state["actions"]:
                elements = ui_state["actions"][action]
                
                formatted_elements = []
                for i, el in enumerate(elements):
                    identifier = el.get("stable_id") or el.get("element_id") or el.get("id") or f"element_{i}"
                    
                    formatted = {
                        "id": identifier,
                        "text": el.get("text", ""),
                        "hint": self._generate_element_hint(el)
                    }
                    
                    if action == "type":
                        formatted["input_type"] = el.get("input_type", "text")
                        formatted["placeholder"] = el.get("placeholder", "")
                    
                    formatted_elements.append(formatted)
                
                return {
                    "success": True,
                    "url": ui_state["url"],
                    "title": ui_state["title"],
                    "action": action,
                    "elements": formatted_elements,
                    "count": len(formatted_elements)
                }
            else:
                return {"success": False, "message": f"Unknown action: {action}"}
        else:
            simplified_actions = {}
            for action_type, elements in ui_state["actions"].items():
                if action_type == "scroll":
                    simplified_actions[action_type] = elements
                else:
                    simplified_actions[action_type] = [
                        {
                            "id": el.get("stable_id") or el.get("element_id") or el.get("id") or f"el_{i}",
                            "text": el.get("text", ""),
                            "hint": self._generate_element_hint(el)
                        }
                        for i, el in enumerate(elements)
                    ]
            
            return {
                "success": True,
                "url": ui_state["url"],
                "title": ui_state["title"],
                "actions": simplified_actions,
                "summary": ui_state.get("summary", {})
            }
    
    def _generate_element_hint(self, element: dict) -> str:
        """Tạo hint mô tả element cho chatbot"""
        tag = element.get("tag", "")
        text = element.get("text", "")
        input_type = element.get("input_type", "")
        
        if tag in ["a", "link"]:
            return f"Link: {text}" if text else "Clickable link"
        elif tag == "button":
            return f"Button: {text}" if text else "Button"
        elif tag == "input":
            if input_type == "submit":
                return f"Submit button: {text}" if text else "Submit button"
            elif input_type == "search":
                return f"Search field: {text or element.get('placeholder', '')}"
            else:
                return f"Input field for {input_type}: {element.get('placeholder', '')}"
        elif tag == "textarea":
            return f"Text area: {element.get('placeholder', '')}"
        elif tag == "select":
            return f"Dropdown: {text}"
        else:
            return f"{tag} element: {text}" if text else f"{tag} element"
    
    async def send_action_and_wait(self, user_id: str, action_data: dict, timeout: float = 10.0) -> dict:
        if user_id not in self.active_connections:
            logger.warning(f"send_action_and_wait - No active connection for user_id={user_id}")
            return {"success": False, "message": "No active connection"}
        
        action_id = action_data["id"]
        future: asyncio.Future = asyncio.get_event_loop().create_future()
        self.pending_actions[action_id] = future
        
        try:
            selector = await self._resolve_element_selector(user_id, action_data)
            
            if not selector:
                return {"success": False, "message": "Element not found"}
            
            await self.active_connections[user_id].send_json({
                "type": "action",
                "id": action_id,
                "payload": {
                    "action": action_data["action"],
                    "selector": selector,
                    "value": action_data.get("value"),
                    "element_id": action_data.get("element_id"),
                    "metadata": {
                        "user_action": action_data.get("user_action", ""),
                        "timestamp": asyncio.get_event_loop().time()
                    }
                }
            })
            logger.info(f"Sent action request: {action_data.get('action')} -> waiting for result")
            
            result = await asyncio.wait_for(future, timeout=timeout)
            logger.info(f"Action result received: {result}")
            return result
        except asyncio.TimeoutError:
            logger.warning(f"Action timeout: {action_id}")
            return {"success": False, "message": "Action timeout"}
        except Exception as e:
            logger.error(f"Action failed: {e}")
            return {"success": False, "message": str(e)}
        finally:
            self.pending_actions.pop(action_id, None)
    
    
    async def _resolve_element_selector(self, user_id: str, action_data: dict) -> Optional[str]:
        """Resolve selector từ element ID hoặc các identifier khác"""
        ui_state = self.ui_states.get(user_id)
        if not ui_state:
            logger.warning(f"_resolve_element_selector - No UI state for user_id={user_id}")
            return None
        
        action_type = action_data.get("action")
        element_id = action_data.get("element_id")
        
        logger.info(f"_resolve_element_selector - action={action_type}, element_id={element_id}")
        
        action_key_map = {
            "click": "click",
            "fill": "type",
            "type": "type",
            "scroll": "scroll"
        }
        
        action_key = action_key_map.get(action_type, action_type)
        
        if action_key in ui_state["actions"]:
            elements = ui_state["actions"][action_key]
            logger.info(f"_resolve_element_selector - searching in '{action_key}' with {len(elements)} elements")
            
            for element in elements:
                if element.get("element_id") == element_id:
                    selector = element.get("selector")
                    logger.info(f"_resolve_element_selector - Found via element_id: {selector}")
                    return selector
                elif element.get("stable_id") == element_id:
                    selector = element.get("selector")
                    logger.info(f"_resolve_element_selector - Found via stable_id: {selector}")
                    return selector
                elif element.get("id") == element_id:
                    selector = element.get("selector")
                    logger.info(f"_resolve_element_selector - Found via id: {selector}")
                    return selector
        
        logger.warning(f"_resolve_element_selector - Element not found in '{action_key}', searching all actions")
        for action_name, elements in ui_state["actions"].items():
            if action_name == "scroll":
                continue
            logger.info(f"_resolve_element_selector - checking '{action_name}' with {len(elements)} elements")
            for element in elements:
                if element.get("element_id") == element_id or \
                   element.get("stable_id") == element_id or \
                   element.get("id") == element_id:
                    selector = element.get("selector")
                    logger.info(f"_resolve_element_selector - Found in '{action_name}': {selector}")
                    return selector
        
        logger.error(f"_resolve_element_selector - Element '{element_id}' not found in any action type")
        return None

    
    def resolve_action(self, action_id: str, result: dict):
        future = self.pending_actions.get(action_id)
        if future and not future.done():
            future.set_result(result)
            logger.info(f"Action resolved: {action_id}")


mcp_manager = MCPConnectionManager()


async def handle_mcp_websocket(websocket: WebSocket, user_id: str):
    await mcp_manager.connect(user_id, websocket)
    
    try:
        while True:
            data = await websocket.receive_json()
            message_type = data.get("type")
            
            if message_type == "ui_state":
                ui_state = data.get("data", {})
                await mcp_manager.store_ui_state(user_id, ui_state)
                
                await websocket.send_json({
                    "type": "ui_state_ack",
                    "status": "stored",
                    "summary": {
                        "elements": len(ui_state.get("elements", [])),
                        "timestamp": ui_state.get("timestamp")
                    }
                })
            
            elif message_type == "action_result":
                action_id = data.get("id")
                result = data.get("payload", {})
                mcp_manager.resolve_action(action_id, result)
            
            elif message_type == "pong":
                pass
            
            elif message_type == "heartbeat":
                await websocket.send_json({
                    "type": "heartbeat_ack",
                    "timestamp": data.get("timestamp")
                })
    
    except WebSocketDisconnect:
        mcp_manager.disconnect(user_id)
    except Exception as e:
        logger.error(f"WebSocket error for {user_id}: {e}")
        mcp_manager.disconnect(user_id)