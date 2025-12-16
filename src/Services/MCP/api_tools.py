import httpx
import logging
import os
from typing import Dict, Any, Optional, List

logger = logging.getLogger(__name__)

# API URLs from environment
GATEWAY_URL = os.getenv("API_GATEWAY_URL", "http://localhost:5000")
PRODUCT_API_URL = os.getenv("PRODUCT_API_URL", "http://localhost:5004")
BASKET_API_URL = os.getenv("BASKET_API_URL", "http://localhost:5003")
ORDER_API_URL = os.getenv("ORDER_API_URL", "http://localhost:5002")
CUSTOMER_API_URL = os.getenv("CUSTOMER_API_URL", "http://localhost:5001")


class ProductAPITools:
    @staticmethod
    async def search_products(
        query: Optional[str] = None,
        category_id: Optional[str] = None,
        min_price: Optional[float] = None,
        max_price: Optional[float] = None,
        token: Optional[str] = None
    ) -> Dict[str, Any]:
        """Search products with filters"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                params = {}
                if query:
                    params["q"] = query
                if category_id:
                    params["categoryId"] = category_id
                if min_price:
                    params["minPrice"] = min_price
                if max_price:
                    params["maxPrice"] = max_price
                
                response = await client.get(
                    f"{PRODUCT_API_URL}/api/Products",
                    params=params,
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error searching products: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def get_product_detail(product_id: str, token: Optional[str] = None) -> Dict[str, Any]:
        """Get detailed product information"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{PRODUCT_API_URL}/api/Products/{product_id}",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting product detail: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def get_categories(token: Optional[str] = None) -> Dict[str, Any]:
        """Get all product categories"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{PRODUCT_API_URL}/api/Categories",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting categories: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def get_brands(token: Optional[str] = None) -> Dict[str, Any]:
        """Get all brands"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{PRODUCT_API_URL}/api/Brands",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting brands: {e}")
            return {"isSuccess": False, "message": str(e)}


class CartAPITools:
    """Tools for cart/basket operations"""
    
    @staticmethod
    async def get_cart(username: str, token: Optional[str] = None) -> Dict[str, Any]:
        """Get user's cart"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{BASKET_API_URL}/api/Baskets/{username}",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting cart: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def update_cart(username: str, items: List[Dict[str, Any]], token: Optional[str] = None) -> Dict[str, Any]:
        """Update cart (add/update items)"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                payload = {
                    "username": username,
                    "items": items
                }
                response = await client.post(
                    f"{BASKET_API_URL}/api/Baskets",
                    json=payload,
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error updating cart: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def checkout_cart(
        username: str,
        first_name: str,
        last_name: str,
        email: str,
        shipping_address: str,
        invoice_address: Optional[str] = None,
        token: Optional[str] = None
    ) -> Dict[str, Any]:
        """Checkout cart"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=30.0) as client:
                payload = {
                    "username": username,
                    "firstName": first_name,
                    "lastName": last_name,
                    "emailAddress": email,
                    "shippingAddress": shipping_address,
                    "invoiceAddress": invoice_address or shipping_address
                }
                response = await client.post(
                    f"{BASKET_API_URL}/api/Baskets/checkout",
                    json=payload,
                    headers=headers
                )
                return {"isSuccess": True, "status": response.status_code}
        except Exception as e:
            logger.error(f"Error checking out: {e}")
            return {"isSuccess": False, "message": str(e)}


class OrderAPITools:
    """Tools for order operations"""
    
    @staticmethod
    async def get_user_orders(username: str, token: Optional[str] = None) -> Dict[str, Any]:
        """Get user's orders"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{ORDER_API_URL}/api/v1/orders/users/{username}",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting orders: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def get_order_detail(order_id: int, token: Optional[str] = None) -> Dict[str, Any]:
        """Get order details"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{ORDER_API_URL}/api/v1/orders/{order_id}",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting order detail: {e}")
            return {"isSuccess": False, "message": str(e)}


class CustomerAPITools:
    """Tools for customer operations"""
    
    @staticmethod
    async def get_customer(username: str, token: Optional[str] = None) -> Dict[str, Any]:
        """Get customer information"""
        try:
            headers = {}
            if token:
                headers["Authorization"] = f"Bearer {token}"
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(
                    f"{CUSTOMER_API_URL}/api/Customers/{username}",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting customer: {e}")
            return {"isSuccess": False, "message": str(e)}
    
    @staticmethod
    async def get_user_info(token: str) -> Dict[str, Any]:
        """
        Get current user info from JWT token.
        Parses the token to extract user identity and returns customer profile.
        """
        try:
            if not token:
                return {"isSuccess": False, "message": "Token is required"}
            
            headers = {"Authorization": f"Bearer {token}"}
            
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.post(
                    f"{CUSTOMER_API_URL}/api/Customers/user-info",
                    headers=headers
                )
                return response.json()
        except Exception as e:
            logger.error(f"Error getting user info: {e}")
            return {"isSuccess": False, "message": str(e)}


# Tool registry for MCP discovery
ECOMMERCE_TOOLS = {
    "search_products": {
        "function": ProductAPITools.search_products,
        "description": "Search for products with optional filters (query, category, price range)",
        "parameters": {
            "type": "object",
            "properties": {
                "query": {"type": "string", "description": "Search query text"},
                "category_id": {"type": "string", "description": "Category GUID to filter"},
                "min_price": {"type": "number", "description": "Minimum price filter"},
                "max_price": {"type": "number", "description": "Maximum price filter"},
                "token": {"type": "string", "description": "Bearer token (optional)"}
            }
        }
    },
    "get_product_detail": {
        "function": ProductAPITools.get_product_detail,
        "description": "Get detailed information about a specific product",
        "parameters": {
            "type": "object",
            "properties": {
                "product_id": {"type": "string", "description": "Product GUID", "required": True},
                "token": {"type": "string", "description": "Bearer token (optional)"}
            },
            "required": ["product_id"]
        }
    },
    "get_categories": {
        "function": ProductAPITools.get_categories,
        "description": "Get all available product categories",
        "parameters": {
            "type": "object",
            "properties": {
                "token": {"type": "string", "description": "Bearer token (optional)"}
            }
        }
    },
    "get_brands": {
        "function": ProductAPITools.get_brands,
        "description": "Get all available brands",
        "parameters": {
            "type": "object",
            "properties": {
                "token": {"type": "string", "description": "Bearer token (optional)"}
            }
        }
    },
    "get_cart": {
        "function": CartAPITools.get_cart,
        "description": "Get user's shopping cart",
        "parameters": {
            "type": "object",
            "properties": {
                "username": {"type": "string", "description": "Username", "required": True}
            },
            "required": ["username"]
        }
    },
    "update_cart": {
        "function": CartAPITools.update_cart,
        "description": "Add or update items in cart",
        "parameters": {
            "type": "object",
            "properties": {
                "username": {"type": "string", "description": "Username", "required": True},
                "items": {
                    "type": "array",
                    "description": "Cart items with itemNo, itemName, quantity, itemPrice",
                    "required": True
                }
            },
            "required": ["username", "items"]
        }
    },
    "checkout_cart": {
        "function": CartAPITools.checkout_cart,
        "description": "Checkout user's cart and create order",
        "parameters": {
            "type": "object",
            "properties": {
                "username": {"type": "string", "required": True},
                "first_name": {"type": "string", "required": True},
                "last_name": {"type": "string", "required": True},
                "email": {"type": "string", "required": True},
                "shipping_address": {"type": "string", "required": True},
                "invoice_address": {"type": "string"}
            },
            "required": ["username", "first_name", "last_name", "email", "shipping_address"]
        }
    },
    "get_user_orders": {
        "function": OrderAPITools.get_user_orders,
        "description": "Get all orders for a user",
        "parameters": {
            "type": "object",
            "properties": {
                "username": {"type": "string", "description": "Username", "required": True}
            },
            "required": ["username"]
        }
    },
    "get_order_detail": {
        "function": OrderAPITools.get_order_detail,
        "description": "Get details of a specific order",
        "parameters": {
            "type": "object",
            "properties": {
                "order_id": {"type": "integer", "description": "Order ID", "required": True}
            },
            "required": ["order_id"]
        }
    },
    "get_customer": {
        "function": CustomerAPITools.get_customer,
        "description": "Get customer profile information",
        "parameters": {
            "type": "object",
            "properties": {
                "username": {"type": "string", "description": "Username", "required": True}
            },
            "required": ["username"]
        }
    },
    "get_user_info": {
        "function": CustomerAPITools.get_user_info,
        "description": "Get current user's profile information from JWT token. Use this when user asks about their own profile/info.",
        "parameters": {
            "type": "object",
            "properties": {
                "token": {"type": "string", "description": "JWT Bearer token (required)", "required": True}
            },
            "required": ["token"]
        }
    }
}
