from langchain_openai import ChatOpenAI
from langchain_core.messages import SystemMessage, HumanMessage
from lg.state import ChatState
from config import config
from textwrap import dedent
import re
import json

llm = ChatOpenAI(
    model=config.GROK_MODEL,
    api_key=config.GROK_API_KEY,
    base_url=config.GROK_BASE_URL
)

async def handle_selection(state: ChatState) -> ChatState:
    """Handle user selection from search results or cart/wishlist"""
    
    user_message = state["messages"][-1].content if state["messages"] else ""
    search_results = state.get("search_results", {})
    cart_items = state.get("cart_items", [])
    wishlist_items = state.get("wishlist_items", [])
    last_displayed_list = state.get("last_displayed_list")
    
    # Debug logging
    print(f"🔍 Selection Analysis:")
    print(f"   User message: {user_message}")
    print(f"   Last displayed list: {last_displayed_list}")
    print(f"   Cart items: {len(cart_items)}")
    print(f"   Wishlist items: {len(wishlist_items)}")
    print(f"   Search results: {len(search_results.get('data', []))}")
    
    system_prompt = dedent("""\
        Phân tích tin nhắn người dùng để xác định việc chọn sản phẩm từ danh sách và hành động.
        
        Các cụm từ chỉ selection:
        - "sản phẩm đầu tiên", "sản phẩm 1", "cái đầu", "thứ nhất" → index 1
        - "sản phẩm thứ hai", "sản phẩm 2", "cái thứ 2", "thứ hai" → index 2  
        - "sản phẩm thứ ba", "sản phẩm 3", "thứ ba" → index 3
        
        Các hành động:
        - "thêm vào giỏ", "mua", "đặt hàng" → action: add_to_cart
        - "xem chi tiết", "chi tiết", "xem thêm" → action: view_detail
        - "thêm wishlist", "yêu thích" → action: add_to_wishlist
        - "xóa khỏi giỏ", "xóa", "remove" → action: remove_from_cart
        - "xóa khỏi wishlist", "bỏ yêu thích" → action: remove_from_wishlist
        
        Trả về JSON:
        {
            "is_selection": true/false,
            "selected_index": number (1-based, 0 nếu không rõ),
            "action": "view_detail|add_to_cart|add_to_wishlist|remove_from_cart|remove_from_wishlist|none",
            "quantity": number (default 1),
            "confidence": number (0-1)
        }
        
        CHỈ TRẢ VỀ JSON, KHÔNG CÓ TEXT KHÁC.
    """)

    messages = [
        SystemMessage(content=system_prompt),
        HumanMessage(content=f"Tin nhắn: {user_message}")
    ]
    
    try:
        response = await llm.ainvoke(messages)
        content = response.content.strip()
        
        # Extract JSON
        json_match = re.search(r'\{.*\}', content, re.DOTALL)
        if json_match:
            result = json.loads(json_match.group())
            print(f"   Parsed result: {result}")
            
            if result.get("is_selection", False) and result.get("confidence", 0) > 0.5:
                selected_index = result.get("selected_index", 0)
                
                # Determine which list to use based on context
                products = []
                source_list = None
                
                if last_displayed_list == "cart" and cart_items:
                    # Extract product info from cart items
                    products = []
                    for item in cart_items:
                        product = item.get("product", {})
                        products.append({
                            "id": product.get("id"),
                            "name": product.get("name"),
                            "price": product.get("price"),
                            "brand": product.get("brand")
                        })
                    source_list = "cart"
                    print(f"   Using cart items: {len(products)}")
                    
                elif last_displayed_list == "wishlist" and wishlist_items:
                    # Wishlist items đã được transform trong finish.py
                    products = wishlist_items
                    source_list = "wishlist"
                    print(f"   Using wishlist items: {len(products)}")
                    
                elif search_results.get("data"):
                    products = search_results["data"]
                    source_list = "search"
                    print(f"   Using search results: {len(products)}")
                
                print(f"   Selection detected - Index: {selected_index}, Products available: {len(products)}")
                
                # Validate index and get product
                if 1 <= selected_index <= len(products):
                    selected_product = products[selected_index - 1]  # Convert to 0-based
                    
                    print(f"   Selected product: {selected_product.get('name', 'N/A')} (ID: {selected_product.get('id')})")
                    
                    # Update state
                    new_state = dict(state)
                    new_state["selected_product"] = selected_product
                    new_state["pending_action"] = {
                        "type": result.get("action", "view_detail"),
                        "quantity": result.get("quantity", 1),
                        "source_list": source_list  # Track where the selection came from
                    }
                    
                    return new_state
                else:
                    print(f"   Invalid index: {selected_index} (max: {len(products)})")

    except Exception as e:
        print(f"Error handling selection: {e}")
    
    return state