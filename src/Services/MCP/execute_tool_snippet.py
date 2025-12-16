    async def execute_tool(
        self,
        user_id: str,
        tool_name: str,
        arguments: dict
    ) -> dict:
        """Execute browser tool - sends action to client via WebSocket or executes server-side"""
        from websocket_handler import manager as ws_manager
        
        # Check if client is connected via WebSocket
        if ws_manager.is_connected(user_id):
            logger.info(f"Sending {tool_name} action to client {user_id} via WebSocket")
            
            # Map tool name to action
            action_map = {
                "browser_click": "click",
                "browser_fill": "fill",
                "browser_scroll": "scroll",
                "browser_navigate": "navigate",
                "browser_type": "type",
            }
            
            action = action_map.get(tool_name, tool_name.replace("browser_", ""))
            
            # Send action request to client via WebSocket
           result = await ws_manager.send_action_request(user_id, action, arguments)
            
            if result:
                logger.info(f"Action {action} executed on client for {user_id}")
                return {"success": True, "message": f"Action {action} executed"}
            else:
                return {"success": False, "error": "Failed to send action to client"}
        
        # Fallback: server-side execution (if no WebSocket)
        logger.warning(f"Client {user_id} not connected, cannot execute {tool_name}")
        return {"success": False, "error": "Client not connected via WebSocket"}
