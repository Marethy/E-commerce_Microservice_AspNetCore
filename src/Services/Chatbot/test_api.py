import asyncio
import httpx
import json

async def test_api():
    """Test API responses directly"""
    
    base_url = "http://localhost:8082"
    
    try:
        async with httpx.AsyncClient(timeout=10.0) as client:
            # Test products search
            print("=== Testing Products Search ===")
            params = {"q": "laptop", "maxPrice": 20000000}
            response = await client.get(f"{base_url}/api/v1/products", params=params)
            
            print(f"Status: {response.status_code}")
            print(f"Headers: {dict(response.headers)}")
            print(f"Raw text: {response.text[:500]}")
            
            if response.status_code == 200:
                try:
                    data = response.json()
                    print(f"JSON data type: {type(data)}")
                    print(f"JSON keys: {list(data.keys()) if isinstance(data, dict) else 'Not a dict'}")
                    print(f"JSON data: {json.dumps(data, indent=2, ensure_ascii=False)[:500]}")
                except Exception as e:
                    print(f"JSON parse error: {e}")
            
            # Test brands
            print("\n=== Testing Brands ===")
            response = await client.get(f"{base_url}/api/v1/brands")
            print(f"Brands status: {response.status_code}")
            if response.status_code == 200:
                brands_data = response.json()
                print(f"Brands data: {json.dumps(brands_data, indent=2, ensure_ascii=False)[:300]}")
                
    except Exception as e:
        print(f"Test error: {e}")

if __name__ == "__main__":
    asyncio.run(test_api())