from lg.state import ChatState
from lg.tools.cart import CartTools
from lg.tools.order import OrderTools
from lg.tools.wishlist import WishlistTools
from lg.tools.product import ProductTools

async def call_tool(state: ChatState) -> ChatState:
    """Execute API tool calls based on intent and actions"""
    
    intent = state.get("last_intent")
    pending_action = state.get("pending_action")
    token = state.get("user_token")
    username = state.get("username")
    action_data = state.get("action_data", {})
    target_product_id = state.get("target_product_id")
    target_order_id = state.get("target_order_id")
    
    print(f"🔧 Tool call - Intent: {intent}")
    print(f"   Pending action: {pending_action}")
    print(f"   Token: {'Present' if token else 'None'}")
    print(f"   Username: {username}")
    
    try:
        if pending_action:
            if "pending_action" in state:
                del state["pending_action"]

            action_type = pending_action.get("type")
            selected_product = state.get("selected_product")
            source_list = pending_action.get("source_list")
            
            print(f"   Processing pending action: {action_type} from {source_list}")
            
            if action_type == "add_to_cart" and selected_product and token and username:
                product_id = selected_product["id"]
                quantity = pending_action.get("quantity", 1)
                print(f"   Adding to cart: Product ID {product_id}, Quantity {quantity}")
                result = await CartTools.add_to_cart(token, username, product_id, quantity)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                
            elif action_type == "add_to_wishlist" and selected_product and token:
                product_id = selected_product["id"]
                print(f"   Adding to wishlist: Product ID {product_id}")
                result = await WishlistTools.add_to_wishlist(token, product_id)
                state["tool_result"] = result
                state["suggested_url"] = "/wishlist"
                
            elif action_type == "view_detail" and selected_product:
                product_id = selected_product["id"]
                print(f"   Getting product detail: Product ID {product_id}")
                result = await ProductTools.get_product_detail(product_id, token)
                state["tool_result"] = result
                state["suggested_url"] = f"/product/{product_id}"
                
            elif action_type == "remove_from_cart" and selected_product and token and username:
                product_id = selected_product["id"]
                print(f"   Removing from cart: Product ID {product_id}")
                result = await CartTools.remove_from_cart(token, username, product_id)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                
                # Update cart_items trong state sau khi xóa
                cart_items = state.get("cart_items", [])
                updated_cart_items = [item for item in cart_items if item.get("product", {}).get("id") != product_id]
                state["cart_items"] = updated_cart_items
                
            elif action_type == "remove_from_wishlist" and selected_product and token:
                product_id = selected_product["id"]
                result = await WishlistTools.remove_from_wishlist(token, product_id)
                state["tool_result"] = result
                state["suggested_url"] = "/wishlist"
                
                wishlist_items = state.get("wishlist_items", [])
                updated_wishlist_items = [item for item in wishlist_items if item.get("id") != product_id]
                state["wishlist_items"] = updated_wishlist_items
                
            elif not token and action_type in ["add_to_cart", "add_to_wishlist", "remove_from_cart", "remove_from_wishlist"]:
                state["tool_result"] = {
                    "status": "error", 
                    "message": "Vui lòng đăng nhập để thực hiện chức năng này"
                }
            elif token and not username and action_type in ["add_to_cart", "remove_from_cart"]:
                state["tool_result"] = {
                    "status": "error",
                    "message": "Không thể xác định thông tin người dùng. Vui lòng đăng nhập lại."
                }
        
        else:
            print(f"   Processing direct intent: {intent}")
            
            if intent == "get_product_detail" and target_product_id:
                result = await ProductTools.get_product_detail(target_product_id, token)
                state["tool_result"] = result
                state["suggested_url"] = f"/product/{target_product_id}"
                
            elif intent == "get_brands":
                result = await ProductTools.get_brands(token)
                state["tool_result"] = result
                
            elif intent == "get_categories":
                result = await ProductTools.get_categories(token)
                state["tool_result"] = result
            
            # CART INTENTS - now require username
            elif intent == "view_cart" and token and username:
                result = await CartTools.get_cart(token, username)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                
            elif intent == "add_to_cart" and target_product_id and token and username:
                quantity = action_data.get("quantity", 1)
                result = await CartTools.add_to_cart(token, username, target_product_id, quantity)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                
            elif intent == "remove_from_cart" and target_product_id and token and username:
                result = await CartTools.remove_from_cart(token, username, target_product_id)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                
            elif intent == "clear_cart" and token and username:
                result = await CartTools.clear_cart(token, username)
                state["tool_result"] = result
                state["suggested_url"] = "/cart"
                # Clear cart_items trong state
                state["cart_items"] = []
                
            elif intent == "prepare_checkout" and token and username:
                result = await CartTools.prepare_checkout(token, username)
                state["tool_result"] = result
                state["suggested_url"] = "/checkout"
            
            # ORDER INTENTS
            elif intent == "view_orders" and token:
                result = await OrderTools.get_user_orders(token)
                state["tool_result"] = result
                state["suggested_url"] = "/orders"
                
            elif intent == "get_order_detail" and target_order_id and token:
                result = await OrderTools.get_order_detail(token, target_order_id)
                state["tool_result"] = result
                state["suggested_url"] = f"/order/{target_order_id}"
                
            elif intent == "create_order" and token:
                order_data = action_data
                result = await OrderTools.create_order_from_cart(token, order_data)
                state["tool_result"] = result
                state["suggested_url"] = "/orders"
                
            elif intent == "cancel_order" and target_order_id and token:
                result = await OrderTools.cancel_order(token, target_order_id)
                state["tool_result"] = result
                state["suggested_url"] = "/orders"
            
            elif intent in ["view_cart", "add_to_cart", "remove_from_cart", "clear_cart", "prepare_checkout",
                            "view_orders", "get_order_detail", "create_order", "cancel_order"] and not token:
                state["tool_result"] = {
                    "status": "error",
                    "message": "Vui lòng đăng nhập để thực hiện chức năng này"
                }
            
            elif intent in ["view_cart", "add_to_cart", "remove_from_cart", "clear_cart", "prepare_checkout"] and token and not username:
                state["tool_result"] = {
                    "status": "error",
                    "message": "Không thể xác định thông tin người dùng. Vui lòng đăng nhập lại."
                }
    
    except Exception as e:
        print(f"❌ Error calling tool: {e}")
        import traceback
        traceback.print_exc()
        state["tool_result"] = {"status": "error", "message": f"Có lỗi xảy ra: {str(e)}"}
    
    return state