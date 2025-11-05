import httpx
from typing import Dict, Any
from config import config


class OrderTools:
    base_url = config.ORDER_API_URL

    @staticmethod
    async def get_user_orders(
        token: str,
        limit: int = 20,
        offset: int = 0
    ) -> Dict[str, Any]:
        """Get user orders"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            params = {"limit": limit, "offset": offset}
            try:
                response = await client.get(
                    f"{OrderTools.base_url}/api/v1/orders/user",
                    headers=headers,
                    params=params
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to get orders: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def get_order_detail(token: str, order_id: str) -> Dict[str, Any]:
        """Get order detail"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.get(
                    f"{OrderTools.base_url}/api/v1/orders/{order_id}",
                    headers=headers
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to get order: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def create_order_from_cart(
        token: str,
        order_data: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Create order from cart"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.post(
                    f"{OrderTools.base_url}/api/v1/orders/from-cart",
                    headers=headers,
                    json=order_data
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Order created successfully",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to create order: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def cancel_order(token: str, order_id: str) -> Dict[str, Any]:
        """Cancel order"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.post(
                    f"{OrderTools.base_url}/api/v1/orders/{order_id}/cancel",
                    headers=headers
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Order cancelled successfully",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to cancel order: {e.response.status_code}"
                }
            except Exception as e:
                return {"status": "error", "message": str(e)}