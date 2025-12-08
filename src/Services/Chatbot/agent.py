import json
import re
import logging
from openai import AsyncOpenAI
from config import config
import mcp_client

logger = logging.getLogger(__name__)

MAX_ITERATIONS = 25

SYSTEM_PROMPT = """Bạn là AI assistant chuyên nghiệp cho hệ thống E-commerce.

QUY TRÌNH XỬ LÝ (BẮT BUỘC):

1. **Giai đoạn suy nghĩ (Reasoning):** Phân tích yêu cầu user.
2. **Giai đoạn hành động (Action):**
   - Nếu bạn chưa có công cụ (tools) cần thiết, hãy YÊU CẦU TÌM TOOL bằng format:
     [[SEARCH: mô tả ngắn gọn chức năng cần tìm]]
   
   - Nếu hệ thống đã cung cấp tool, hãy SỬ DỤNG TOOL bằng JSON format:
     ```json
     {
       "name": "tên_tool",
       "arguments": { ... }
     }
     ```

3. **Giai đoạn trả lời:** Nếu đã có đủ thông tin, hãy trả lời user bằng ngôn ngữ tự nhiên.

LƯU Ý QUAN TRỌNG:
- KHÔNG bao giờ bịa ra kết quả tool.
- Chỉ output JSON khi bạn thực sự muốn gọi tool.
"""

def extract_json_snippet(text):
    """Extract JSON from text, handling nested objects"""
    try:
        # Try ```json block first
        match = re.search(r'```json\s*(\{.+?\})\s*```', text, re.DOTALL)
        if match:
            return json.loads(match.group(1))
        
        # Try to find JSON object by counting braces
        start_idx = text.find('{')
        if start_idx == -1:
            return None
            
        brace_count = 0
        for i in range(start_idx, len(text)):
            if text[i] == '{':
                brace_count += 1
            elif text[i] == '}':
                brace_count -= 1
                if brace_count == 0:
                    # Found matching closing brace
                    json_str = text[start_idx:i+1]
                    try:
                        return json.loads(json_str)
                    except:
                        pass
                    break
    except Exception as e:
        logger.debug(f"Failed to extract JSON: {e}")
    return None

async def stream_chat(user_id: str, message: str, history: list[dict], auth_token: str = None):
    client = AsyncOpenAI(
        api_key=config.XAI_API_KEY,
        base_url="https://api.x.ai/v1"
    )
    
    messages = [{"role": "system", "content": SYSTEM_PROMPT}]
    messages.extend(history)
    messages.append({"role": "user", "content": message})
    
    iteration = 0
    
    while iteration < MAX_ITERATIONS:
        iteration += 1
        full_content_buffer = ""
        reasoning_buffer = ""
        emit_buffer = ""  # Buffer để delay emission
        
        tool_search_query = None
        is_searching = False
        
        try:
            stream = await client.chat.completions.create(
                model="grok-3-mini",
                messages=messages,
                stream=True
            )
            
            async for chunk in stream:
                if not chunk.choices:
                    continue
                
                # LOG: Kiểm tra cấu trúc chunk
                logger.debug(f"[CHUNK] Full chunk: {chunk}")
                    
                delta = chunk.choices[0].delta
                
                # LOG: Kiểm tra delta attributes
                logger.debug(f"[DELTA] Delta attributes: {dir(delta)}")
                logger.debug(f"[DELTA] Has reasoning_content: {hasattr(delta, 'reasoning_content')}")
                logger.debug(f"[DELTA] Has content: {delta.content is not None}")
                
                # 1. Xử lý Reasoning (Chỉ để hiển thị cho UI)
                if hasattr(delta, "reasoning_content") and delta.reasoning_content:
                    content = delta.reasoning_content
                    reasoning_buffer += content
                    logger.info(f"[REASONING] Got reasoning chunk: {content[:100]}...")
                    yield {"type": "thinking", "content": content}
                
                # 2. Xử lý Content (Nơi chứa lệnh thực sự)
                if delta.content:
                    content = delta.content
                    full_content_buffer += content
                    
                    logger.debug(f"[CONTENT] Got content chunk: {content[:100]}")
                    
                    # Kiểm tra pattern SEARCH TOOL: [[SEARCH: ...]]
                    if not is_searching and '[[SEARCH' in full_content_buffer:
                        search_match = re.search(r'\[\[SEARCH:(.*?)\]\]', full_content_buffer)
                        if search_match:
                            is_searching = True
                            tool_search_query = search_match.group(1).strip()
                            emit_buffer = ""  # Clear buffer khi detect search
                            yield {"type": "searching", "content": tool_search_query}
                    
                    has_search = '[[SEARCH' in full_content_buffer
                    has_json = False
                    
                    # Check for ```json block
                    if '```json' in full_content_buffer:
                        has_json = True
                        # Emit text before ```json
                        json_start = full_content_buffer.find('```json')
                        text_before = full_content_buffer[:json_start].strip()
                        if text_before and text_before != emit_buffer.strip():
                            remaining_text = text_before[len(emit_buffer):].strip() if emit_buffer else text_before
                            if remaining_text:
                                yield {"type": "content", "content": remaining_text}
                        emit_buffer = ""  # Clear buffer
                    # Check if current chunk or buffer contains { - potential JSON
                    elif '{' in content or '{' in full_content_buffer:
                        # Stop emitting immediately when { detected
                        has_json = True
                        # Emit only the part BEFORE { in emit_buffer
                        if emit_buffer and '{' not in emit_buffer:
                            yield {"type": "content", "content": emit_buffer}
                        elif emit_buffer and '{' in emit_buffer:
                            # Split at { and only emit before part
                            clean_part = emit_buffer.split('{')[0].strip()
                            if clean_part:
                                yield {"type": "content", "content": clean_part}
                        emit_buffer = ""  # Clear buffer completely
                    
                    # Only add to emit_buffer if no JSON detected AND no search
                    if not has_json and not has_search and not is_searching:
                        emit_buffer += content
                        
                        # Emit buffer when it's long enough or has newline
                        if len(emit_buffer) > 50 or '\n' in emit_buffer:
                            yield {"type": "content", "content": emit_buffer}
                            emit_buffer = ""
            
            # --- KẾT THÚC STREAM CỦA 1 TURN ---
            
            logger.info(f"[STREAM_END] Full content buffer length: {len(full_content_buffer)}")
            logger.info(f"[STREAM_END] Reasoning buffer length: {len(reasoning_buffer)}")
            logger.info(f"[STREAM_END] Full content preview: {full_content_buffer[:200]}...")
            
            # KHÔNG emit buffer nữa - để logic bên dưới xử lý
            
            # CASE A: Agent yêu cầu tìm kiếm Tool
            if tool_search_query:
                logger.info(f"Agent requested tool search: {tool_search_query}")
                
                result = await mcp_client.get_relevant_tools(user_id, tool_search_query, auth_token)
                
                if result.get("success") and result.get("tools"):
                    tools = result["tools"]
                    tools_desc = json.dumps(tools, ensure_ascii=False, indent=2)
                    
                    messages.append({"role": "assistant", "content": full_content_buffer})
                    messages.append({
                        "role": "user",
                        "content": f"SYSTEM: Tìm thấy các tools sau. Hãy chọn tool phù hợp và output JSON để thực thi:\n{tools_desc}"
                    })
                    
                    # Emit full tool info objects
                    yield {"type": "searched", "tools": [
                        {"name": t["name"], "description": t.get("description", "")}
                        for t in tools
                    ]}
                    continue
                else:
                    messages.append({"role": "assistant", "content": full_content_buffer})
                    messages.append({
                        "role": "user",
                        "content": "SYSTEM: Không tìm thấy tool nào phù hợp. Hãy trả lời user dựa trên hiểu biết của bạn."
                    })
                    continue
            
            # CASE B: Agent output JSON để gọi Tool
            potential_json = extract_json_snippet(full_content_buffer)
            
            if potential_json and "name" in potential_json:
                tool_name = potential_json.get("name")
                tool_args = potential_json.get("arguments", {})
                
                # Find JSON position to extract text before it
                json_start = None
                
                # Try ```json block first
                json_match = re.search(r'```json\s*(\{.*?\})\s*```', full_content_buffer, re.DOTALL)
                if json_match:
                    json_start = json_match.start()
                else:
                    # Find raw JSON by counting braces
                    start_idx = full_content_buffer.find('{')
                    if start_idx != -1:
                        brace_count = 0
                        for i in range(start_idx, len(full_content_buffer)):
                            if full_content_buffer[i] == '{':
                                brace_count += 1
                            elif full_content_buffer[i] == '}':
                                brace_count -= 1
                                if brace_count == 0:
                                    json_start = start_idx
                                    json_end = i + 1
                                    break
                
                # Emit text before JSON (if any)
                if json_start is not None:
                    text_before_json = full_content_buffer[:json_start].strip()
                    if text_before_json:
                        yield {"type": "content", "content": text_before_json}
                
                yield {"type": "executing", "tool": tool_name, "args": tool_args}
                
                exec_result = await mcp_client.execute_tool(user_id, tool_name, tool_args, auth_token)
                
                success = exec_result.get("success", False)
                result_data = exec_result.get("result", {})
                error_str = exec_result.get("error", "")
                
                if success:
                    result_str = json.dumps(result_data, ensure_ascii=False)
                    yield {"type": "executed", "tool": tool_name, "success": True}
                else:
                    result_str = error_str
                    yield {"type": "executed", "tool": tool_name, "success": False, "error": error_str}
                
                messages.append({"role": "assistant", "content": full_content_buffer})
                messages.append({
                    "role": "user",
                    "content": f"TOOL_OUTPUT (Success: {success}):\n{result_str}"
                })
                continue
            
            # CASE C: Phản hồi thông thường (Final Answer)
            # Emit remaining buffer nếu có
            if emit_buffer:
                yield {"type": "content", "content": emit_buffer}
            break
            
        except Exception as e:
            logger.error(f"Error in stream_chat: {e}")
            yield {"type": "error", "content": str(e)}
            break
    
    if iteration >= MAX_ITERATIONS:
        yield {"type": "content", "content": "\n[Hệ thống: Đã dừng do vượt quá số bước xử lý cho phép]"}
