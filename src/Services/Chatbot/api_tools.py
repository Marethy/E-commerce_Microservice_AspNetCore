import logging
from typing import Dict, Any, Optional, List
from utils.base_api_client import BaseAPIClient
from config import config

logger = logging.getLogger(__name__)


class ProductAPITools:
    _client = BaseAPIClient(config.PRODUCT_API_URL)
    
    @classmethod
    async def search_products(
        cls,
        query: Optional[str] = None,
        category_id: Optional[str] = None,
        min_price: Optional[float] = None,
        max_price: Optional[float] = None
    ) -> Dict[str, Any]:
        params = {}
        if query:
            params["q"] = query
        if category_id:
            params["categoryId"] = category_id
        if min_price:
            params["minPrice"] = min_price
        if max_price:
            params["maxPrice"] = max_price
        return await cls._client.get("/api/Products/search", params=params)
    
    @classmethod
    async def get_product_detail(cls, product_id: str) -> Dict[str, Any]:
        return await cls._client.get(f"/api/Products/{product_id}")
    
    @classmethod
    async def get_categories(cls) -> Dict[str, Any]:
        return await cls._client.get("/api/Categories")
    
    @classmethod
    async def get_brands(cls) -> Dict[str, Any]:
        return await cls._client.get("/api/Brands")


class CartAPITools:
    _client = BaseAPIClient(config.BASKET_API_URL)
    
    @classmethod
    async def get_cart(cls, username: str) -> Dict[str, Any]:
        return await cls._client.get(f"/api/Baskets/{username}")
    
    @classmethod
    async def update_cart(cls, username: str, items: List[Dict[str, Any]], email: Optional[str] = None) -> Dict[str, Any]:
        payload = {"username": username, "items": items}
        if email:
            payload["emailAddress"] = email
        logger.info(f"CartAPITools.update_cart payload: {payload}")
        result = await cls._client.post("/api/Baskets", json_data=payload)
        logger.info(f"CartAPITools.update_cart result: {result}")
        return result
    
    @classmethod
    async def checkout_cart(
        cls,
        username: str,
        first_name: str,
        last_name: str,
        email: str,
        shipping_address: str,
        invoice_address: Optional[str] = None
    ) -> Dict[str, Any]:
        payload = {
            "username": username,
            "firstName": first_name,
            "lastName": last_name,
            "emailAddress": email,
            "shippingAddress": shipping_address,
            "invoiceAddress": invoice_address or shipping_address
        }
        return await cls._client.post("/api/Baskets/checkout", json_data=payload, timeout=30.0)


class OrderAPITools:
    _client = BaseAPIClient(config.ORDER_API_URL)
    
    @classmethod
    async def get_user_orders(cls, username: str) -> Dict[str, Any]:
        return await cls._client.get(f"/api/v1/orders/users/{username}")
    
    @classmethod
    async def get_order_detail(cls, order_id: int) -> Dict[str, Any]:
        return await cls._client.get(f"/api/v1/orders/{order_id}")


class CustomerAPITools:
    _client = BaseAPIClient(config.CUSTOMER_API_URL)
    
    @classmethod
    async def get_customer(cls, username: str) -> Dict[str, Any]:
        return await cls._client.get(f"/api/Customers/{username}")
    
    @classmethod
    async def get_user_info(cls, user_id: str) -> Dict[str, Any]:
        if not user_id:
            return {"success": False, "message": "User ID is required"}
        return await cls._client.post("/api/Customers/user-info", json_data={"userId": user_id})
