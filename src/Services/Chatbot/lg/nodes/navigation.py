from lg.state import ChatState
from urllib.parse import urlencode

async def navigate_ui(state: ChatState) -> ChatState:
    """Set UI navigation based on context"""
    
    intent = state.get("last_intent")
    filters = state.get("filters", {})
    selected_product = state.get("selected_product")
    
    if intent in ["search_product", "get_top_rated", "get_top_selling", "get_new_arrivals"]:
        # Build search URL with all supported parameters
        url_params = {}
        
        if filters.get("query"):
            url_params["q"] = filters["query"]
        
        if filters.get("category"):
            url_params["categoryName"] = filters["category"]
        
        if filters.get("brand"):
            url_params["brandName"] = filters["brand"]
        
        # Price filters
        if filters.get("minPrice") and filters["minPrice"] > 0:
            url_params["minPrice"] = str(filters["minPrice"])
        if filters.get("maxPrice") and filters["maxPrice"] > 0:
            url_params["maxPrice"] = str(filters["maxPrice"])
        
        # Rating filters
        if filters.get("minRating") and filters["minRating"] > 0:
            url_params["minRating"] = str(filters["minRating"])
        if filters.get("maxRating") and filters["maxRating"] > 0:
            url_params["maxRating"] = str(filters["maxRating"])
        
        # Build URL
        if url_params:
            query_string = urlencode(url_params)
            state["suggested_url"] = f"/search?{query_string}"
        else:
            state["suggested_url"] = "/search"
            
    elif intent == "get_product_detail" and selected_product:
        state["suggested_url"] = f"/product/{selected_product['id']}"
    elif intent == "view_cart":
        state["suggested_url"] = "/cart"
    elif intent == "view_orders":
        state["suggested_url"] = "/orders"
    elif intent == "view_wishlist":
        state["suggested_url"] = "/wishlist"
    
    return state