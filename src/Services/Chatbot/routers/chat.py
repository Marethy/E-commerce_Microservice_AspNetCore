# routers/chat.py
from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
from models.chat import ChatRequest, ChatResponse
from lg.graph import create_graph
from langchain_core.messages import HumanMessage, AIMessage
from utils.jwt_helper import extract_username  # ADD THIS
import json
import asyncio

router = APIRouter()

# Global graph instance
graph = create_graph()

# Session storage (in production, use Redis or database)
sessions = {}

@router.post("/chat")
async def chat_endpoint(request: ChatRequest):
    """Main chat endpoint with streaming response"""
    
    try:
        # Get or create session state
        session_id = request.session_id
        if session_id not in sessions:
            sessions[session_id] = {
                "messages": [],
                "filters": {},
                "selected_product": None,
                "cart_state": None,
                "last_intent": None,
                "user_token": request.user_token,
                "username": None,  # ADD THIS
                "suggested_url": None,
                "should_finish": False
            }
        
        state = sessions[session_id]

        state.pop("presentation_response", None)
        state.pop("final_response", None)
        state.pop("tool_result", None)
        state.pop("pending_action", None)
        
        # Add user message
        state["messages"].append(HumanMessage(content=request.message))
        
        # Update token if provided and extract username
        if request.user_token:
            state["user_token"] = request.user_token
            # Extract username from JWT token
            username = extract_username(request.user_token)
            if username:
                state["username"] = username
                print(f"🔐 Extracted username from token: {username}")
            else:
                print("⚠️ Failed to extract username from token")
        
        async def generate_response():
            """Generate streaming response"""
            try:
                # Run graph
                result = await graph.ainvoke(state)
                
                # Update session
                sessions[session_id] = result
                
                # Get final response
                final_response = result.get("final_response")
                if not final_response:
                    print("No final_response found, using fallback")
                    final_response = "Tôi có thể giúp gì cho bạn?"

                print(f"Final response to send: {final_response}")
                suggested_url = result.get("suggested_url")
                
                # Add AI message to history
                result["messages"].append(AIMessage(content=final_response))

                # Create response
                response_data = {
                    "reply": final_response,
                    "suggested_url": suggested_url
                }
                
                # Create response
                response_json = json.dumps(response_data, ensure_ascii=False)
                yield f"data: {json.dumps({'chunk': response_json})}\n\n"
                yield f"data: {json.dumps({'done': True})}\n\n"
                
            except Exception as e:
                import traceback
                traceback.print_exc()
                print(f"Error in generate_response: {e}")
                
                error_response = {
                    "reply": "Xin lỗi, có lỗi xảy ra. Vui lòng thử lại.",
                    "suggested_url": None
                }
                error_json = json.dumps(error_response, ensure_ascii=False)
                yield f"data: {json.dumps({'chunk': error_json})}\n\n"
                yield f"data: {json.dumps({'done': True})}\n\n"
        
        return StreamingResponse(
            generate_response(),
            media_type="text/plain",
            headers={
                "Cache-Control": "no-cache",
                "Connection": "keep-alive",
                "X-Accel-Buffering": "no"
            }
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.delete("/chat/{session_id}")
async def clear_session(session_id: str):
    """Clear chat session"""
    if session_id in sessions:
        del sessions[session_id]
    return {"message": "Session cleared"}