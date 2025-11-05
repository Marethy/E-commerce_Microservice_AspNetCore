from lg.state import ChatState
from lg.tools.product import ProductTools

async def search_products(state: ChatState) -> ChatState:
    """Search products based on filters"""
    intent = state.get("last_intent")
    filters = state.get("filters", {}).copy()
    token = state.get("user_token")  # ADD THIS

    new_state = {**state}
    
    try:
        if intent == "search_product":
            api_filters = filters.copy()
     
            if api_filters.get("brand"):
                brands_response = await ProductTools.get_brands(token)  # ADD token
                if brands_response.get("status") == "success":
                    brands = brands_response["data"]
                    brand_id = next((b["id"] for b in brands if b["name"].lower() == api_filters["brand"].lower()), None)
                    if brand_id:
                        api_filters["brandIds"] = [brand_id]
                        del api_filters["brand"]
            
            if api_filters.get("category"):
                categories_response = await ProductTools.get_categories(token)  # ADD token
                if categories_response.get("status") == "success":
                    categories = categories_response["data"]
                    category_id = next((c["id"] for c in categories if c["name"].lower() == api_filters["category"].lower()), None)
                    if category_id:
                        api_filters["categoryIds"] = [category_id]
                        del api_filters["category"]
        
            result = await ProductTools.search_products(api_filters, token)  # ADD token
    
            if result.get("status") == "error":
                if "timeout" in result.get("message", "").lower():
                    result["user_message"] = "Hệ thống đang bận, vui lòng thử lại sau vài giây."
                elif "connect" in result.get("message", "").lower():
                    result["user_message"] = "Không thể kết nối đến dịch vụ tìm kiếm. Vui lòng thử lại."
                else:
                    result["user_message"] = "Có lỗi xảy ra khi tìm kiếm. Vui lòng thử từ khóa khác."
            
            new_state["search_results"] = result
            
        elif intent in ["get_top_rated", "top_rated"]:
            result = await ProductTools.get_top_rated(token)  # ADD token
            new_state["search_results"] = result
            new_state["filters"] = {**filters, "sortBy": "ratingAverage", "direction": "desc"}
  
        elif intent in ["get_top_selling", "top_selling"]:
            result = await ProductTools.get_top_selling(token)  # ADD token
            new_state["search_results"] = result
            new_state["filters"] = {**filters, "sortBy": "allTimeQuantitySold", "direction": "desc"}
            
        elif intent in ["get_new_arrivals", "new_arrivals"]:
            result = await ProductTools.get_new_arrivals(token)  # ADD token
            new_state["search_results"] = result
            new_state["filters"] = {**filters, "sortBy": "createdAt", "direction": "desc"}
      
    except Exception as e:
        import traceback
        traceback.print_exc()
        new_state["search_results"] = {
            "status": "error", 
            "message": str(e),
            "user_message": "Có lỗi hệ thống. Vui lòng thử lại sau."
        }
    
    return new_state