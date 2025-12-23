import httpx
import logging
from typing import Dict, Any, Optional
from tenacity import retry, stop_after_attempt, wait_exponential, retry_if_exception_type

logger = logging.getLogger(__name__)

class BaseAPIClient:
    def __init__(self, base_url: str, default_timeout: float = 10.0):
        self.base_url = base_url
        self.default_timeout = default_timeout
    
    def _build_headers(self, token: Optional[str] = None) -> Dict[str, str]:
        headers = {}
        if token:
            headers["Authorization"] = f"Bearer {token}"
        return headers
    
    @retry(
        stop=stop_after_attempt(3),
        wait=wait_exponential(multiplier=1, min=1, max=5),
        retry=retry_if_exception_type((httpx.TimeoutException, httpx.NetworkError)),
        reraise=True
    )
    async def _request(
        self,
        method: str,
        endpoint: str,
        token: Optional[str] = None,
        params: Optional[Dict[str, Any]] = None,
        json_data: Optional[Dict[str, Any]] = None,
        timeout: Optional[float] = None
    ) -> Dict[str, Any]:
        try:
            headers = self._build_headers(token)
            timeout_value = timeout or self.default_timeout
            
            async with httpx.AsyncClient(timeout=timeout_value) as client:
                response = await client.request(
                    method=method,
                    url=f"{self.base_url}{endpoint}",
                    headers=headers,
                    params=params,
                    json=json_data
                )
                response.raise_for_status()
                return response.json()
        except httpx.HTTPStatusError as e:
            logger.error(f"HTTP {e.response.status_code} error for {endpoint}: {e}")
            logger.error(f"Response body: {e.response.text}")
            return {"success": False, "message": f"HTTP {e.response.status_code}: {str(e)}"}
        except httpx.TimeoutException as e:
            logger.error(f"Timeout error for {endpoint}: {e}")
            return {"success": False, "message": "Request timeout"}
        except Exception as e:
            logger.error(f"Error calling {endpoint}: {e}")
            return {"success": False, "message": str(e)}
    
    async def get(self, endpoint: str, token: Optional[str] = None, params: Optional[Dict[str, Any]] = None, timeout: Optional[float] = None) -> Dict[str, Any]:
        return await self._request("GET", endpoint, token, params=params, timeout=timeout)
    
    async def post(self, endpoint: str, token: Optional[str] = None, json_data: Optional[Dict[str, Any]] = None, timeout: Optional[float] = None) -> Dict[str, Any]:
        return await self._request("POST", endpoint, token, json_data=json_data, timeout=timeout)
