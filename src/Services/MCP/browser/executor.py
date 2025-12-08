import asyncio
import logging
from typing import Optional
from dataclasses import dataclass
from playwright.async_api import Page, TimeoutError as PlaywrightTimeout

logger = logging.getLogger(__name__)


@dataclass
class ActionResult:
    success: bool
    action: str
    selector: str
    message: str = ""
    error: Optional[str] = None
    screenshot: Optional[bytes] = None
    new_url: Optional[str] = None
    
    def to_dict(self) -> dict:
        return {
            "success": self.success,
            "action": self.action,
            "selector": self.selector,
            "message": self.message,
            "error": self.error,
            "new_url": self.new_url,
            "has_screenshot": self.screenshot is not None
        }


class ActionExecutor:
    def __init__(self, page: Page):
        self.page = page
        self._default_timeout = 10000  # 10 seconds
    
    async def execute(
        self,
        action: str,
        selector: str,
        value: Optional[str] = None,
        options: dict = {}
    ) -> ActionResult:
        action_handlers = {
            'click': self._click,
            'fill': self._fill,
            'type': self._type_text,
            'select': self._select,
            'scroll': self._scroll,
            'navigate': self._navigate,
            'hover': self._hover,
            'focus': self._focus,
            'clear': self._clear,
            'check': self._check,
            'uncheck': self._uncheck,
            'upload': self._upload,
            'wait': self._wait_for_element,
            'screenshot': self._take_screenshot,
            'press': self._press_key,
        }
        
        handler = action_handlers.get(action)
        if not handler:
            return ActionResult(
                success=False,
                action=action,
                selector=selector,
                error=f"Unknown action: {action}"
            )
        
        try:
            return await handler(selector, value, options)
        except PlaywrightTimeout as e:
            return ActionResult(
                success=False,
                action=action,
                selector=selector,
                error=f"Timeout waiting for element: {selector}"
            )
        except Exception as e:
            logger.error(f"Action execution error: {e}")
            return ActionResult(
                success=False,
                action=action,
                selector=selector,
                error=str(e)
            )
    
    async def _click(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        
        # Wait for element to be visible and clickable
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        
        # Scroll into view if needed
        await locator.scroll_into_view_if_needed()
        
        # Click with options
        click_options = {
            'force': options.get('force', False),
            'click_count': options.get('click_count', 1),
            'delay': options.get('delay', 0),
        }
        
        if options.get('button'):
            click_options['button'] = options['button']
        
        await locator.click(**click_options)
        
        # Wait a bit for any navigation or state change
        await asyncio.sleep(0.3)
        
        return ActionResult(
            success=True,
            action='click',
            selector=selector,
            message=f"Clicked on element",
            new_url=self.page.url
        )
    
    async def _fill(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        if value is None:
            return ActionResult(
                success=False,
                action='fill',
                selector=selector,
                error="Value is required for fill action"
            )
        
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        
        # Clear existing value first if specified
        if options.get('clear_first', True):
            await locator.clear()
        
        # Fill the input
        await locator.fill(value)
        
        return ActionResult(
            success=True,
            action='fill',
            selector=selector,
            message=f"Filled input with value: {value[:50]}..."
        )
    
    async def _type_text(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        if value is None:
            return ActionResult(
                success=False,
                action='type',
                selector=selector,
                error="Value is required for type action"
            )
        
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        
        # Focus on element
        await locator.focus()
        
        # Type with delay between characters
        delay = options.get('delay', 50)
        await locator.type(value, delay=delay)
        
        return ActionResult(
            success=True,
            action='type',
            selector=selector,
            message=f"Typed text: {value[:50]}..."
        )
    
    async def _select(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        if value is None:
            return ActionResult(
                success=False,
                action='select',
                selector=selector,
                error="Value is required for select action"
            )
        
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        
        # Try selecting by value, then by label
        try:
            await locator.select_option(value=value)
        except:
            await locator.select_option(label=value)
        
        return ActionResult(
            success=True,
            action='select',
            selector=selector,
            message=f"Selected option: {value}"
        )
    
    async def _scroll(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        direction = value or options.get('direction', 'down')
        amount = options.get('amount', 500)
        
        if selector and selector != 'body':
            # Scroll within an element
            locator = self.page.locator(selector)
            if direction == 'down':
                await locator.evaluate(f"el => el.scrollBy(0, {amount})")
            elif direction == 'up':
                await locator.evaluate(f"el => el.scrollBy(0, -{amount})")
            elif direction == 'right':
                await locator.evaluate(f"el => el.scrollBy({amount}, 0)")
            elif direction == 'left':
                await locator.evaluate(f"el => el.scrollBy(-{amount}, 0)")
        else:
            # Scroll the page
            if direction == 'down':
                await self.page.evaluate(f"window.scrollBy(0, {amount})")
            elif direction == 'up':
                await self.page.evaluate(f"window.scrollBy(0, -{amount})")
            elif direction == 'bottom':
                await self.page.evaluate("window.scrollTo(0, document.body.scrollHeight)")
            elif direction == 'top':
                await self.page.evaluate("window.scrollTo(0, 0)")
        
        return ActionResult(
            success=True,
            action='scroll',
            selector=selector,
            message=f"Scrolled {direction}"
        )
    
    async def _navigate(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        url = value or selector
        if not url:
            return ActionResult(
                success=False,
                action='navigate',
                selector='',
                error="URL is required for navigate action"
            )
        
        # Ensure URL has protocol
        if not url.startswith(('http://', 'https://')):
            url = f"https://{url}"
        
        wait_until = options.get('wait_until', 'networkidle')
        await self.page.goto(url, wait_until=wait_until)
        
        return ActionResult(
            success=True,
            action='navigate',
            selector=url,
            message=f"Navigated to {url}",
            new_url=self.page.url
        )
    
    async def _hover(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        await locator.hover()
        
        return ActionResult(
            success=True,
            action='hover',
            selector=selector,
            message="Hovered over element"
        )
    
    async def _focus(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        await locator.focus()
        
        return ActionResult(
            success=True,
            action='focus',
            selector=selector,
            message="Focused on element"
        )
    
    async def _clear(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        await locator.clear()
        
        return ActionResult(
            success=True,
            action='clear',
            selector=selector,
            message="Cleared input field"
        )
    
    async def _check(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        await locator.check()
        
        return ActionResult(
            success=True,
            action='check',
            selector=selector,
            message="Checked checkbox"
        )
    
    async def _uncheck(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        locator = self.page.locator(selector)
        await locator.wait_for(state='visible', timeout=options.get('timeout', self._default_timeout))
        await locator.uncheck()
        
        return ActionResult(
            success=True,
            action='uncheck',
            selector=selector,
            message="Unchecked checkbox"
        )
    
    async def _upload(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        if not value:
            return ActionResult(
                success=False,
                action='upload',
                selector=selector,
                error="File path is required for upload action"
            )
        
        locator = self.page.locator(selector)
        await locator.set_input_files(value)
        
        return ActionResult(
            success=True,
            action='upload',
            selector=selector,
            message=f"Uploaded file: {value}"
        )
    
    async def _wait_for_element(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        state = value or options.get('state', 'visible')
        timeout = options.get('timeout', self._default_timeout)
        
        locator = self.page.locator(selector)
        await locator.wait_for(state=state, timeout=timeout)
        
        return ActionResult(
            success=True,
            action='wait',
            selector=selector,
            message=f"Element is now {state}"
        )
    
    async def _take_screenshot(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        full_page = options.get('full_page', False)
        
        if selector and selector != 'body':
            # Screenshot of specific element
            locator = self.page.locator(selector)
            screenshot = await locator.screenshot()
        else:
            # Full page screenshot
            screenshot = await self.page.screenshot(full_page=full_page)
        
        return ActionResult(
            success=True,
            action='screenshot',
            selector=selector,
            message="Screenshot taken",
            screenshot=screenshot
        )
    
    async def _press_key(
        self,
        selector: str,
        value: Optional[str],
        options: dict
    ) -> ActionResult:
        key = value or options.get('key', 'Enter')
        
        if selector and selector != 'body':
            locator = self.page.locator(selector)
            await locator.press(key)
        else:
            await self.page.keyboard.press(key)
        
        return ActionResult(
            success=True,
            action='press',
            selector=selector,
            message=f"Pressed key: {key}"
        )
    
    async def execute_chain(self, actions: list[dict]) -> list[ActionResult]:
        results = []
        
        for action_def in actions:
            result = await self.execute(
                action=action_def.get('action', 'click'),
                selector=action_def.get('selector', ''),
                value=action_def.get('value'),
                options=action_def.get('options', {})
            )
            results.append(result)
            
            # Stop chain if action failed and stop_on_error is True
            if not result.success and action_def.get('stop_on_error', True):
                break
            
            # Optional delay between actions
            delay = action_def.get('delay_after', 0)
            if delay > 0:
                await asyncio.sleep(delay / 1000)
        
        return results
