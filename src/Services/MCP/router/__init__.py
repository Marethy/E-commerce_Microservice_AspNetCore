import logging
from dataclasses import dataclass
from typing import Optional

from embeddings import EmbeddingService, cosine_similarity

logger = logging.getLogger(__name__)


@dataclass
class ToolDescription:
    name: str
    description: str
    keywords: list[str]
    category: str  # ui, api, navigation
    action_type: str  # click, fill, scroll, navigate, select, etc.
    embedding: Optional[list[float]] = None
    
    def get_searchable_text(self) -> str:
        """Get combined text for embedding"""
        return f"{self.name} {self.description} {' '.join(self.keywords)}"


@dataclass
class RouteResult:
    tool_name: str
    action_type: str
    category: str
    confidence: float
    matched_description: str
    
    def to_dict(self) -> dict:
        return {
            "tool_name": self.tool_name,
            "action_type": self.action_type,
            "category": self.category,
            "confidence": self.confidence,
            "matched_description": self.matched_description
        }


class SemanticRouter:
    # Static tool descriptions for routing
    STATIC_TOOLS: list[ToolDescription] = [
        # UI Interaction Tools
        ToolDescription(
            name="click",
            description="Click on a button, link, or interactive element. Perform click action on UI.",
            keywords=["click", "press", "tap", "button", "submit", "confirm", "cancel", "open", "close", "expand", "collapse", "bấm", "nhấn", "mở"],
            category="ui",
            action_type="click"
        ),
        ToolDescription(
            name="fill_form",
            description="Fill in a form field, input text, enter data into a textbox or textarea.",
            keywords=["fill", "type", "enter", "input", "write", "form", "text", "field", "nhập", "điền", "viết", "gõ"],
            category="ui",
            action_type="fill"
        ),
        ToolDescription(
            name="select",
            description="Select an option from a dropdown, choose from a list, pick a value.",
            keywords=["select", "choose", "pick", "option", "dropdown", "list", "combo", "chọn", "lựa chọn"],
            category="ui",
            action_type="select"
        ),
        ToolDescription(
            name="scroll",
            description="Scroll the page up, down, to top or bottom. Navigate within the page.",
            keywords=["scroll", "up", "down", "top", "bottom", "cuộn", "kéo"],
            category="ui",
            action_type="scroll"
        ),
        ToolDescription(
            name="navigate",
            description="Navigate to a URL, go to a page, visit a website, open a link.",
            keywords=["navigate", "go", "visit", "url", "page", "website", "link", "open page", "mở trang", "đi đến", "truy cập"],
            category="navigation",
            action_type="navigate"
        ),
        ToolDescription(
            name="screenshot",
            description="Take a screenshot of the current page or a specific element.",
            keywords=["screenshot", "capture", "photo", "image", "snapshot", "chụp", "hình ảnh"],
            category="ui",
            action_type="screenshot"
        ),
        ToolDescription(
            name="wait",
            description="Wait for an element to appear, become visible, or page to load.",
            keywords=["wait", "loading", "appear", "visible", "ready", "đợi", "chờ", "tải"],
            category="ui",
            action_type="wait"
        ),
        ToolDescription(
            name="hover",
            description="Hover over an element to show tooltip or trigger hover effects.",
            keywords=["hover", "mouse over", "tooltip", "di chuột", "rê"],
            category="ui",
            action_type="hover"
        ),
        ToolDescription(
            name="check",
            description="Check or uncheck a checkbox, toggle a switch.",
            keywords=["check", "checkbox", "toggle", "switch", "tick", "untick", "đánh dấu", "bỏ chọn"],
            category="ui",
            action_type="check"
        ),
        ToolDescription(
            name="upload",
            description="Upload a file, attach a document, select a file from computer.",
            keywords=["upload", "file", "attach", "document", "tải lên", "đính kèm", "tệp"],
            category="ui",
            action_type="upload"
        ),
        ToolDescription(
            name="type_text",
            description="Type text slowly character by character, simulating real typing.",
            keywords=["type", "typing", "slow", "character", "gõ từng ký tự"],
            category="ui",
            action_type="type"
        ),
        ToolDescription(
            name="press_key",
            description="Press a keyboard key like Enter, Escape, Tab, arrow keys.",
            keywords=["press", "key", "enter", "escape", "tab", "arrow", "keyboard", "phím"],
            category="ui",
            action_type="press"
        ),
        
        # API/Data Tools
        ToolDescription(
            name="search",
            description="Search for products, items, content. Look up information.",
            keywords=["search", "find", "look", "query", "tìm kiếm", "tìm", "tra cứu"],
            category="api",
            action_type="api_call"
        ),
        ToolDescription(
            name="get_data",
            description="Get data, fetch information, retrieve details, view content.",
            keywords=["get", "fetch", "retrieve", "view", "data", "info", "details", "lấy", "xem", "thông tin"],
            category="api",
            action_type="api_call"
        ),
        ToolDescription(
            name="add_to_cart_ui",
            description="Click the 'Add to Cart' button on product page. Visually add product to shopping cart through UI interaction.",
            keywords=["add to cart", "click add", "button cart", "add button", "thêm vào giỏ", "nhấn thêm", "click mua", "bấm giỏ hàng"],
            category="ui",
            action_type="click"
        ),
        ToolDescription(
            name="add_to_cart_api",
            description="Add item to shopping cart via API (no UI). Use when UI interaction is not possible or fails.",
            keywords=["api cart", "backend cart", "direct add cart", "cart api"],
            category="api",
            action_type="api_call"
        ),
        ToolDescription(
            name="checkout",
            description="Checkout, place order, complete purchase, pay.",
            keywords=["checkout", "order", "pay", "purchase", "complete", "thanh toán", "đặt hàng"],
            category="api",
            action_type="api_call"
        ),
    ]
    
    def __init__(self, embedding_service: EmbeddingService):
        self.embedding_service = embedding_service
        self._tool_embeddings: dict[str, list[float]] = {}
        self._initialized = False
    
    async def initialize(self):
        if self._initialized:
            return
        
        logger.info("Initializing SemanticRouter embeddings...")
        
        texts = [tool.get_searchable_text() for tool in self.STATIC_TOOLS]
        embeddings = await self.embedding_service.get_embeddings_batch(texts)
        
        for tool, emb in zip(self.STATIC_TOOLS, embeddings):
            tool.embedding = emb
            self._tool_embeddings[tool.name] = emb
        
        self._initialized = True
        logger.info(f"SemanticRouter initialized with {len(self.STATIC_TOOLS)} tools")
    
    async def route_intent(
        self,
        user_intent: str,
        top_k: int = 3,
        min_confidence: float = 0.3
    ) -> list[RouteResult]:
        if not self._initialized:
            await self.initialize()
        
        if not user_intent.strip():
            return []
        
        # Get embedding for user intent
        intent_embedding = await self.embedding_service.get_embedding(user_intent)
        
        # Calculate similarities with all tools
        results: list[RouteResult] = []
        
        for tool in self.STATIC_TOOLS:
            if tool.embedding is None:
                continue
            
            similarity = cosine_similarity(intent_embedding, tool.embedding)
            
            if similarity >= min_confidence:
                results.append(RouteResult(
                    tool_name=tool.name,
                    action_type=tool.action_type,
                    category=tool.category,
                    confidence=similarity,
                    matched_description=tool.description
                ))
        
        # Sort by confidence and return top_k
        results.sort(key=lambda r: r.confidence, reverse=True)
        
        logger.debug(f"Route results for '{user_intent[:50]}': {[(r.tool_name, r.confidence) for r in results[:top_k]]}")
        
        return results[:top_k]
    
    async def classify_action(self, user_intent: str) -> Optional[RouteResult]:
        results = await self.route_intent(user_intent, top_k=1, min_confidence=0.35)
        return results[0] if results else None
    
    def get_action_type(self, tool_name: str) -> str:
        """Get the action type for a tool name"""
        for tool in self.STATIC_TOOLS:
            if tool.name == tool_name:
                return tool.action_type
        return "click"  # Default
    
    def get_category(self, tool_name: str) -> str:
        """Get the category for a tool name"""
        for tool in self.STATIC_TOOLS:
            if tool.name == tool_name:
                return tool.category
        return "ui"  # Default


class IntentExtractor:
    @staticmethod
    def extract_from_response(response: str) -> str:
        import re
        
        # Try to extract from tags
        patterns = [
            r'<tool_assistant>(.*?)</tool_assistant>',
            r'<action>(.*?)</action>',
            r'<intent>(.*?)</intent>',
            r'\[ACTION\](.*?)\[/ACTION\]',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, response, re.IGNORECASE | re.DOTALL)
            if match:
                return match.group(1).strip()
        
        # If no tags found, look for action keywords
        action_indicators = [
            r'I (?:want|need|would like) to (.+?)(?:\.|$)',
            r'(?:Please|Can you|Could you) (.+?)(?:\.|$)',
            r'(?:Click|Press|Fill|Enter|Select|Navigate|Go to) (.+?)(?:\.|$)',
        ]
        
        for pattern in action_indicators:
            match = re.search(pattern, response, re.IGNORECASE)
            if match:
                return match.group(0).strip()
        
        # Return cleaned response as fallback
        return response.strip()[:200]
    
    @staticmethod
    def is_action_intent(text: str) -> bool:
        action_words = [
            'click', 'press', 'tap', 'fill', 'type', 'enter', 'select', 'choose',
            'navigate', 'go', 'open', 'scroll', 'search', 'find', 'add', 'remove',
            'submit', 'cancel', 'confirm', 'check', 'uncheck', 'upload', 'download',
            'bấm', 'nhấn', 'điền', 'chọn', 'mở', 'cuộn', 'tìm', 'thêm', 'xóa'
        ]
        
        text_lower = text.lower()
        return any(word in text_lower for word in action_words)
