from langchain_openai import ChatOpenAI
from langchain_core.messages import SystemMessage, HumanMessage
from lg.state import ChatState
from config import config
from textwrap import dedent

llm = ChatOpenAI(
    model=config.GROK_MODEL,
    api_key=config.GROK_API_KEY,
    base_url=config.GROK_BASE_URL
)

async def finish(state: ChatState) -> ChatState:
    """Generate final response and finish conversation"""
    
    intent = state.get("last_intent")
    presentation_response = state.get("presentation_response")
    tool_result = state.get("tool_result")
    
    new_state = dict(state)
    
    # Ưu tiên presentation_response cho search results
    if presentation_response and intent == "search_product":
        final_response = presentation_response
        
    elif tool_result:
        # Xử lý các response khác nhau theo intent
        
        if intent == "view_cart":
            final_response = await handle_cart_response(tool_result, new_state)
            
        elif intent == "view_orders":
            final_response = await handle_orders_response(tool_result)
            
        elif intent == "view_wishlist":
            final_response = await handle_wishlist_response(tool_result, new_state)
            
        elif intent == "get_brands":
            final_response = await handle_brands_response(tool_result)
            
        elif intent == "get_categories":
            final_response = await handle_categories_response(tool_result)
            
        elif intent == "get_order_detail":
            final_response = await handle_order_detail_response(tool_result)
            
        elif intent in ["add_to_cart", "remove_from_cart", "clear_cart"]:
            final_response = await handle_cart_action_response(intent, tool_result)
            
        elif intent in ["add_to_wishlist", "remove_from_wishlist"]:
            final_response = await handle_wishlist_action_response(intent, tool_result)
            
        elif intent in ["create_order", "cancel_order"]:
            final_response = await handle_order_action_response(intent, tool_result)
            
        elif intent == "prepare_checkout":
            final_response = await handle_checkout_response(tool_result)
            
        else:
            # Generic handler cho các intent khác
            final_response = await handle_generic_response(intent, tool_result)
    else:
        # Fallback responses
        final_response = get_fallback_response(intent)
    
    return {
        **new_state,
        "final_response": final_response,
        "should_finish": True
    }

# Helper functions cho từng loại response
async def handle_cart_response(tool_result, state):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Giỏ hàng của bạn đang trống. Hãy tìm kiếm và thêm sản phẩm vào giỏ hàng nhé!"
    
    cart_data = tool_result.get("data", {})
    items = cart_data.get("items", [])
    total = cart_data.get("pricing", {}).get("total", 0)
    
    if not items:
        return "Giỏ hàng của bạn đang trống."
    
    # Lưu cart items vào state
    state["cart_items"] = items
    state["last_displayed_list"] = "cart"
    
    cart_text = f"🛒 **Giỏ hàng của bạn có {len(items)} sản phẩm:**\n\n"
    for idx, item in enumerate(items, start=1):
        product = item.get("product", {})
        cart_text += f"{idx}. **{product.get('name', 'N/A')}**\n"
        cart_text += f"   💰 {item.get('quantity', 1)} x {product.get('price', 0):,.0f}đ = {item.get('quantity', 1) * product.get('price', 0):,.0f}đ\n\n"
    
    cart_text += f"💰 **Tổng cộng: {total:,.0f}đ**\n\n"
    cart_text += "🔍 **Các thao tác:**\n"
    cart_text += "- \"Chi tiết sản phẩm 1\" để xem chi tiết\n"
    cart_text += "- \"Xóa sản phẩm 1\" để xóa khỏi giỏ\n"
    cart_text += "- \"Xóa hết giỏ hàng\" để làm trống giỏ\n"
    cart_text += "- \"Thanh toán\" để tiến hành đặt hàng"
    
    return cart_text

async def handle_orders_response(tool_result):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Bạn chưa có đơn hàng nào. Hãy mua sắm ngay! 🛍️"
    
    orders = tool_result.get("data", [])
    if not orders:
        return "Bạn chưa có đơn hàng nào."
    
    orders_text = f"📦 **Bạn có {len(orders)} đơn hàng:**\n\n"
    for idx, order in enumerate(orders, start=1):
        order_id = order.get("id", "N/A")
        status = order.get("status", "N/A")
        total = order.get("total", 0)
        created_at = order.get("createdAt", "")
        
        orders_text += f"{idx}. **Đơn hàng #{order_id}**\n"
        orders_text += f"   📅 {created_at[:10]} - 💰 {total:,.0f}đ\n"
        orders_text += f"   📋 Trạng thái: {status}\n\n"
    
    orders_text += "🔍 **Các thao tác:**\n"
    orders_text += "- \"Chi tiết đơn hàng [order_id]\" để xem chi tiết\n"
    orders_text += "- \"Hủy đơn hàng [order_id]\" để hủy đơn"
    
    return orders_text

async def handle_wishlist_response(tool_result, state):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Danh sách yêu thích của bạn đang trống. ❤️"
    
    data = tool_result.get("data", {})
    items = data.get("items", [])
    total = data.get("total", 0)
    
    if not items:
        return "Danh sách yêu thích trống."
    
    # Debug: check data structure
    print(f"🔍 Wishlist data structure: total={total}, items_count={len(items)}")
    if items:
        print(f"🔍 First item structure: {items[0].get('productName', 'N/A')}")
    
    # Transform items để tương thích với selection handler
    transformed_items = []
    for item in items:
        product = item.get("product", {})
        transformed_item = {
            "id": product.get("id", item.get("productId", "")),
            "name": product.get("name", item.get("productName", "N/A")),
            "price": product.get("price", 0),
            "brand": product.get("brand", {}),
            "originalPrice": product.get("originalPrice", 0),
            "ratingAverage": product.get("ratingAverage", 0),
            "reviewCount": product.get("reviewCount", 0),
            "inventoryStatus": product.get("inventoryStatus", ""),
            "images": product.get("images", [])
        }
        transformed_items.append(transformed_item)
    
    # Lưu wishlist items vào state (dùng transformed items)
    state["wishlist_items"] = transformed_items
    state["last_displayed_list"] = "wishlist"
    
    wishlist_text = f"❤️ **Danh sách yêu thích có {total} sản phẩm:**\n\n"
    
    for idx, item in enumerate(items[:5], start=1):  # Limit 5 items
        product = item.get("product", {})
        name = product.get("name", item.get("productName", "N/A"))
        price = product.get("price", 0)
        original_price = product.get("originalPrice", 0)
        brand = product.get("brand", {})
        brand_name = brand.get("name", "N/A") if isinstance(brand, dict) else "N/A"
        rating = product.get("ratingAverage", 0)
        inventory_status = product.get("inventoryStatus", "")
        
        # Hiển thị thông tin sản phẩm
        wishlist_text += f"{idx}. **{name}**\n"
        
        # Giá
        if original_price > price > 0:
            discount = int((original_price - price) / original_price * 100)
            wishlist_text += f"   💰 {price:,.0f}đ ~~{original_price:,.0f}đ~~ (-{discount}%)\n"
        elif price > 0:
            wishlist_text += f"   💰 {price:,.0f}đ\n"
        
        # Thương hiệu và đánh giá
        info_line = f"   🏷️ {brand_name}"
        if rating > 0:
            info_line += f" | ⭐ {rating:.1f}"
        
        # Trạng thái kho
        if inventory_status == "out_of_stock":
            info_line += " | ❌ Hết hàng"
        elif inventory_status == "available":
            info_line += " | ✅ Còn hàng"
        
        wishlist_text += f"{info_line}\n\n"
    
    if total > 5:
        wishlist_text += f"... và {total - 5} sản phẩm khác\n\n"
    
    wishlist_text += "🔍 **Các thao tác:**\n"
    wishlist_text += "- \"Chi tiết sản phẩm 1\" để xem chi tiết\n"
    wishlist_text += "- \"Thêm sản phẩm 1 vào giỏ\" để mua\n"
    wishlist_text += "- \"Xóa sản phẩm 1 khỏi wishlist\" để xóa"
    
    return wishlist_text

async def handle_brands_response(tool_result):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Không thể lấy danh sách thương hiệu."
    
    brands = tool_result.get("data", [])
    if not brands:
        return "Không có thương hiệu nào."
    
    brands_text = f"🏷️ **Danh sách thương hiệu ({len(brands)} thương hiệu):**\n\n"
    for brand in brands:
        name = brand.get("name", "N/A")
        brands_text += f"• {name}\n"
    
    if len(brands) > 10:
        brands_text += f"\n... và {len(brands) - 10} thương hiệu khác"
    
    brands_text += "\n\n💡 Tìm kiếm theo thương hiệu: \"Tìm điện thoại Samsung\""
    
    return brands_text

async def handle_categories_response(tool_result):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Không thể lấy danh sách danh mục."
    
    categories = tool_result.get("data", [])
    if not categories:
        return "Không có danh mục nào."
    
    categories_text = f"📂 **Danh sách danh mục ({len(categories)} danh mục):**\n\n"
    for category in categories:
        name = category.get("name", "N/A")
        categories_text += f"• {name}\n"
    
    if len(categories) > 10:
        categories_text += f"\n... và {len(categories) - 10} danh mục khác"
    
    categories_text += "\n\n💡 Tìm kiếm theo danh mục: \"Tìm laptop\" hoặc \"Sản phẩm trong danh mục điện thoại\""
    
    return categories_text

async def handle_cart_action_response(intent, tool_result):
    if tool_result.get("status") == "success":
        if intent == "add_to_cart":
            return "✅ Đã thêm sản phẩm vào giỏ hàng thành công!"
        elif intent == "remove_from_cart":
            return "✅ Đã xóa sản phẩm khỏi giỏ hàng!"
        elif intent == "clear_cart":
            return "✅ Đã xóa toàn bộ giỏ hàng!"
    else:
        error_msg = tool_result.get("message", "Có lỗi xảy ra")
        return f"❌ {error_msg}"

async def handle_wishlist_action_response(intent, tool_result):
    if tool_result.get("status") == "success":
        if intent == "add_to_wishlist":
            return "✅ Đã thêm sản phẩm vào danh sách yêu thích!"
        elif intent == "remove_from_wishlist":
            return "✅ Đã xóa sản phẩm khỏi danh sách yêu thích!"
    else:
        error_msg = tool_result.get("message", "Có lỗi xảy ra")
        return f"❌ {error_msg}"

async def handle_order_action_response(intent, tool_result):
    if tool_result.get("status") == "success":
        if intent == "create_order":
            order_id = tool_result.get("data", {}).get("id", "")
            return f"✅ Đã tạo đơn hàng thành công! Mã đơn hàng: {order_id}"
        elif intent == "cancel_order":
            return "✅ Đã hủy đơn hàng thành công!"
    else:
        error_msg = tool_result.get("message", "Có lỗi xảy ra")
        return f"❌ {error_msg}"

async def handle_checkout_response(tool_result):
    if tool_result.get("status") == "success":
        return "✅ Giỏ hàng đã sẵn sàng thanh toán! Bạn có thể tiến hành đặt hàng."
    else:
        error_msg = tool_result.get("message", "Có lỗi xảy ra")
        return f"❌ Không thể chuẩn bị thanh toán: {error_msg}"

async def handle_order_detail_response(tool_result):
    if tool_result.get("status") == "error" or not tool_result.get("data"):
        return "Không thể lấy thông tin chi tiết đơn hàng."
    
    order = tool_result.get("data", {})
    order_id = order.get("id", "N/A")
    status = order.get("status", "N/A")
    total = order.get("total", 0)
    items = order.get("items", [])
    
    detail_text = f"📦 **Chi tiết đơn hàng #{order_id}**\n\n"
    detail_text += f"📋 **Trạng thái:** {status}\n"
    detail_text += f"💰 **Tổng tiền:** {total:,.0f}đ\n\n"
    
    if items:
        detail_text += "**Sản phẩm trong đơn:**\n"
        for idx, item in enumerate(items, start=1):
            product = item.get("product", {})
            quantity = item.get("quantity", 1)
            price = product.get("price", 0)
            detail_text += f"{idx}. {product.get('name', 'N/A')}\n"
            detail_text += f"   {quantity} x {price:,.0f}đ = {quantity * price:,.0f}đ\n\n"
    
    return detail_text

async def handle_generic_response(intent, tool_result):
    system_prompt = dedent("""\
        Bạn là AI assistant e-commerce. Tạo phản hồi ngắn gọn dựa trên kết quả API.
        
        Quy tắc:
        - Nếu kết quả error hoặc empty, thông báo rõ ràng
        - Xác nhận hành động đã thực hiện
        - Trả lời bằng tiếng Việt
        - Gợi ý hành động tiếp theo nếu cần
    """)
    
    messages = [
        SystemMessage(content=system_prompt),
        HumanMessage(content=f"Intent: {intent}\nKết quả API: {tool_result}")
    ]
    
    response = await llm.ainvoke(messages)
    return response.content

def get_fallback_response(intent):
    fallback_responses = {
        "search_product": "Xin lỗi, không tìm thấy sản phẩm phù hợp. Vui lòng thử từ khóa khác.",
        "view_cart": "Vui lòng đăng nhập để xem giỏ hàng.",
        "view_orders": "Vui lòng đăng nhập để xem đơn hàng.",
        "view_wishlist": "Vui lòng đăng nhập để xem danh sách yêu thích.",
    }
    
    return fallback_responses.get(intent, 
        "Tôi có thể giúp bạn:\n"
        "🔍 Tìm kiếm sản phẩm\n"
        "🛒 Quản lý giỏ hàng\n"
        "📦 Xem đơn hàng\n"
        "❤️ Quản lý wishlist\n"
        "🏷️ Xem thương hiệu & danh mục\n\n"
        "Bạn cần hỗ trợ gì?"
    )