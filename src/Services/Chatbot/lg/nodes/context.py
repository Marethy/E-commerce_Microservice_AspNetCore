from lg.state import ChatState

async def update_context(state: ChatState) -> ChatState:
    """Update conversation context and state"""
    
    new_state = dict(state)
    
    intent = new_state.get("last_intent")
    tool_result = new_state.get("tool_result")
    
    if intent in ["add_to_cart", "view_cart"] and tool_result:
        if tool_result.get("status") == "success":
            new_state["cart_state"] = tool_result.get("data")
    
    if "search_results" in new_state and intent not in ["search_product"]:
        del new_state["search_results"]
    
    if "tool_result" in new_state:
        del new_state["tool_result"]
    
    return new_state