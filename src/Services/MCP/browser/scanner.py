import logging
import re
from dataclasses import dataclass, field
from typing import Optional
from playwright.async_api import Page, ElementHandle

logger = logging.getLogger(__name__)


@dataclass
class UIElement:
    id: str
    selector: str
    type: str  # button, input, link, select, textarea, checkbox, radio, interactive
    description: str
    text: str
    aria_label: Optional[str] = None
    placeholder: Optional[str] = None
    test_id: Optional[str] = None
    name: Optional[str] = None
    value: Optional[str] = None
    is_visible: bool = True
    is_enabled: bool = True
    rect: dict = field(default_factory=dict)
    attributes: dict = field(default_factory=dict)
    options: list[str] = field(default_factory=list)  # For select elements
    context: str = ""  # Surrounding context/parent info
    
    def to_dict(self) -> dict:
        return {
            "id": self.id,
            "selector": self.selector,
            "type": self.type,
            "description": self.description,
            "text": self.text,
            "aria_label": self.aria_label,
            "placeholder": self.placeholder,
            "test_id": self.test_id,
            "name": self.name,
            "value": self.value,
            "is_visible": self.is_visible,
            "is_enabled": self.is_enabled,
            "rect": self.rect,
            "attributes": self.attributes,
            "options": self.options,
            "context": self.context
        }


class SmartLocator:
    @staticmethod
    async def generate_selector(element: ElementHandle, page: Page) -> str:
        try:
            # Priority 1: data-testid
            test_id = await element.get_attribute("data-testid")
            if test_id:
                return f'[data-testid="{test_id}"]'
            
            # Priority 2: aria-label
            aria_label = await element.get_attribute("aria-label")
            if aria_label:
                return f'[aria-label="{aria_label}"]'
            
            # Priority 3: id (if unique and stable-looking)
            elem_id = await element.get_attribute("id")
            if elem_id and not re.match(r'^(react-|ember-|:r\d|[a-f0-9-]{36})', elem_id):
                return f'#{elem_id}'
            
            # Priority 4: name attribute
            name = await element.get_attribute("name")
            if name:
                tag = await element.evaluate("el => el.tagName.toLowerCase()")
                return f'{tag}[name="{name}"]'
            
            # Priority 5: Role + text content
            role = await element.get_attribute("role")
            text = await element.inner_text()
            if role and text and len(text) < 50:
                clean_text = text.strip()[:30]
                return f'[role="{role}"]:has-text("{clean_text}")'
            
            # Priority 6: Tag + visible text
            tag = await element.evaluate("el => el.tagName.toLowerCase()")
            if text and len(text) < 50:
                clean_text = text.strip()[:30]
                return f'{tag}:has-text("{clean_text}")'
            
            # Fallback: Generate CSS path
            selector = await element.evaluate("""el => {
                const path = [];
                while (el && el.nodeType === Node.ELEMENT_NODE) {
                    let selector = el.tagName.toLowerCase();
                    if (el.id && !el.id.match(/^(react-|ember-|:r\\d)/)) {
                        selector = '#' + el.id;
                        path.unshift(selector);
                        break;
                    }
                    const parent = el.parentNode;
                    if (parent) {
                        const siblings = Array.from(parent.children).filter(
                            c => c.tagName === el.tagName
                        );
                        if (siblings.length > 1) {
                            const index = siblings.indexOf(el) + 1;
                            selector += ':nth-of-type(' + index + ')';
                        }
                    }
                    path.unshift(selector);
                    el = el.parentNode;
                }
                return path.slice(-5).join(' > ');
            }""")
            
            return selector
            
        except Exception as e:
            logger.warning(f"Error generating selector: {e}")
            return ""
    
    @staticmethod
    async def generate_unique_id(element: ElementHandle, index: int) -> str:
        try:
            test_id = await element.get_attribute("data-testid")
            if test_id:
                return f"el_{test_id}"
            
            elem_id = await element.get_attribute("id")
            if elem_id:
                return f"el_{elem_id}"
            
            name = await element.get_attribute("name")
            tag = await element.evaluate("el => el.tagName.toLowerCase()")
            
            if name:
                return f"el_{tag}_{name}"
            
            text = await element.inner_text()
            if text:
                clean_text = re.sub(r'[^a-zA-Z0-9]', '_', text[:20].strip().lower())
                return f"el_{tag}_{clean_text}_{index}"
            
            return f"el_{tag}_{index}"
            
        except Exception as e:
            return f"el_unknown_{index}"


class ElementScanner:
    # Element types to scan
    INTERACTIVE_ROLES = [
        "button", "link", "menuitem", "menuitemcheckbox", "menuitemradio",
        "tab", "switch", "checkbox", "radio", "combobox", "listbox",
        "textbox", "searchbox", "option", "slider", "spinbutton"
    ]
    
    INTERACTIVE_TAGS = [
        "button", "a", "input", "select", "textarea", 
        "details", "summary"
    ]
    
    def __init__(self, page: Page):
        self.page = page
        self._locator = SmartLocator()
    
    async def capture_snapshot(self, max_elements: int = 100) -> list[UIElement]:
        elements: list[UIElement] = []
        seen_selectors: set[str] = set()
        
        try:
            # Wait for page to be ready
            await self.page.wait_for_load_state('domcontentloaded')
            
            # Scan by roles
            role_elements = await self._scan_by_roles()
            elements.extend(role_elements)
            
            # Scan by tags
            tag_elements = await self._scan_by_tags()
            for el in tag_elements:
                if el.selector not in seen_selectors:
                    elements.append(el)
                    seen_selectors.add(el.selector)
            
            # Scan data-testid elements
            testid_elements = await self._scan_data_testid()
            for el in testid_elements:
                if el.selector not in seen_selectors:
                    elements.append(el)
                    seen_selectors.add(el.selector)
            
            # Sort by position (top to bottom, left to right)
            elements.sort(key=lambda e: (e.rect.get('y', 9999), e.rect.get('x', 9999)))
            
            # Limit results
            elements = elements[:max_elements]
            
            logger.info(f"Captured {len(elements)} interactive elements")
            return elements
            
        except Exception as e:
            logger.error(f"Error capturing DOM snapshot: {e}")
            return []
    
    async def _scan_by_roles(self) -> list[UIElement]:
        elements: list[UIElement] = []
        
        for role in self.INTERACTIVE_ROLES:
            try:
                locator = self.page.get_by_role(role)
                count = await locator.count()
                
                for i in range(min(count, 20)):  # Limit per role
                    try:
                        elem = locator.nth(i)
                        
                        # Check visibility
                        if not await elem.is_visible():
                            continue
                        
                        handle = await elem.element_handle()
                        if not handle:
                            continue
                        
                        ui_element = await self._extract_element_info(
                            handle, role, len(elements)
                        )
                        
                        if ui_element:
                            elements.append(ui_element)
                            
                    except Exception as e:
                        continue
                        
            except Exception as e:
                continue
        
        return elements
    
    async def _scan_by_tags(self) -> list[UIElement]:
        elements: list[UIElement] = []
        
        for tag in self.INTERACTIVE_TAGS:
            try:
                locator = self.page.locator(tag)
                count = await locator.count()
                
                for i in range(min(count, 30)):  # Limit per tag
                    try:
                        elem = locator.nth(i)
                        
                        # Check visibility
                        if not await elem.is_visible():
                            continue
                        
                        # Skip disabled elements
                        is_disabled = await elem.get_attribute("disabled")
                        if is_disabled is not None:
                            continue
                        
                        handle = await elem.element_handle()
                        if not handle:
                            continue
                        
                        # Determine element type
                        elem_type = await self._determine_element_type(handle, tag)
                        
                        ui_element = await self._extract_element_info(
                            handle, elem_type, len(elements)
                        )
                        
                        if ui_element:
                            elements.append(ui_element)
                            
                    except Exception as e:
                        continue
                        
            except Exception as e:
                continue
        
        return elements
    
    async def _scan_data_testid(self) -> list[UIElement]:
        elements: list[UIElement] = []
        
        try:
            locator = self.page.locator('[data-testid]')
            count = await locator.count()
            
            for i in range(min(count, 50)):
                try:
                    elem = locator.nth(i)
                    
                    if not await elem.is_visible():
                        continue
                    
                    handle = await elem.element_handle()
                    if not handle:
                        continue
                    
                    tag = await handle.evaluate("el => el.tagName.toLowerCase()")
                    elem_type = await self._determine_element_type(handle, tag)
                    
                    ui_element = await self._extract_element_info(
                        handle, elem_type, len(elements)
                    )
                    
                    if ui_element:
                        elements.append(ui_element)
                        
                except Exception:
                    continue
                    
        except Exception as e:
            logger.warning(f"Error scanning data-testid elements: {e}")
        
        return elements
    
    async def _determine_element_type(self, handle: ElementHandle, tag: str) -> str:
        try:
            if tag == 'input':
                input_type = await handle.get_attribute("type") or "text"
                if input_type in ('checkbox', 'radio'):
                    return input_type
                elif input_type in ('submit', 'button', 'reset'):
                    return 'button'
                else:
                    return 'input'
            
            elif tag == 'a':
                return 'link'
            
            elif tag == 'button':
                return 'button'
            
            elif tag == 'select':
                return 'select'
            
            elif tag == 'textarea':
                return 'textarea'
            
            else:
                # Check for interactive role
                role = await handle.get_attribute("role")
                if role in ('button', 'link', 'menuitem', 'tab'):
                    return role
                
                # Check for click handlers
                has_onclick = await handle.evaluate(
                    "el => !!el.onclick || el.hasAttribute('onclick')"
                )
                if has_onclick:
                    return 'interactive'
                
                return 'interactive'
                
        except Exception:
            return 'interactive'
    
    async def _extract_element_info(
        self, 
        handle: ElementHandle, 
        elem_type: str,
        index: int
    ) -> Optional[UIElement]:
        try:
            # Generate selector and ID
            selector = await self._locator.generate_selector(handle, self.page)
            elem_id = await self._locator.generate_unique_id(handle, index)
            
            if not selector:
                return None
            
            # Get text content
            text = ""
            try:
                text = await handle.inner_text()
                text = text.strip()[:100] if text else ""
            except:
                pass
            
            # Get attributes
            aria_label = await handle.get_attribute("aria-label")
            placeholder = await handle.get_attribute("placeholder")
            test_id = await handle.get_attribute("data-testid")
            name = await handle.get_attribute("name")
            value = await handle.get_attribute("value")
            title = await handle.get_attribute("title")
            
            # Get bounding rect
            rect = {}
            try:
                box = await handle.bounding_box()
                if box:
                    rect = {
                        'x': int(box['x']),
                        'y': int(box['y']),
                        'width': int(box['width']),
                        'height': int(box['height'])
                    }
            except:
                pass
            
            # Check enabled state
            is_enabled = True
            try:
                is_enabled = await handle.is_enabled()
            except:
                pass
            
            # Get options for select elements
            options = []
            if elem_type == 'select':
                try:
                    options = await handle.evaluate("""el => 
                        Array.from(el.options).map(o => o.text).slice(0, 20)
                    """)
                except:
                    pass
            
            # Build description
            description = self._build_description(
                elem_type, text, aria_label, placeholder, title, name
            )
            
            # Get parent context
            context = ""
            try:
                context = await handle.evaluate("""el => {
                    const parent = el.closest('form, section, nav, header, main, aside, article');
                    if (parent) {
                        const label = parent.getAttribute('aria-label') || 
                                     parent.getAttribute('data-testid') ||
                                     parent.tagName.toLowerCase();
                        return label;
                    }
                    return '';
                }""")
            except:
                pass
            
            return UIElement(
                id=elem_id,
                selector=selector,
                type=elem_type,
                description=description,
                text=text,
                aria_label=aria_label,
                placeholder=placeholder,
                test_id=test_id,
                name=name,
                value=value,
                is_visible=True,
                is_enabled=is_enabled,
                rect=rect,
                options=options,
                context=context,
                attributes={
                    "title": title or "",
                    "aria-label": aria_label or "",
                }
            )
            
        except Exception as e:
            logger.warning(f"Error extracting element info: {e}")
            return None
    
    def _build_description(
        self,
        elem_type: str,
        text: str,
        aria_label: Optional[str],
        placeholder: Optional[str],
        title: Optional[str],
        name: Optional[str]
    ) -> str:
        label = aria_label or title or text or placeholder or name or "unlabeled"
        label = label.strip()[:50]
        
        type_names = {
            'button': 'Button',
            'link': 'Link',
            'input': 'Input field',
            'textarea': 'Text area',
            'select': 'Dropdown',
            'checkbox': 'Checkbox',
            'radio': 'Radio button',
            'interactive': 'Clickable element'
        }
        
        type_name = type_names.get(elem_type, 'Element')
        
        return f"{type_name}: {label}"
    
    async def find_element_by_text(self, text: str) -> Optional[UIElement]:
        """Find an interactive element by its text content"""
        try:
            locator = self.page.get_by_text(text, exact=False)
            
            if await locator.count() > 0:
                handle = await locator.first.element_handle()
                if handle:
                    return await self._extract_element_info(handle, 'interactive', 0)
            
            return None
            
        except Exception as e:
            logger.error(f"Error finding element by text: {e}")
            return None
    
    async def find_element_by_role(self, role: str, name: Optional[str] = None) -> Optional[UIElement]:
        """Find an element by role and optional name"""
        try:
            if name:
                locator = self.page.get_by_role(role, name=name)
            else:
                locator = self.page.get_by_role(role)
            
            if await locator.count() > 0:
                handle = await locator.first.element_handle()
                if handle:
                    return await self._extract_element_info(handle, role, 0)
            
            return None
            
        except Exception as e:
            logger.error(f"Error finding element by role: {e}")
            return None
