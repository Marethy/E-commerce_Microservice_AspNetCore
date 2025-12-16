import asyncio
import logging
from dataclasses import dataclass, field
from typing import Optional
from playwright.async_api import async_playwright, Browser, BrowserContext, Page, Playwright

logger = logging.getLogger(__name__)


@dataclass
class BrowserConfig:
    """Configuration for browser instances"""
    headless: bool = True
    viewport_width: int = 1920
    viewport_height: int = 1080
    user_agent: str = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
    timeout: int = 30000  # 30 seconds
    slow_mo: int = 0  # Milliseconds between actions
    locale: str = "en-US"
    timezone_id: str = "America/New_York"


@dataclass
class BrowserSession:
    """Represents an active browser session for a user"""
    session_id: str
    page: Page
    context: BrowserContext
    created_at: float = field(default_factory=lambda: asyncio.get_event_loop().time())
    last_activity: float = field(default_factory=lambda: asyncio.get_event_loop().time())
    
    def update_activity(self):
        """Update last activity timestamp"""
        self.last_activity = asyncio.get_event_loop().time()
    
    async def close(self):
        """Close the browser session"""
        try:
            await self.page.close()
            await self.context.close()
            logger.info(f"Browser session {self.session_id} closed")
        except Exception as e:
            logger.error(f"Error closing session {self.session_id}: {e}")


class BrowserManager:
    """
    Manages browser instances and sessions using Playwright.
    Implements singleton pattern for browser lifecycle management.
    """
    
    _instance: Optional['BrowserManager'] = None
    _lock = asyncio.Lock()
    
    def __init__(self):
        self._playwright: Optional[Playwright] = None
        self._browser: Optional[Browser] = None
        self._sessions: dict[str, BrowserSession] = {}
        self._config = BrowserConfig()
        self._initialized = False
    
    @classmethod
    async def get_instance(cls) -> 'BrowserManager':
        """Get or create singleton instance"""
        async with cls._lock:
            if cls._instance is None:
                cls._instance = BrowserManager()
                await cls._instance.initialize()
            return cls._instance
    
    async def initialize(self):
        """Initialize Playwright and browser"""
        if self._initialized:
            return
        
        try:
            self._playwright = await async_playwright().start()
            self._browser = await self._playwright.chromium.launch(
                headless=self._config.headless,
                slow_mo=self._config.slow_mo,
                args=[
                    '--disable-blink-features=AutomationControlled',
                    '--disable-infobars',
                    '--no-sandbox',
                    '--disable-setuid-sandbox',
                    '--disable-dev-shm-usage',
                ]
            )
            self._initialized = True
            logger.info("BrowserManager initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize BrowserManager: {e}")
            raise
    
    async def create_session(self, session_id: str, start_url: Optional[str] = None) -> BrowserSession:
        """Create a new browser session for a user"""
        if session_id in self._sessions:
            logger.info(f"Returning existing session for {session_id}")
            return self._sessions[session_id]
        
        if not self._browser:
            await self.initialize()
        
        # Create a new browser context with custom settings
        context = await self._browser.new_context(
            viewport={
                'width': self._config.viewport_width,
                'height': self._config.viewport_height
            },
            user_agent=self._config.user_agent,
            locale=self._config.locale,
            timezone_id=self._config.timezone_id,
            ignore_https_errors=True,
        )
        
        # Set default timeout
        context.set_default_timeout(self._config.timeout)
        
        # Create a new page
        page = await context.new_page()
        
        # Navigate to start URL if provided
        if start_url:
            await page.goto(start_url, wait_until='networkidle')
        
        # Create session
        session = BrowserSession(
            session_id=session_id,
            page=page,
            context=context
        )
        
        self._sessions[session_id] = session
        logger.info(f"Created browser session {session_id}")
        
        return session
    
    def get_session(self, session_id: str) -> Optional[BrowserSession]:
        """Get an existing browser session"""
        session = self._sessions.get(session_id)
        if session:
            session.update_activity()
        return session
    
    async def close_session(self, session_id: str):
        """Close and remove a browser session"""
        session = self._sessions.pop(session_id, None)
        if session:
            await session.close()
    
    async def close_all_sessions(self):
        """Close all browser sessions"""
        for session_id in list(self._sessions.keys()):
            await self.close_session(session_id)
    
    async def cleanup_inactive_sessions(self, max_idle_seconds: int = 1800):
        """Clean up sessions that have been idle for too long"""
        current_time = asyncio.get_event_loop().time()
        inactive_sessions = [
            sid for sid, session in self._sessions.items()
            if current_time - session.last_activity > max_idle_seconds
        ]
        
        for session_id in inactive_sessions:
            logger.info(f"Cleaning up inactive session: {session_id}")
            await self.close_session(session_id)
        
        return len(inactive_sessions)
    
    async def navigate(self, session_id: str, url: str, wait_until: str = 'networkidle') -> bool:
        """Navigate to a URL in the specified session"""
        session = self.get_session(session_id)
        if not session:
            logger.error(f"Session {session_id} not found")
            return False
        
        try:
            await session.page.goto(url, wait_until=wait_until)
            session.update_activity()
            logger.info(f"Session {session_id} navigated to {url}")
            return True
        except Exception as e:
            logger.error(f"Navigation error for {session_id}: {e}")
            return False
    
    async def get_current_url(self, session_id: str) -> Optional[str]:
        """Get current URL for a session"""
        session = self.get_session(session_id)
        return session.page.url if session else None
    
    async def get_page_title(self, session_id: str) -> Optional[str]:
        """Get page title for a session"""
        session = self.get_session(session_id)
        return await session.page.title() if session else None
    
    async def take_screenshot(self, session_id: str, full_page: bool = False) -> Optional[bytes]:
        """Take a screenshot of the current page"""
        session = self.get_session(session_id)
        if not session:
            return None
        
        try:
            return await session.page.screenshot(full_page=full_page)
        except Exception as e:
            logger.error(f"Screenshot error for {session_id}: {e}")
            return None
    
    async def shutdown(self):
        """Shutdown the browser manager"""
        await self.close_all_sessions()
        
        if self._browser:
            await self._browser.close()
            self._browser = None
        
        if self._playwright:
            await self._playwright.stop()
            self._playwright = None
        
        self._initialized = False
        logger.info("BrowserManager shutdown complete")
    
    @property
    def active_sessions_count(self) -> int:
        """Get count of active sessions"""
        return len(self._sessions)
    
    @property
    def is_initialized(self) -> bool:
        """Check if browser manager is initialized"""
        return self._initialized
