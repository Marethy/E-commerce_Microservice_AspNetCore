import logging
from typing import Optional

from browser.scanner import UIElement

logger = logging.getLogger(__name__)


class DynamicToolBuilder:
    @staticmethod
    def build_click_tool(elements: list[UIElement], context: dict = {}) -> dict:
        # Filter only clickable elements
        clickable = [e for e in elements if e.type in ('button', 'link', 'interactive', 'menuitem', 'tab')]
        
        # Build description with available targets
        element_list = DynamicToolBuilder._format_element_list(clickable, max_items=20)
        
        description = f"""Click on an interactive element on the current page.

**Available clickable elements:**
{element_list}

**Usage:** Provide the element ID (e.g., "el_btn_submit") to click on it."""

        return {
            "name": "browser_click",
            "description": description,
            "_action": "click",
            "_dynamic": True,
            "_element_count": len(clickable),
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ID of the element to click (from the available elements list)",
                        "enum": [e.id for e in clickable[:30]]
                    }
                },
                "required": ["element_id"]
            }
        }
    
    @staticmethod
    def build_fill_tool(elements: list[UIElement], context: dict = {}) -> dict:
        # Filter input elements
        inputs = [e for e in elements if e.type in ('input', 'textarea', 'textbox', 'searchbox')]
        
        element_list = DynamicToolBuilder._format_element_list(inputs, max_items=15)
        
        description = f"""Fill in a form field or input with text.

**Available input fields:**
{element_list}

**Usage:** Provide the element ID and the value to enter."""

        return {
            "name": "browser_fill",
            "description": description,
            "_action": "fill",
            "_dynamic": True,
            "_element_count": len(inputs),
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ID of the input field to fill",
                        "enum": [e.id for e in inputs[:20]]
                    },
                    "value": {
                        "type": "string",
                        "description": "The text value to enter into the field"
                    }
                },
                "required": ["element_id", "value"]
            }
        }
    
    @staticmethod
    def build_select_tool(elements: list[UIElement], context: dict = {}) -> dict:
        # Filter select elements
        selects = [e for e in elements if e.type in ('select', 'combobox', 'listbox')]
        
        element_list = []
        for el in selects[:10]:
            options_str = f" (Options: {', '.join(el.options[:5])})" if el.options else ""
            element_list.append(f"- [{el.id}] {el.description}{options_str}")
        
        description = f"""Select an option from a dropdown.

**Available dropdowns:**
{chr(10).join(element_list) if element_list else '- No dropdown elements found'}

**Usage:** Provide the element ID and the option value to select."""

        return {
            "name": "browser_select",
            "description": description,
            "_action": "select",
            "_dynamic": True,
            "_element_count": len(selects),
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ID of the dropdown element",
                        "enum": [e.id for e in selects[:15]]
                    },
                    "value": {
                        "type": "string",
                        "description": "The option value or label to select"
                    }
                },
                "required": ["element_id", "value"]
            }
        }
    
    @staticmethod
    def build_scroll_tool(context: dict = {}) -> dict:
        return {
            "name": "browser_scroll",
            "description": """Scroll the page in a direction.

**Directions:**
- "up": Scroll up 500px
- "down": Scroll down 500px
- "top": Scroll to top of page
- "bottom": Scroll to bottom of page""",
            "_action": "scroll",
            "_dynamic": False,
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "direction": {
                        "type": "string",
                        "enum": ["up", "down", "top", "bottom"],
                        "description": "The direction to scroll"
                    }
                },
                "required": ["direction"]
            }
        }
    
    @staticmethod
    def build_navigate_tool(context: dict = {}) -> dict:
        current_url = context.get('url', 'unknown')
        
        return {
            "name": "browser_navigate",
            "description": f"""Navigate to a URL.

**Current page:** {current_url}

**Usage:** Provide a full URL (https://example.com) or relative path (/products).""",
            "_action": "navigate",
            "_dynamic": False,
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "url": {
                        "type": "string",
                        "description": "The URL to navigate to"
                    }
                },
                "required": ["url"]
            }
        }
    
    @staticmethod
    def build_screenshot_tool(context: dict = {}) -> dict:
        return {
            "name": "browser_screenshot",
            "description": "Take a screenshot of the current page.",
            "_action": "screenshot",
            "_dynamic": False,
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "full_page": {
                        "type": "boolean",
                        "description": "Whether to capture the full page (scrollable) or just the viewport",
                        "default": False
                    }
                },
                "required": []
            }
        }
    
    @staticmethod
    def build_wait_tool(context: dict = {}) -> dict:
        return {
            "name": "browser_wait",
            "description": "Wait for a condition before proceeding.",
            "_action": "wait",
            "_dynamic": False,
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "selector": {
                        "type": "string",
                        "description": "CSS selector to wait for (optional)"
                    },
                    "timeout": {
                        "type": "integer",
                        "description": "Maximum time to wait in milliseconds",
                        "default": 5000
                    }
                },
                "required": []
            }
        }
    
    @staticmethod
    def build_type_tool(elements: list[UIElement], context: dict = {}) -> dict:
        inputs = [e for e in elements if e.type in ('input', 'textarea', 'textbox', 'searchbox')]
        
        return {
            "name": "browser_type",
            "description": """Type text character by character (simulates real typing).

Use this for forms that have real-time validation or autocomplete.""",
            "_action": "type",
            "_dynamic": True,
            "_element_count": len(inputs),
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ID of the input field",
                        "enum": [e.id for e in inputs[:20]]
                    },
                    "text": {
                        "type": "string",
                        "description": "The text to type"
                    },
                    "delay": {
                        "type": "integer",
                        "description": "Delay between keystrokes in milliseconds",
                        "default": 50
                    }
                },
                "required": ["element_id", "text"]
            }
        }
    
    @staticmethod
    def build_press_key_tool(context: dict = {}) -> dict:
        return {
            "name": "browser_press_key",
            "description": """Press a keyboard key.

**Common keys:** Enter, Tab, Escape, Backspace, Delete, ArrowUp, ArrowDown, ArrowLeft, ArrowRight""",
            "_action": "press",
            "_dynamic": False,
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "key": {
                        "type": "string",
                        "description": "The key to press",
                        "enum": ["Enter", "Tab", "Escape", "Backspace", "Delete", 
                                "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight",
                                "Home", "End", "PageUp", "PageDown", "Space"]
                    }
                },
                "required": ["key"]
            }
        }
    
    @staticmethod
    def build_checkbox_tool(elements: list[UIElement], context: dict = {}) -> dict:
        checkboxes = [e for e in elements if e.type in ('checkbox', 'radio', 'switch')]
        
        element_list = DynamicToolBuilder._format_element_list(checkboxes, max_items=10)
        
        description = f"""Check or uncheck a checkbox/toggle.

**Available checkboxes:**
{element_list}"""

        return {
            "name": "browser_check",
            "description": description,
            "_action": "check",
            "_dynamic": True,
            "_element_count": len(checkboxes),
            "_context": context,
            "inputSchema": {
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ID of the checkbox",
                        "enum": [e.id for e in checkboxes[:15]]
                    },
                    "checked": {
                        "type": "boolean",
                        "description": "Whether to check (true) or uncheck (false)",
                        "default": True
                    }
                },
                "required": ["element_id"]
            }
        }
    
    @staticmethod
    def _format_element_list(elements: list[UIElement], max_items: int = 20) -> str:
        if not elements:
            return "- No matching elements found on this page"
        
        lines = []
        for el in elements[:max_items]:
            type_badge = f"({el.type.capitalize()})"
            context_str = f" [{el.context}]" if el.context else ""
            lines.append(f"- [{el.id}] {el.description} {type_badge}{context_str}")
        
        if len(elements) > max_items:
            lines.append(f"- ... and {len(elements) - max_items} more elements")
        
        return "\n".join(lines)
    
    @staticmethod
    def build_all_tools(
        elements: list[UIElement],
        context: dict = {},
        include_static: bool = True
    ) -> list[dict]:
        tools = []
        
        # Dynamic tools (require elements)
        tools.append(DynamicToolBuilder.build_click_tool(elements, context))
        tools.append(DynamicToolBuilder.build_fill_tool(elements, context))
        tools.append(DynamicToolBuilder.build_select_tool(elements, context))
        tools.append(DynamicToolBuilder.build_type_tool(elements, context))
        tools.append(DynamicToolBuilder.build_checkbox_tool(elements, context))
        
        # Static tools
        if include_static:
            tools.append(DynamicToolBuilder.build_scroll_tool(context))
            tools.append(DynamicToolBuilder.build_navigate_tool(context))
            tools.append(DynamicToolBuilder.build_screenshot_tool(context))
            tools.append(DynamicToolBuilder.build_wait_tool(context))
            tools.append(DynamicToolBuilder.build_press_key_tool(context))
        
        return tools
    
    @staticmethod
    def build_tools_for_action(
        action_type: str,
        elements: list[UIElement],
        context: dict = {}
    ) -> Optional[dict]:
        builders = {
            'click': lambda: DynamicToolBuilder.build_click_tool(elements, context),
            'fill': lambda: DynamicToolBuilder.build_fill_tool(elements, context),
            'select': lambda: DynamicToolBuilder.build_select_tool(elements, context),
            'scroll': lambda: DynamicToolBuilder.build_scroll_tool(context),
            'navigate': lambda: DynamicToolBuilder.build_navigate_tool(context),
            'screenshot': lambda: DynamicToolBuilder.build_screenshot_tool(context),
            'wait': lambda: DynamicToolBuilder.build_wait_tool(context),
            'type': lambda: DynamicToolBuilder.build_type_tool(elements, context),
            'press': lambda: DynamicToolBuilder.build_press_key_tool(context),
            'check': lambda: DynamicToolBuilder.build_checkbox_tool(elements, context),
        }
        
        builder = builders.get(action_type)
        return builder() if builder else None


class ElementResolver:
    def __init__(self, elements: list[UIElement]):
        self._elements_map: dict[str, UIElement] = {e.id: e for e in elements}
    
    def resolve(self, element_id: str) -> Optional[UIElement]:
        return self._elements_map.get(element_id)
    
    def get_selector(self, element_id: str) -> Optional[str]:
        element = self.resolve(element_id)
        return element.selector if element else None
    
    def find_by_text(self, text: str) -> Optional[UIElement]:
        text_lower = text.lower()
        for el in self._elements_map.values():
            if text_lower in el.text.lower() or text_lower in el.description.lower():
                return el
        return None
    
    def find_by_type(self, elem_type: str) -> list[UIElement]:
        return [el for el in self._elements_map.values() if el.type == elem_type]
