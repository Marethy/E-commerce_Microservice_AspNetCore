import asyncio
import logging
from typing import Any, Optional
from dataclasses import dataclass, field

from browser.manager import BrowserManager, BrowserSession
from browser.scanner import ElementScanner, UIElement
from browser.executor import ActionExecutor
from router import SemanticRouter, RouteResult
from router.tool_builder import DynamicToolBuilder, ElementResolver
from embeddings import EmbeddingService

logger = logging.getLogger(__name__)


@dataclass
class PlaywrightSession:
    user_id: str
    browser_session: BrowserSession
    scanner: ElementScanner
    executor: ActionExecutor
    elements: list[UIElement] = field(default_factory=list)
    element_resolver: Optional[ElementResolver] = None
    last_url: str = ""
    last_snapshot_time: float = 0


class PlaywrightMCPServer:
    # Domain whitelist for security
    ALLOWED_DOMAINS = [
        "localhost",
        "127.0.0.1",
        "localhost:5173",
        "127.0.0.1:5173",
        "host.docker.internal",
        "host.docker.internal:5173",
    ]
    
    def __init__(self, embedding_service: Optional[EmbeddingService] = None):
        self.embedding_service = embedding_service or EmbeddingService()
        self.router = SemanticRouter(self.embedding_service)
        self._browser_manager: Optional[BrowserManager] = None
        self._sessions: dict[str, PlaywrightSession] = {}
        self._initialized = False
    
    async def initialize(self):
        if self._initialized:
            return
        
        logger.info("Initializing PlaywrightMCPServer...")
        
        # Initialize semantic router embeddings
        await self.router.initialize()
        
        # Initialize browser manager
        self._browser_manager = await BrowserManager.get_instance()
        
        self._initialized = True
        logger.info("PlaywrightMCPServer initialized successfully")
    
    async def create_session(
        self,
        user_id: str,
        start_url: Optional[str] = None
    ) -> PlaywrightSession:
        if not self._initialized:
            await self.initialize()
        
        # Check for existing session
        if user_id in self._sessions:
            logger.info(f"Returning existing session for {user_id}")
            return self._sessions[user_id]
        
        # Create browser session
        browser_session = await self._browser_manager.create_session(user_id, start_url)
        
        # Create scanner and executor
        scanner = ElementScanner(browser_session.page)
        executor = ActionExecutor(browser_session.page)
        
        # Create combined session
        session = PlaywrightSession(
            user_id=user_id,
            browser_session=browser_session,
            scanner=scanner,
            executor=executor
        )
        
        self._sessions[user_id] = session
        logger.info(f"Created Playwright session for {user_id}")
        
        return session
    
    def get_session(self, user_id: str) -> Optional[PlaywrightSession]:
        return self._sessions.get(user_id)
    
    async def close_session(self, user_id: str):
        session = self._sessions.pop(user_id, None)
        if session and self._browser_manager:
            await self._browser_manager.close_session(user_id)
            logger.info(f"Closed session for {user_id}")
    
    async def execute_client_action(
        self,
        user_id: str,
        action: dict,
        timeout: float = 30.0
    ) -> dict:
        """Execute action via WebSocket on client's browser, fallback to server-side"""
        from websocket_handler import manager
        import uuid
        
        # Check if client is connected
        if manager.is_connected(user_id):
            try:
                action_id = str(uuid.uuid4())
                action_with_id = {"id": action_id, **action}
                
                logger.info(f"Executing action via WebSocket for {user_id}: {action}")
                result = await manager.execute_action(user_id, action_id, action_with_id, timeout)
                
                return result
                
            except Exception as e:
                logger.warning(f"WebSocket execution failed for {user_id}: {e}")
                # Continue to fallback
        
        # Fallback to server-side Playwright execution
        logger.info(f"No WebSocket connection, using server-side Playwright for {user_id}")
        session = self._sessions.get(user_id)
        if session:
            try:
                result = await session.executor.execute(
                    action=action.get("action", ""),
                    selector=action.get("selector", ""),
                    value=action.get("value"),
                    **action.get("options", {})
                )
                # Convert ActionResult to dict, with screenshot as base64
                import base64
                result_dict = result.to_dict() if hasattr(result, 'to_dict') else result
                if hasattr(result, 'screenshot') and result.screenshot:
                    result_dict["screenshot"] = base64.b64encode(result.screenshot).decode('utf-8')
                return result_dict
            except Exception as e:
                return {"success": False, "error": str(e)}
        
        return {"success": False, "error": "No session and no WebSocket connection"}
    
    # ============== Phase 1: Intent Routing ==============
    
    async def route_intent(
        self,
        user_intent: str,
        top_k: int = 3
    ) -> list[RouteResult]:
        if not self._initialized:
            await self.initialize()
        
        return await self.router.route_intent(user_intent, top_k)
    
    async def classify_intent(self, user_intent: str) -> Optional[RouteResult]:
        if not self._initialized:
            await self.initialize()
        
        return await self.router.classify_action(user_intent)
    
    # ============== Phase 2: Dynamic Discovery ==============
    
    async def capture_snapshot(
        self,
        user_id: str,
        max_elements: int = 100
    ) -> list[UIElement]:
        # Try to get client elements first via WebSocket
        from websocket_handler import manager as ws_manager
        
        if ws_manager.is_connected(user_id):
            ui_state = ws_manager.get_ui_state(user_id)
            if ui_state and ui_state.get('elements'):
                logger.info(f"Using client UI state for {user_id} (WebSocket connected)")
                try:
                    # Parse client elements
                    elements = self._parse_client_elements(ui_state['elements'])
                    
                    # Ensure session exists (but don't create browser if using client state)
                    session = self._sessions.get(user_id)
                    if not session:
                        # Create minimal session without browser (will use client browser)
                        logger.info(f"Creating minimal session for client-driven user {user_id}")
                        session = PlaywrightSession(
                            user_id=user_id,
                            browser_session=None,  # No server-side browser needed
                            scanner=None,
                            executor=None
                        )
                        self._sessions[user_id] = session
                    
                    # Update session state from client
                    session.elements = elements
                    session.element_resolver = ElementResolver(elements)
                    session.last_url = ui_state.get('url', '')  # Set from client UI state
                    session.last_snapshot_time = asyncio.get_event_loop().time()
                    
                    logger.info(f"Captured {len(elements)} elements from client for {user_id}")
                    return elements[:max_elements]
                except Exception as e:
                    logger.warning(f"Failed to parse client elements for {user_id}: {e}")
                    # Fall through to server-side scanning
        
        # Fallback: server-side Playwright scanning
        logger.info(f"Using server-side Playwright for {user_id} (client not connected)")
        
        session = self._sessions.get(user_id)
        if not session:
            try:
                session = await self.create_session(user_id, start_url="http://host.docker.internal:5173")
            except Exception as e:
                logger.error(f"Failed to create session for {user_id}: {e}")
                return []
        
        # Capture snapshot from server-side browser
        elements = await session.scanner.capture_snapshot(max_elements)
        
        # Update session state
        session.elements = elements
        session.element_resolver = ElementResolver(elements)
        if session.browser_session and session.browser_session.page:
            session.last_url = session.browser_session.page.url
        session.last_snapshot_time = asyncio.get_event_loop().time()
        
        logger.info(f"Captured {len(elements)} elements from server for {user_id}")
        
        return elements
    
    def _parse_client_elements(self, client_elements: list[dict]) -> list[UIElement]:
        """Convert client element dicts to UIElement objects"""
        elements = []
        
        for el_dict in client_elements:
            try:
                elements.append(UIElement(
                    id=el_dict.get('id', ''),
                    selector=el_dict.get('selector', ''),
                    type=el_dict.get('type', 'interactive'),
                    description=el_dict.get('description', ''),
                    text=el_dict.get('text', ''),
                    aria_label=el_dict.get('ariaLabel'),
                    placeholder=el_dict.get('placeholder'),
                    test_id=None,
                    name=None,
                    value=el_dict.get('value'),
                    is_visible=el_dict.get('visible', True),
                    is_enabled=True,
                    rect={},
                    attributes={
                        'aria-label': el_dict.get('ariaLabel', ''),
                        'href': el_dict.get('href', '')
                    },
                    options=[],
                    context=''
                ))
            except Exception as e:
                logger.warning(f"Failed to parse client element {el_dict.get('id', 'unknown')}: {e}")
                continue
        
        return elements
    
    async def discover_tools(
        self,
        user_id: str,
        query: str = "",
        force_refresh: bool = False
    ) -> dict:
        session = self._sessions.get(user_id)
        if not session:
            try:
                logger.info(f"Auto-creating session for user {user_id}")
                session = await self.create_session(user_id, start_url=None)
            except Exception as e:
                logger.error(f"Failed to auto-create session: {e}")
                return {"tools": [], "error": f"Failed to create session: {str(e)}"}
        
        # Get page state safely
        page_state = await self.get_page_state(user_id)
        current_url = page_state.get("url", session.last_url if hasattr(session, 'last_url') else "")
        
        should_refresh = (
            force_refresh or
            not session.elements or
            current_url != session.last_url or
            (asyncio.get_event_loop().time() - session.last_snapshot_time) > 30  # 30 second cache
        )
        
        if should_refresh:
            await self.capture_snapshot(user_id)
        
        # Get page context
        context = {
            "url": page_state.get("url", ""),
            "title": page_state.get("title", ""),
        }
        
        # If query provided, use semantic routing to prioritize tools
        if query:
            route_results = await self.route_intent(query, top_k=3)
            if route_results:
                # Build tools for top matched action types
                tools = []
                seen_actions = set()
                
                for result in route_results:
                    if result.action_type not in seen_actions:
                        tool = DynamicToolBuilder.build_tools_for_action(
                            result.action_type,
                            session.elements,
                            context
                        )
                        if tool:
                            tool["_routing_confidence"] = result.confidence
                            tools.append(tool)
                            seen_actions.add(result.action_type)
                
                # Add remaining essential tools
                for action_type in ['scroll', 'navigate', 'screenshot']:
                    if action_type not in seen_actions:
                        tool = DynamicToolBuilder.build_tools_for_action(
                            action_type,
                            session.elements,
                            context
                        )
                        if tool:
                            tools.append(tool)
                
                return {
                    "tools": tools,
                    "context": context,
                    "element_count": len(session.elements),
                    "query": query,
                    "routing_results": [r.to_dict() for r in route_results]
                }
        
        # No query - return all tools
        tools = DynamicToolBuilder.build_all_tools(
            session.elements,
            context,
            include_static=True
        )
        
        return {
            "tools": tools,
            "context": context,
            "element_count": len(session.elements)
        }
    
    # ============== Action Execution ==============
    
    async def execute_tool(
        self,
        user_id: str,
        tool_name: str,
        arguments: dict[str, Any] = {}
    ) -> dict:
        session = self._sessions.get(user_id)
        if not session:
            try:
                session = await self.create_session(user_id, start_url="http://host.docker.internal:5173")
            except Exception as e:
                return {"success": False, "error": f"Failed to create session: {str(e)}"}
        
        # Parse tool name to get action
        action = self._parse_tool_action(tool_name)
        
        # Resolve element if needed
        selector = ""
        element_id = arguments.get("element_id")
        
        if element_id and session.element_resolver:
            logger.info(f"Resolving element_id: {element_id} for user {user_id}")
            element = session.element_resolver.resolve(element_id)
            if element:
                selector = element.selector
                logger.info(f"Resolved {element_id} to selector: {selector}")
            else:
                logger.error(f"Element '{element_id}' not found in resolver")
                return {"success": False, "error": f"Element '{element_id}' not found"}
        elif arguments.get("selector"):
            selector = arguments["selector"]
        
        # Get value for fill/type/select actions
        value = arguments.get("value") or arguments.get("text") or arguments.get("url")
        
        # Handle special arguments
        options = {}
        if "direction" in arguments:
            value = arguments["direction"]
        if "key" in arguments:
            value = arguments["key"]
        if "delay" in arguments:
            options["delay"] = arguments["delay"]
        if "full_page" in arguments:
            options["full_page"] = arguments["full_page"]
        if "timeout" in arguments:
            options["timeout"] = arguments["timeout"]
        if "checked" in arguments:
            action = "check" if arguments["checked"] else "uncheck"
        
        # Prepare action payload
        action_payload = {
            "action": action,
            "selector": selector,
            "value": value,
            "options": options
        }
        
        # Try client-side execution first (if client is connected)
        try:
            result = await self.execute_client_action(user_id, action_payload, timeout=30.0)
            
            # If action changed page state, invalidate snapshot cache
            if result.get("success") and action in ('click', 'fill', 'select', 'navigate', 'check', 'uncheck'):
                session.elements = []  # Force refresh on next discover
            
            return result
            
        except Exception as e:
            logger.warning(f"Client-side execution failed, falling back to server-side: {e}")
            # Fallback is handled in execute_client_action
            return {"success": False, "error": str(e)}
    
    def _parse_tool_action(self, tool_name: str) -> str:
        # Map tool names to actions
        tool_action_map = {
            "browser_click": "click",
            "browser_fill": "fill",
            "browser_type": "type",
            "browser_select": "select",
            "browser_scroll": "scroll",
            "browser_navigate": "navigate",
            "browser_screenshot": "screenshot",
            "browser_wait": "wait",
            "browser_hover": "hover",
            "browser_press_key": "press",
            "browser_check": "check",
        }
        
        return tool_action_map.get(tool_name, "click")
    
    # ============== Navigation & State ==============
    
    async def navigate(
        self,
        user_id: str,
        url: str,
        wait_until: str = "networkidle"
    ) -> dict:
        session = self._sessions.get(user_id)
        if not session:
            return {"success": False, "error": "No session found"}
        
        # Validate domain against whitelist
        from urllib.parse import urlparse
        parsed = urlparse(url)
        
        # Build domain with port
        domain = parsed.hostname or ""
        if parsed.port:
            domain = f"{domain}:{parsed.port}"
        
        # Check whitelist
        if domain and domain not in self.ALLOWED_DOMAINS:
            logger.warning(f"Navigation blocked: {domain} not in allowed domains")
            return {
                "success": False,
                "error": f"Navigation blocked: domain '{domain}' not allowed. Only localhost:5173 is permitted."
            }
        
        result = await session.executor.execute(
            action="navigate",
            selector="",
            value=url,
            options={"wait_until": wait_until}
        )
        
        # Invalidate snapshot cache
        if result.success:
            session.elements = []
        
        return result.to_dict()
    
    async def get_page_state(self, user_id: str) -> dict:
        """Get current page state (URL, title)"""
        from websocket_handler import manager as ws_manager
        
        # Try to get state from WebSocket client first
        if ws_manager.is_connected(user_id):
            ui_state = ws_manager.get_ui_state(user_id)
            if ui_state:
                return {
                    "url": ui_state.get("url", ""),
                    "title": ui_state.get("title", "")
                }
        
        # Fallback: get from server-side browser session
        session = self._sessions.get(user_id)
        if session and session.browser_session and session.browser_session.page:
            return {
                "url": session.browser_session.page.url,
                "title": await session.browser_session.page.title()
            }
        
        return {"url": "", "title": ""}
    
    async def take_screenshot(
        self,
        user_id: str,
        full_page: bool = False
    ) -> Optional[bytes]:
        session = self._sessions.get(user_id)
        if not session:
            return None
        
        result = await session.executor.execute(
            action="screenshot",
            selector="",
            value=None,
            options={"full_page": full_page}
        )
        
        return result.screenshot
    
    # ============== Client-Side Execution (via WebSocket) ==============
    
    async def execute_client_action(
        self,
        user_id: str,
        action: dict,
        timeout: float = 30.0
    ) -> dict:
        # Import here to avoid circular dependency
        from websocket_handler import manager as ws_manager
        
        # Check if user is connected via WebSocket
        if not ws_manager.is_connected(user_id):
            # Fallback to server-side Playwright execution
            logger.warning(f"User {user_id} not connected via WebSocket, using server-side execution")
            return await self._execute_server_side(user_id, action)
        
        # Generate action ID
        import uuid
        action_id = str(uuid.uuid4())
        
        try:
            # Execute action via WebSocket and wait for result
            result = await ws_manager.execute_action(user_id, action_id, action, timeout)
            return result
        except Exception as e:
            logger.error(f"Client-side execution failed for {user_id}: {e}")
            return {
                "success": False,
                "error": str(e)
            }
    
    async def _execute_server_side(self, user_id: str, action: dict) -> dict:
        session = self._sessions.get(user_id)
        if not session:
            # Create session if doesn't exist
            session = await self.create_session(user_id)
        
        action_type = action.get("action", "")
        selector = action.get("selector", "")
        value = action.get("value")
        options = action.get("options", {})
        
        try:
            result = await session.executor.execute(
                action=action_type,
                selector=selector,
                value=value,
                options=options
            )
            
            # Get page state safely
            page_state = await self.get_page_state(user_id)
            
            return {
                "success": result.success,
                "error": result.error,
                "newState": {
                    "url": page_state.get("url", ""),
                    "title": page_state.get("title", "")
                }
            }
        except Exception as e:
            logger.error(f"Server-side execution error: {e}")
            return {
                "success": False,
                "error": str(e)
            }
    
    async def get_client_ui_state(self, user_id: str) -> Optional[dict]:
        from websocket_handler import manager as ws_manager
        return ws_manager.get_ui_state(user_id)
    
    # ============== Lifecycle ==============
    
    async def shutdown(self):
        logger.info("Shutting down PlaywrightMCPServer...")
        
        # Close all sessions
        for user_id in list(self._sessions.keys()):
            await self.close_session(user_id)
        
        # Shutdown browser manager
        if self._browser_manager:
            await self._browser_manager.shutdown()
        
        self._initialized = False
        logger.info("PlaywrightMCPServer shutdown complete")
    
    @property
    def active_sessions(self) -> int:
        return len(self._sessions)
    
    @property
    def is_initialized(self) -> bool:
        return self._initialized


# Singleton instance
_playwright_mcp_server: Optional[PlaywrightMCPServer] = None


async def get_playwright_mcp_server() -> PlaywrightMCPServer:
    global _playwright_mcp_server
    
    if _playwright_mcp_server is None:
        _playwright_mcp_server = PlaywrightMCPServer()
        await _playwright_mcp_server.initialize()
    
    return _playwright_mcp_server
