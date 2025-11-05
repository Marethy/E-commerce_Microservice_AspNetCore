from langchain_openai import ChatOpenAI
from lg.state import ChatState
from config import config

llm = ChatOpenAI(
    model=config.GROK_MODEL,
    api_key=config.GROK_API_KEY,
    base_url=config.GROK_BASE_URL
)

async def present_results(state: ChatState) -> ChatState:
    """Present search results to user"""
    
    search_results = state.get("search_results")
    
    new_state = state.copy()
    
    if not search_results or search_results.get("status") != "success":
        print(f"📋 No valid search results")
        response_text = "Xin lỗi, không tìm thấy sản phẩm phù hợp. Bạn có thể thử từ khóa khác không?"
    else:
        products = search_results.get("data", [])
        print(f"📋 Presenting {len(products)} products")
        
        if not products:
            response_text = "Không tìm thấy sản phẩm nào phù hợp với yêu cầu của bạn."
        else:
            # Limit to 5 products and format clearly
            products = products[:5]
            
            new_state["last_displayed_list"] = "search"
            
            # Create detailed product list
            products_list = []
            for i, product in enumerate(products, 1):
                name = product.get("name", "N/A")
                price = product.get("price", 0)
                brand = product.get("brand", {})
                brand_name = brand.get("name", "N/A") if isinstance(brand, dict) else str(brand) if brand else "N/A"
                
                product_info = f"{i}. **{name}**\n   💰 Giá: {price:,.0f}đ\n   🏷️ Thương hiệu: {brand_name}"
                products_list.append(product_info)
            
            # Create response
            response_text = f"Tôi tìm thấy {len(products)} sản phẩm phù hợp:\n\n"
            response_text += "\n\n".join(products_list)
            response_text += "\n\n🛒 Bạn muốn xem chi tiết sản phẩm nào? Chỉ cần nói \"sản phẩm 1\" hoặc \"chi tiết sản phẩm đầu tiên\"."
            response_text += "\n📱 Hoặc nói \"thêm sản phẩm 1 vào giỏ\" để mua ngay!"
    
    new_state["presentation_response"] = response_text
    print(f"📋 Generated presentation: {len(response_text)} chars")
    
    return new_state