from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, SystemMessage
from lg.state import ChatState
from config import config
from textwrap import dedent
import json
import re

# Cập nhật để sử dụng Grok
llm = ChatOpenAI(
    model=config.GROK_MODEL,
    api_key=config.GROK_API_KEY,
    base_url=config.GROK_BASE_URL
)

CATEGORIES = [
    "Voucher - Dịch vụ", "Thời Trang Cho Mẹ Và Bé", "Chăm sóc nhà cửa", 
    "Làm Đẹp - Sức Khỏe", "Giày - Dép nam", "Giày - Dép nữ", 
    "Cross Border - Hàng Quốc Tế", "Điện Thoại - Máy Tính Bảng", 
    "Máy Ảnh - Máy Quay Phim", "Thiết Bị Số - Phụ Kiện Số", 
    "Laptop - Máy Vi Tính - Linh kiện", "Điện Gia Dụng", 
    "Nhà Cửa - Đời Sống", "Thể Thao - Dã Ngoại", "Đồ Chơi - Mẹ & Bé", 
    "Phụ kiện thời trang", "Túi thời trang nam", "Điện Tử - Điện Lạnh", 
    "Bách Hóa Online", "NGON", "Balo và Vali", "Nhà Sách Tiki", 
    "Đồng hồ và Trang sức", "Ô Tô - Xe Máy - Xe Đạp", "Thời trang nam", 
    "Thời trang nữ", "Túi thời trang nữ"
]

async def analyze_intent(state: ChatState) -> ChatState:
    """Analyze user intent and extract filters"""
    
    user_message = state["messages"][-1].content if state["messages"] else ""
    has_previous_results = bool(state.get("search_results", {}).get("data"))
    last_displayed_list = state.get("last_displayed_list")
    cart_items = state.get("cart_items", [])
    wishlist_items = state.get("wishlist_items", [])
    
    # Clear old states để tránh lẫn lộn
    if state.get("last_intent") not in ["search_product", "view_cart", "view_orders", "view_wishlist"]:
        state.pop("search_results", None)
        state.pop("presentation_response", None)
    
    categories_list = "\n".join([f"- {cat}" for cat in CATEGORIES])
    
    system_prompt = dedent(f"""\
        Bạn là AI assistant phân tích ý định người dùng trong hệ thống e-commerce.
        
        Context: 
        - {"Có kết quả tìm kiếm trước đó" if has_previous_results else "Không có kết quả tìm kiếm trước"}
        - Danh sách hiển thị cuối: {last_displayed_list or "None"}
        - Có {len(cart_items)} sản phẩm trong cart context
        - Có {len(wishlist_items)} sản phẩm trong wishlist context
        
        **QUAN TRỌNG - PHÂN BIỆT SELECTION VÀ PRODUCT ID:**
        
        **SELECTION (từ danh sách hiện tại):**
        - "sản phẩm 1", "sản phẩm đầu tiên", "cái thứ 2" → intent: "other" (để route qua selection)
        - "xóa sản phẩm 1", "thêm sản phẩm 2 vào giỏ" → intent: "other" (để route qua selection)
        - "chi tiết sản phẩm 3" → intent: "other" (để route qua selection)
        
        **PRODUCT ID CỤ THỂ:**
        - "xóa sản phẩm có id ABC123" → intent: "remove_from_cart" + productId: "ABC123"
        - "product/67890abc" → intent tương ứng + productId: "67890abc"
        
        **QUY TẮC:**
        - Nếu user nhắc đến "sản phẩm [số]" và có context danh sách → intent: "other"
        - Chỉ dùng intent cụ thể khi user cung cấp ID thật (dài, có chữ cái, không phải số đơn giản 1-10)
        
        Danh sách categories có sẵn:
        {categories_list}
        
        **DANH SÁCH ĐẦY ĐỦ CÁC INTENT:**
        
        **1. PRODUCT INTENTS:**
        - search_product: tìm kiếm sản phẩm
        - get_product_detail: xem chi tiết sản phẩm cụ thể (khi có productId thật)
        - get_top_rated: sản phẩm đánh giá cao
        - get_top_selling: sản phẩm bán chạy
        - get_new_arrivals: sản phẩm mới
        - get_brands: xem danh sách thương hiệu
        - get_categories: xem danh sách danh mục
        
        **2. CART INTENTS:**
        - view_cart: xem giỏ hàng
        - add_to_cart: thêm sản phẩm vào giỏ hàng (chỉ khi có productId thật)
        - remove_from_cart: xóa sản phẩm khỏi giỏ hàng (chỉ khi có productId thật)
        - clear_cart: xóa toàn bộ giỏ hàng
        - prepare_checkout: chuẩn bị thanh toán
        
        **3. ORDER INTENTS:**
        - view_orders: xem danh sách đơn hàng
        - get_order_detail: xem chi tiết đơn hàng
        - create_order: tạo đơn hàng từ giỏ hàng
        - cancel_order: hủy đơn hàng
        
        **4. WISHLIST INTENTS:**
        - view_wishlist: xem danh sách yêu thích
        - add_to_wishlist: thêm sản phẩm vào wishlist (chỉ khi có productId thật)
        - remove_from_wishlist: xóa sản phẩm khỏi wishlist (chỉ khi có productId thật)
        
        **5. OTHER:**
        - other: selection từ danh sách, các trường hợp khác
        
        Trả về JSON format:
        {{
            "intent": "intent_name",
            "filters": {{
                "query": "từ khóa",
                "category": "category chính xác từ list",
                "minPrice": number,
                "maxPrice": number,
                "brand": "tên brand"
            }},
            "productId": "product_id_if_any",
            "orderId": "order_id_if_any", 
            "actionData": {{"quantity": 1, "productId": "", "orderId": ""}},
            "confidence": 0.0-1.0
        }}
        
        CHỈ TRẢ VỀ JSON, KHÔNG CÓ TEXT KHÁC.
    """)
    
    messages = [
        SystemMessage(content=system_prompt),
        HumanMessage(content=f"Tin nhắn: {user_message}")
    ]
    
    try:
        print(f"Analyzing intent for: {user_message}")
        response = await llm.ainvoke(messages)
        print(f"LLM Response: {response.content}")
        
        content = response.content.strip()
        json_match = re.search(r'\{.*\}', content, re.DOTALL)
        if json_match:
            result = json.loads(json_match.group())
        else:
            result = {"intent": "other", "filters": {}}
            print(f"Could not parse JSON from: {content}")
        
        state["last_intent"] = result.get("intent")
        state["filters"] = result.get("filters", {})
        state["action_data"] = result.get("actionData", {})
        
        if result.get("productId"):
            state["target_product_id"] = result["productId"]
        if result.get("orderId"):
            state["target_order_id"] = result["orderId"]
        
        print(f"Parsed intent: {state['last_intent']}, filters: {state['filters']}")
        
        return state
        
    except Exception as e:
        print(f"Error parsing intent: {e}")
        state["last_intent"] = "other"
        state["filters"] = {}
        return state