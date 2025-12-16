import json
import logging
import httpx
import os
from openai import AsyncOpenAI
from config import config
import mcp_client

logger = logging.getLogger(__name__)

# API URLs
CUSTOMER_API_URL = os.getenv("CUSTOMER_API_URL", "http://localhost:5005")

MAX_ITERATIONS = 15

SYSTEM_PROMPT = """Bạn là AI Browser Agent tương tác với website E-commerce.

QUY TRÌNH:
1. Gọi `get_elements` để xem các element trên trang (buttons, links, inputs...)
2. Dựa vào danh sách element, quyết định action: click link/button hoặc fill input
3. Sau mỗi action, gọi lại `get_elements` để xem trang mới
4. Lặp lại đến khi hoàn thành task

LƯU Ý:
- Muốn đi đến trang khác? Click vào link tương ứng trên header/menu
- Muốn tìm kiếm? Fill vào search input rồi click nút search
- LUÔN gọi get_elements trước mọi action để biết element_id"""

# Browser automation tools + API tools
TOOLS = [
    # ===== BROWSER TOOLS =====
    {
        "type": "function",
        "function": {
            "name": "get_elements",
            "description": "Lấy danh sách các element tương tác được trên trang (buttons, links, inputs, etc.) với element_id. LUÔN gọi trước khi click/fill.",
            "parameters": {"type": "object", "properties": {}}
        }
    },
    {
        "type": "function",
        "function": {
            "name": "click",
            "description": "Click vào element trên trang web",
            "parameters": {
                "type": "object",
                "properties": {
                    "element_id": {"type": "string", "description": "ID của element từ get_elements"}
                },
                "required": ["element_id"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "fill",
            "description": "Điền text vào input/textarea trên trang web",
            "parameters": {
                "type": "object",
                "properties": {
                    "element_id": {"type": "string", "description": "ID của input từ get_elements"},
                    "value": {"type": "string", "description": "Giá trị cần điền"}
                },
                "required": ["element_id", "value"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "scroll",
            "description": "Cuộn trang để xem thêm element",
            "parameters": {
                "type": "object",
                "properties": {
                    "direction": {"type": "string", "enum": ["up", "down"]}
                },
                "required": ["direction"]
            }
        }
    },
    # ===== E-COMMERCE API TOOLS =====
    {
        "type": "function",
        "function": {
            "name": "search_products",
            "description": "Tìm kiếm sản phẩm qua API với filters (query, category, price range). Dùng để tìm sản phẩm nhanh thay vì browser.",
            "parameters": {
                "type": "object",
                "properties": {
                    "query": {"type": "string", "description": "Từ khóa tìm kiếm (vd: laptop, smartphone)"},
                    "category_id": {"type": "string", "description": "GUID của category"},
                    "min_price": {"type": "number", "description": "Giá tối thiểu"},
                    "max_price": {"type": "number", "description": "Giá tối đa"}
                }
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "get_cart",
            "description": "Lấy giỏ hàng của user qua API",
            "parameters": {
                "type": "object",
                "properties": {
                    "username": {"type": "string", "description": "Username của user"}
                },
                "required": ["username"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "update_cart",
            "description": "Thêm hoặc cập nhật sản phẩm trong giỏ hàng qua API",
            "parameters": {
                "type": "object",
                "properties": {
                    "username": {"type": "string", "description": "Username"},
                    "items": {
                        "type": "array",
                        "description": "Danh sách items với itemNo, itemName, quantity, itemPrice"
                    }
                },
                "required": ["username", "items"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "checkout_cart",
            "description": "Thanh toán giỏ hàng và tạo order qua API",
            "parameters": {
                "type": "object",
                "properties": {
                    "username": {"type": "string"},
                    "first_name": {"type": "string"},
                    "last_name": {"type": "string"},
                    "email": {"type": "string"},
                    "shipping_address": {"type": "string"},
                    "invoice_address": {"type": "string"}
                },
                "required": ["username", "first_name", "last_name", "email", "shipping_address"]
            }
        }
    },
    # ===== USER INFO TOOL =====
    {
        "type": "function",
        "function": {
            "name": "get_user_info",
            "description": "Lấy thông tin profile của user hiện tại (từ token). Dùng khi user hỏi về thông tin cá nhân như tên, email, địa chỉ.",
            "parameters": {
                "type": "object",
                "properties": {}
            }
        }
    }
]


async def execute_tool(user_id: str, tool_name: str, args: dict, auth_token: str = None) -> dict:
    """Execute tool and return result"""
    
    # ===== BROWSER TOOLS =====
    if tool_name == "get_elements":
        result = await mcp_client.get_page_elements(user_id, auth_token)
        if result.get("success"):
            elements = result.get("elements", [])
            # Format elements for LLM
            formatted = []
            for el in elements[:60]:  # Limit to 60 elements
                formatted.append(f"[{el['id']}] {el['type']}: {el['description']}")
            return {
                "success": True,
                "current_url": result.get("current_url", ""),
                "page_title": result.get("page_title", ""),
                "elements": formatted
            }
        return result
    
    elif tool_name == "click":
        element_id = args.get("element_id", "")
        # Strip brackets if LLM included them from formatted output
        element_id = element_id.strip("[]")
        return await mcp_client.execute_tool(
            user_id, "browser_click", {"element_id": element_id}, auth_token
        )
    
    elif tool_name == "fill":
        element_id = args.get("element_id", "")
        # Strip brackets if LLM included them
        element_id = element_id.strip("[]")
        value = args.get("value", "")
        return await mcp_client.execute_tool(
            user_id, "browser_fill", {"element_id": element_id, "value": value}, auth_token
        )
    
    elif tool_name == "scroll":
        direction = args.get("direction", "down")
        return await mcp_client.execute_tool(
            user_id, "browser_scroll", {"direction": direction}, auth_token
        )
    
    # ===== E-COMMERCE API TOOLS =====
    elif tool_name in ["search_products", "get_cart", "update_cart", "checkout_cart"]:
        # Call through MCP API tools
        return await mcp_client.execute_tool(user_id, tool_name, args, auth_token)
    
    # ===== USER INFO TOOL (Direct API call) =====
    elif tool_name == "get_user_info":
        if not auth_token:
            return {"success": False, "error": "Bạn cần đăng nhập để xem thông tin cá nhân"}
        try:
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.post(
                    f"{CUSTOMER_API_URL}/api/Customers/user-info",
                    headers={"Authorization": f"Bearer {auth_token}"}
                )
                data = response.json()
                if data.get("isSuccess"):
                    return {"success": True, "user_info": data.get("result")}
                return {"success": False, "error": data.get("message", "Không tìm thấy thông tin user")}
        except Exception as e:
            logger.error(f"Error getting user info: {e}")
            return {"success": False, "error": str(e)}
    
    return {"success": False, "error": f"Unknown tool: {tool_name}"}


async def stream_chat(user_id: str, message: str, history: list[dict], auth_token: str = None):
    """DOM-based browser agent"""
    client = AsyncOpenAI(api_key=config.XAI_API_KEY, base_url="https://api.x.ai/v1")
    
    messages = [{"role": "system", "content": SYSTEM_PROMPT}]
    messages.extend(history)
    messages.append({"role": "user", "content": message})
    
    iteration = 0
    
    while iteration < MAX_ITERATIONS:
        iteration += 1
        
        try:
            stream = await client.chat.completions.create(
                model="grok-3-mini",
                messages=messages,
                tools=TOOLS,
                tool_choice="auto",
                stream=True
            )
            
            full_content = ""
            tool_calls_data = {}
            
            async for chunk in stream:
                if not chunk.choices:
                    continue
                
                delta = chunk.choices[0].delta
                
                if hasattr(delta, "reasoning_content") and delta.reasoning_content:
                    yield {"type": "thinking", "content": delta.reasoning_content}
                
                if delta.content:
                    full_content += delta.content
                    yield {"type": "content", "content": delta.content}
                
                if delta.tool_calls:
                    for tc in delta.tool_calls:
                        idx = tc.index
                        if idx not in tool_calls_data:
                            tool_calls_data[idx] = {"id": "", "name": "", "arguments": ""}
                        if tc.id:
                            tool_calls_data[idx]["id"] = tc.id
                        if tc.function:
                            if tc.function.name:
                                tool_calls_data[idx]["name"] = tc.function.name
                            if tc.function.arguments:
                                tool_calls_data[idx]["arguments"] += tc.function.arguments
            
            if not tool_calls_data:
                break
            
            # Process tool calls
            assistant_msg = {"role": "assistant", "content": full_content or None}
            tool_calls_list = []
            for idx in sorted(tool_calls_data.keys()):
                tc = tool_calls_data[idx]
                tool_calls_list.append({
                    "id": tc["id"],
                    "type": "function",
                    "function": {"name": tc["name"], "arguments": tc["arguments"]}
                })
            assistant_msg["tool_calls"] = tool_calls_list
            messages.append(assistant_msg)
            
            for tc in tool_calls_list:
                tool_name = tc["function"]["name"]
                try:
                    args = json.loads(tc["function"]["arguments"])
                except:
                    args = {}
                
                yield {"type": "executing", "tool": tool_name, "args": args}
                
                result = await execute_tool(user_id, tool_name, args, auth_token)
                
                yield {"type": "executed", "tool": tool_name, "success": result.get("success", False)}
                
                messages.append({
                    "role": "tool",
                    "tool_call_id": tc["id"],
                    "content": json.dumps(result, ensure_ascii=False)
                })
            
        except Exception as e:
            logger.error(f"Error in stream_chat: {e}")
            yield {"type": "error", "content": str(e)}
            break
    
    if iteration >= MAX_ITERATIONS:
        yield {"type": "content", "content": "\n[Đã dừng do vượt quá giới hạn]"}
