import logging
import uuid
from typing import Optional, Annotated
from langchain_core.tools import tool, InjectedToolArg
from langchain_core.runnables import RunnableConfig
from websocket_mcp import mcp_manager
from api_tools import ProductAPITools, CartAPITools, OrderAPITools, CustomerAPITools

logger = logging.getLogger(__name__)


def _get_context(config: RunnableConfig) -> tuple[str, str, Optional[str], Optional[str]]:
    cfg = config.get("configurable", {})
    username = cfg.get("username", "")
    email = username if username and "@" in username else None
    return cfg.get("user_id", ""), username, cfg.get("auth_token"), email


def _format_selector(element_id: str) -> str:
    return element_id if element_id.startswith("#") else f"#{element_id}"


async def _send_browser_action(user_id: str, action: str, element_id: str = None, value: str = None) -> dict:
    action_data = {"id": str(uuid.uuid4()), "action": action}
    if element_id:
        action_data["element_id"] = element_id
    if value is not None:
        action_data["value"] = value
    return await mcp_manager.send_action_and_wait(user_id, action_data)


@tool
async def get_elements(
    action: Optional[str] = None,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Lấy danh sách elements trên trang hiện tại. action: 'click', 'type', hoặc None để lấy tất cả."""
    user_id, _, _, _ = _get_context(config)
    logger.info(f"get_elements tool called with user_id={user_id}, action={action}")
    try:
        return await mcp_manager.get_elements(user_id, action)
    except Exception as e:
        logger.error(f"get_elements error: {e}")
        return {"success": False, "message": str(e)}


@tool
async def click(
    element_id: str,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Click vào element theo ID."""
    user_id, _, _, _ = _get_context(config)
    logger.info(f"click tool - user_id={user_id}, element_id={element_id}")
    return await _send_browser_action(user_id, "click", element_id)


@tool
async def fill(
    element_id: str,
    value: str,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Điền text vào input/textarea theo ID."""
    user_id, _, _, _ = _get_context(config)
    logger.info(f"fill tool - user_id={user_id}, element_id={element_id}, value={value}")
    return await _send_browser_action(user_id, "fill", element_id, value)


@tool
async def scroll(
    direction: str = "down",
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Scroll trang. direction: 'up' hoặc 'down'."""
    user_id, _, _, _ = _get_context(config)
    logger.info(f"scroll tool - user_id={user_id}, direction={direction}")
    return await _send_browser_action(user_id, "scroll", value=direction)


@tool
async def navigate(
    url: str,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Điều hướng đến URL. 
    
    Args:
        url: URL để navigate, có thể là:
            - Relative path: '/products', '/cart', '/product/123'
            - Full URL: 'https://example.com'
            - Query params: '/search?q=laptop'
    
    Examples:
        - navigate('/products') - Đến trang products
        - navigate('/cart') - Đến trang giỏ hàng
        - navigate('/product/abc-123') - Đến chi tiết sản phẩm
        - navigate('/search?q=laptop') - Tìm kiếm laptop
    """
    user_id, _, _, _ = _get_context(config)
    logger.info(f"navigate tool - user_id={user_id}, url={url}")
    return await _send_browser_action(user_id, "navigate", value=url)


@tool
async def search_products(
    query: Optional[str] = None,
    category_id: Optional[str] = None,
    min_price: Optional[float] = None,
    max_price: Optional[float] = None
) -> dict:
    """Tìm kiếm sản phẩm. Response chứa list products với các field quan trọng:
    - id: ID sản phẩm (dùng làm itemNo khi thêm vào giỏ)
    - name: Tên sản phẩm (dùng làm itemName)
    - price: Giá sản phẩm (dùng làm itemPrice)
    - stockQuantity: Số lượng tồn kho
    """
    return await ProductAPITools.search_products(query, category_id, min_price, max_price)


@tool
async def get_product_detail(product_id: str) -> dict:
    """Lấy chi tiết sản phẩm theo ID (GUID format).
    Response chứa: id, name, price, description, stockQuantity, images, category, brand.
    """
    return await ProductAPITools.get_product_detail(product_id)


@tool
async def get_categories() -> dict:
    """Lấy danh sách danh mục sản phẩm."""
    return await ProductAPITools.get_categories()


@tool
async def get_brands() -> dict:
    """Lấy danh sách thương hiệu."""
    return await ProductAPITools.get_brands()


@tool
async def get_cart(
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Lấy giỏ hàng của user hiện tại."""
    _, username, _, _ = _get_context(config)
    return await CartAPITools.get_cart(username)


@tool
async def update_cart(
    items: list,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Thêm/cập nhật sản phẩm trong giỏ hàng.
    
    Args:
        items: Danh sách sản phẩm, mỗi item PHẢI có đủ các field:
            - itemNo: ID sản phẩm (từ product.no khi search)
            - itemName: Tên sản phẩm (từ product.name)
            - itemPrice: Giá sản phẩm (từ product.price)
            - quantity: Số lượng muốn thêm
            
    Example: items=[{"itemNo": "LAPTOP-001", "itemName": "Laptop", "itemPrice": 15000000, "quantity": 1}]
    """
    _, username, _, email = _get_context(config)
    return await CartAPITools.update_cart(username, items, email)


@tool
async def add_to_cart(
    product_id: str,
    quantity: int = 1,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Thêm sản phẩm vào giỏ hàng theo product ID. Tool này tự động lấy thông tin sản phẩm.
    
    Args:
        product_id: ID sản phẩm (GUID từ kết quả search_products hoặc get_product_detail)
        quantity: Số lượng muốn thêm (mặc định = 1)
    """
    _, username, _, email = _get_context(config)
    
    if not username:
        return {"success": False, "message": "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng"}
    
    product = await ProductAPITools.get_product_detail(product_id)
    logger.info(f"add_to_cart - product response: {product}")
    
    if not product.get("isSuccess"):
        return {"success": False, "message": f"Không tìm thấy sản phẩm với ID: {product_id}"}
    
    data = product.get("data", {})
    item = {
        "itemNo": data.get("no") or str(data.get("id", product_id)),
        "itemName": data.get("name", "Unknown"),
        "itemPrice": float(data.get("price", 0)),
        "quantity": quantity
    }
    logger.info(f"add_to_cart - item to add: {item}")
    
    current_cart = await CartAPITools.get_cart(username)
    current_items = current_cart.get("data", {}).get("items", []) if current_cart.get("isSuccess") else []
    
    existing = next((i for i in current_items if i.get("itemNo") == item["itemNo"]), None)
    if existing:
        existing["quantity"] += quantity
    else:
        current_items.append(item)
    
    logger.info(f"add_to_cart - final items: {current_items}")
    return await CartAPITools.update_cart(username, current_items, email)


@tool
async def checkout_cart(
    first_name: str,
    last_name: str,
    email: str,
    shipping_address: str,
    invoice_address: Optional[str] = None,
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Thanh toán giỏ hàng. Tất cả các field đều bắt buộc trừ invoice_address.
    
    Args:
        first_name: Tên người nhận
        last_name: Họ người nhận  
        email: Email nhận thông báo đơn hàng
        shipping_address: Địa chỉ giao hàng
        invoice_address: Địa chỉ hóa đơn (mặc định = shipping_address)
    """
    _, username, _, _ = _get_context(config)
    return await CartAPITools.checkout_cart(username, first_name, last_name, email, shipping_address, invoice_address)


@tool
async def get_user_orders(
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Lấy danh sách đơn hàng của user."""
    _, username, _, _ = _get_context(config)
    return await OrderAPITools.get_user_orders(username)


@tool
async def get_order_detail(order_id: int) -> dict:
    """Lấy chi tiết đơn hàng theo ID (số nguyên).
    Response chứa: id, status, totalPrice, orderDate, items, shippingAddress.
    """
    return await OrderAPITools.get_order_detail(order_id)


@tool
async def get_user_info(
    config: Annotated[RunnableConfig, InjectedToolArg] = None
) -> dict:
    """Lấy thông tin profile của user hiện tại."""
    user_id, _, _, _ = _get_context(config)
    return await CustomerAPITools.get_user_info(user_id)


def get_tools() -> list:
    return [
        get_elements, click, fill, scroll, navigate,
        search_products, get_product_detail, get_categories, get_brands,
        get_cart, add_to_cart, update_cart, checkout_cart,
        get_user_orders, get_order_detail,
        get_user_info,
    ]
