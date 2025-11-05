from typing import TypedDict, List, Dict, Any, Optional
from langchain_core.messages import BaseMessage

class ChatState(TypedDict, total=False):
    messages: List[BaseMessage]
    filters: Optional[Dict[str, Any]]
    selected_product: Optional[Dict[str, Any]]
    cart_state: Optional[Dict[str, Any]]
    cart_items: Optional[List[Dict[str, Any]]]
    wishlist_items: Optional[List[Dict[str, Any]]]
    last_displayed_list: Optional[str]  # "cart", "wishlist", "search"
    last_intent: Optional[str]
    user_token: Optional[str]
    username: Optional[str]  # extracted from JWT token
    suggested_url: Optional[str]
    should_finish: Optional[bool]
    search_results: Optional[Dict[str, Any]]
    presentation_response: Optional[str]
    tool_result: Optional[Dict[str, Any]]
    pending_action: Optional[Dict[str, Any]]
    final_response: Optional[str]
    action_data: Optional[Dict[str, Any]]
    target_product_id: Optional[str]
    target_order_id: Optional[str]