import logging
from typing import Any, Optional
from api_tools import ECOMMERCE_TOOLS
from embeddings import EmbeddingService, cosine_similarity

logger = logging.getLogger(__name__)


class APIToolsServer:
    def __init__(self, embedding_service: Optional[EmbeddingService] = None):
        self._initialized = False
        self.embedding_service = embedding_service or EmbeddingService()
        self._tool_embeddings = {}
    
    async def initialize(self):
        if self._initialized:
            return
        
        logger.info("Initializing APIToolsServer...")
        
        # Create embeddings for all tools (for semantic matching)
        tool_descriptions = []
        tool_names = []
        
        for tool_name, tool_spec in ECOMMERCE_TOOLS.items():
            tool_descriptions.append(tool_spec["description"])
            tool_names.append(tool_name)
        
        if tool_descriptions:
            embeddings = await self.embedding_service.get_embeddings_batch(tool_descriptions)
            self._tool_embeddings = dict(zip(tool_names, embeddings))
            logger.info(f"Created embeddings for {len(tool_names)} API tools")
        
        self._initialized = True
        logger.info("APIToolsServer initialized successfully")
    
    async def discover_tools(
        self,
        user_id: str,
        query: str = "",
        auth_token: str = None
    ) -> dict:
        if not self._initialized:
            await self.initialize()
        
        if not query or not query.strip():
            tools = []
            for tool_name, tool_spec in ECOMMERCE_TOOLS.items():
                tool_def = {
                    "name": tool_name,
                    "description": tool_spec["description"],
                    "parameters": tool_spec["parameters"]
                }
                tools.append(tool_def)
            
            logger.info(f"Discovered all {len(tools)} API tools for user {user_id} (no query)")
            
            return {
                "tools": tools,
                "context": {
                    "type": "api_tools",
                    "gateway": "http://localhost:5000"
                },
                "tool_count": len(tools)
            }
        
        query_embedding = await self.embedding_service.get_embedding(query)
        
        tool_scores = []
        for tool_name, tool_embedding in self._tool_embeddings.items():
            similarity = cosine_similarity(query_embedding, tool_embedding)
            tool_scores.append((tool_name, similarity))
        
        # Sort by similarity descending
        tool_scores.sort(key=lambda x: x[1], reverse=True)
        
        # Get top relevant tools (threshold 0.5 or top 5)
        threshold = 0.5
        top_k = 5
        
        relevant_tools = []
        for tool_name, score in tool_scores[:top_k]:
            if score >= threshold or len(relevant_tools) < 3:  # Always return at least 3
                tool_spec = ECOMMERCE_TOOLS[tool_name]
                tool_def = {
                    "name": tool_name,
                    "description": tool_spec["description"],
                    "parameters": tool_spec["parameters"],
                    "_confidence": float(score)
                }
                relevant_tools.append(tool_def)
        
        logger.info(f"Discovered {len(relevant_tools)} relevant API tools for query: '{query}' (user {user_id})")
        
        return {
            "tools": relevant_tools,
            "context": {
                "type": "api_tools",
                "gateway": "http://localhost:5000",
                "query": query
            },
            "tool_count": len(relevant_tools)
        }
    
    async def execute_tool(
        self,
        user_id: str,
        tool_name: str,
        arguments: dict[str, Any] = {},
        auth_token: str = None
    ) -> dict:
        if not self._initialized:
            await self.initialize()
        
        # Inject token into arguments if provided and not already present
        if auth_token and 'token' not in arguments:
            arguments['token'] = auth_token
        
        # Find tool
        tool_spec = ECOMMERCE_TOOLS.get(tool_name)
        if not tool_spec:
            return {
                "success": False,
                "error": f"Tool '{tool_name}' not found"
            }
        
        # Execute tool function
        try:
            tool_function = tool_spec["function"]
            result = await tool_function(**arguments)
            
            return {
                "success": True,
                "result": result,
                "tool": tool_name
            }
        except Exception as e:
            logger.error(f"Error executing tool {tool_name}: {e}")
            return {
                "success": False,
                "error": str(e),
                "tool": tool_name
            }
    
    @property
    def is_initialized(self) -> bool:
        return self._initialized


# Singleton instance
_api_tools_server: Optional[APIToolsServer] = None


async def get_api_tools_server() -> APIToolsServer:
    global _api_tools_server
    
    if _api_tools_server is None:
        _api_tools_server = APIToolsServer()
        await _api_tools_server.initialize()
    
    return _api_tools_server
