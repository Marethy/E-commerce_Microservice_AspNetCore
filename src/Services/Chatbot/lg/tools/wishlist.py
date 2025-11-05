import httpx
from typing import Dict, Any
from config import config


class WishlistTools:
    # Wishlist is typically part of Product or User service
    # Using PRODUCT_API_URL as default
    base_url = config.PRODUCT_API_URL

    @staticmethod
    async def get_wishlist(
        token: str,
        page: int = 1,
        limit: int = 20
    ) -> Dict[str, Any]:
        """Get user wishlist"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            params = {"page": page, "limit": limit}
            try:
                response = await client.get(
                    f"{WishlistTools.base_url}/api/wishlist",
                    headers=headers,
                    params=params
                )
                response.raise_for_status()
                return {"status": "success", "data": response.json()}
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to get wishlist: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def add_to_wishlist(token: str, product_id: str) -> Dict[str, Any]:
        """Add product to wishlist"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            data = {"productId": product_id}
            try:
                response = await client.post(
                    f"{WishlistTools.base_url}/api/wishlist",
                    headers=headers,
                    json=data
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Added to wishlist",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to add to wishlist: {e.response.status_code}",
                    "data": None
                }
            except Exception as e:
                return {"status": "error", "message": str(e), "data": None}

    @staticmethod
    async def remove_from_wishlist(token: str, product_id: str) -> Dict[str, Any]:
        """Remove product from wishlist"""
        async with httpx.AsyncClient(timeout=30.0) as client:
            headers = {"Authorization": f"Bearer {token}"}
            try:
                response = await client.delete(
                    f"{WishlistTools.base_url}/api/wishlist/{product_id}",
                    headers=headers
                )
                response.raise_for_status()
                return {
                    "status": "success",
                    "message": "Removed from wishlist",
                    "data": response.json()
                }
            except httpx.HTTPStatusError as e:
                return {
                    "status": "error",
                    "message": f"Failed to remove from wishlist: {e.response.status_code}"
                }
            except Exception as e:
                return {"status": "error", "message": str(e)}