from langgraph.graph import StateGraph
from lg.state import ChatState
from lg.nodes.intent import analyze_intent
from lg.nodes.search import search_products
from lg.nodes.present import present_results
from lg.nodes.selection import handle_selection
from lg.nodes.tools import call_tool
from lg.nodes.navigation import navigate_ui
from lg.nodes.context import update_context
from lg.nodes.finish import finish

def create_graph():
    """Create the LangGraph workflow"""
    
    graph = StateGraph(ChatState)
    
    # Add nodes
    graph.add_node("analyze_intent", analyze_intent)
    graph.add_node("search_products", search_products)
    graph.add_node("present_results", present_results)
    graph.add_node("handle_selection", handle_selection)
    graph.add_node("call_tool", call_tool)
    graph.add_node("navigate_ui", navigate_ui)
    graph.add_node("update_context", update_context)
    graph.add_node("finish", finish)
    
    # Set entry point
    graph.set_entry_point("analyze_intent")
    
    # Add conditional edges
    graph.add_conditional_edges(
        "analyze_intent",
        route_after_intent,
        {
            "search": "search_products",
            "selection": "handle_selection",
            "tool": "call_tool", 
            "finish": "finish"
        }
    )
    
    graph.add_edge("search_products", "present_results")
    graph.add_edge("present_results", "navigate_ui")
    
    graph.add_conditional_edges(
        "navigate_ui",
        route_after_navigation,
        {
            "continue": "update_context",
            "selection": "handle_selection"
        }
    )
    
    graph.add_edge("handle_selection", "call_tool")
    graph.add_edge("call_tool", "update_context")
    graph.add_edge("update_context", "finish")
    
    return graph.compile()

def route_after_intent(state: ChatState) -> str:
    """Route after intent analysis"""
    intent = state.get("last_intent")
    
    print(f"🔀 Route after intent: {intent}")
    
    # Search intents
    if intent in ["search_product", "get_top_rated", "get_top_selling", "get_new_arrivals"]:
        return "search"
    
    # Tool intents
    elif intent in [
        # Product intents
        "get_product_detail", "get_brands", "get_categories",
        # Cart intents
        "view_cart", "clear_cart", "prepare_checkout",
        # Order intents
        "view_orders", "get_order_detail", "create_order", "cancel_order",
        # Wishlist intents
        "view_wishlist"
    ]:
        return "tool"
    
    elif intent in ["add_to_cart", "remove_from_cart", "add_to_wishlist", "remove_from_wishlist"]:
        target_product_id = state.get("target_product_id")
        if target_product_id and len(target_product_id) > 2:
            return "tool"
        else:
            return "finish"
    
    # Xử lý intent "other" - kiểm tra xem có phải selection không
    elif intent == "other":
        user_message = state["messages"][-1].content if state["messages"] else ""
        
        # Kiểm tra selection keywords
        selection_keywords = [
            "sản phẩm", "đầu tiên", "thứ", "chi tiết", "thêm", "mua", 
            "giỏ hàng", "wishlist", "yêu thích", "chọn", "cái đầu", "xóa"
        ]
        has_selection_keyword = any(keyword in user_message.lower() for keyword in selection_keywords)
        
        # Kiểm tra có context list không
        search_results = state.get("search_results")
        has_search_results = (search_results and 
                             isinstance(search_results, dict) and 
                             search_results.get("status") == "success" and 
                             search_results.get("data"))
        
        cart_items = state.get("cart_items", [])
        wishlist_items = state.get("wishlist_items", [])
        has_list_context = has_search_results or len(cart_items) > 0 or len(wishlist_items) > 0
        
        if has_selection_keyword and has_list_context:
            return "selection"
        else:
            return "finish"
    
    else:
        return "finish"

def route_after_navigation(state: ChatState) -> str:
    """Route after navigation setup"""
    user_message = state["messages"][-1].content if state["messages"] else ""
    intent = state.get("last_intent")
    
    # Kiểm tra selection keywords
    selection_keywords = [
        "sản phẩm", "đầu tiên", "thứ", "chi tiết", "thêm", "mua", 
        "giỏ hàng", "wishlist", "yêu thích", "chọn", "cái đầu", "xóa"
    ]
    has_selection_keyword = any(keyword in user_message.lower() for keyword in selection_keywords)
    
    # Kiểm tra có context list không
    search_results = state.get("search_results")
    has_search_results = (search_results and 
                         isinstance(search_results, dict) and 
                         search_results.get("status") == "success" and 
                         search_results.get("data"))
    
    cart_items = state.get("cart_items", [])
    wishlist_items = state.get("wishlist_items", [])
    has_list_context = has_search_results or len(cart_items) > 0 or len(wishlist_items) > 0
    
    print(f"🔀 Route after navigation:")
    print(f"   user_message: {user_message}")
    print(f"   intent: {intent}")
    print(f"   has_selection_keyword: {has_selection_keyword}")
    print(f"   has_list_context: {has_list_context}")
    print(f"   cart_items: {len(cart_items)}")
    print(f"   wishlist_items: {len(wishlist_items)}")
    
    if (has_selection_keyword and has_list_context) or (intent == "other" and has_selection_keyword):
        print(f"   -> Going to selection")
        return "selection"
    else:
        print(f"   -> Going to continue")
        return "continue"