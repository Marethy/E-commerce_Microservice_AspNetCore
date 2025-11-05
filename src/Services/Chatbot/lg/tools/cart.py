import httpx
from typing import Dict, Any
from config import config


class CartTools:
    base_url = config.BASKET_API_URL

    @staticmethod
    async def get_cart(token: str, username: str) -> Dict[str, Any]:
        """Get user cart - requires username"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.get(
                    f"{CartTools.base_url}/api/baskets/{username}",
                    headers=headers
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to get cart: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def add_to_cart(
        token: str,
        username: str,
        product_id: str,
        quantity: int = 1
    ) -> Dict[str, Any]:
        """Add product to cart by updating the cart"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                cart_response = await client.get(
                    f"{CartTools.base_url}/api/baskets/{username}",
                    headers=headers
                )

                if cart_response.status_code == 404:
                    cart_data = {
                        "username": username,
                        "items": [{
                            "itemNo": product_id,
                            "quantity": quantity
                        }]
                    }
                else:
                    cart_response.raise_for_status()
                    cart_data = cart_response.json()

                existing_item = next(
                    (item for item in cart_data.get("items", [])
                     if item["itemNo"] == product_id),
                    None
                )
                if existing_item:
                    existing_item["quantity"] += quantity
                else:
                    if "items" not in cart_data:
                        cart_data["items"] = []
                    cart_data["items"].append({
                        "itemNo": product_id,
                        "quantity": quantity
                    })

                response = await client.post(
                    f"{CartTools.base_url}/api/baskets",
                    headers=headers,
                    json=cart_data
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Added to cart",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to add to cart: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def remove_from_cart(
        token: str,
        username: str,
        product_id: str
    ) -> Dict[str, Any]:
        """Remove product from cart"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                cart_response = await client.get(
                    f"{CartTools.base_url}/api/baskets/{username}",
                    headers=headers
                )
                cart_response.raise_for_status()
                cart_data = cart_response.json()

                cart_data["items"] = [
                    item for item in cart_data.get("items", [])
                    if item["itemNo"] != product_id
                ]

                response = await client.post(
                    f"{CartTools.base_url}/api/baskets",
                    headers=headers,
                    json=cart_data
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Removed from cart",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to remove from cart: {e.response.status_code}"
                }
            except Exception as e:
                return {"status": "error", "message": str(e)}

    @staticmethod
    async def clear_cart(token: str, username: str) -> Dict[str, Any]:
        """Clear cart - delete entire basket"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.delete(
                    f"{CartTools.base_url}/api/baskets/{username}",
                    headers=headers
                )
                response.raise_for_status()
                return {"status": "success", "message": "Cart cleared"}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to clear cart: {e.response.status_code}"
                }
            except Exception as e:
                return {"status": "error", "message": str(e)}

    @staticmethod
    async def prepare_checkout(token: str, username: str) -> Dict[str, Any]:
        """Validate cart before checkout"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.get(
                    f"{CartTools.base_url}/api/baskets/{username}/validate",
                    headers=headers
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Cart validation failed: {e.response.status_code}"
                }
            except Exception as e:
                return {"status": "error", "message": str(e)}

    @staticmethod
    async def get_cart_count(token: str, username: str) -> Dict[str, Any]:
        """Get cart item count"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.get(
                    f"{CartTools.base_url}/api/baskets/{username}/count",
                    headers=headers
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except Exception as e:
                return {"status": "error", "message": str(e), "data": {"count": 0}}
