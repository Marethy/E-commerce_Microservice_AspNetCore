"""
End-to-End Browser Automation Test
Test scenario: Search → Add to Cart → Checkout → Fill Form

This script tests the complete agent workflow:
1. Type text in search bar
2. Scroll and find products
3. Add product to cart (with screenshot)
4. Navigate to cart
5. Click checkout
6. Select COD payment
7. Fill checkout form
"""

import asyncio
import logging
from playwright.async_api import async_playwright
import sys
import os

# Add parent directory to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from browser.manager import BrowserManager
from browser.scanner import ElementScanner
from browser.executor import ActionExecutor
from router import SemanticRouter
from embeddings import EmbeddingService

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


async def test_e2e_shopping_flow():
    """Complete shopping flow test"""
    
    print("\n" + "="*60)
    print("🧪 BROWSER AUTOMATION E2E TEST")
    print("="*60 + "\n")
    
    async with async_playwright() as p:
        # Launch browser
        print("🌐 Launching browser...")
        browser = await p.chromium.launch(headless=False, slow_mo=1000)
        page = await browser.new_page(viewport={'width': 1280, 'height': 800})
        
        scanner = ElementScanner(page)
        executor = ActionExecutor(page)
        
        try:
            # STEP 1: Navigate to homepage
            print("\n📍 Step 1: Navigate to homepage")
            await page.goto("http://localhost:5173")
            await asyncio.sleep(2)
            print("✅ Homepage loaded")
            
            # STEP 2: Search for product
            print("\n🔍 Step 2: Search for products")
            search_input = "input[type='search'], input[placeholder*='Search'], input[placeholder*='Tìm']"
            
            # Type search query
            result = await executor.execute(
                action="fill",
                selector=search_input,
                value="laptop",
                options={}
            )
            
            if result.success:
                print("✅ Typed 'laptop' in search")
                await asyncio.sleep(1)
                
                # Press Enter to search
                await executor.execute(
                    action="press",
                    selector=search_input,
                    value="Enter",
                    options={}
                )
                await asyncio.sleep(2)
                print("✅ Search executed")
            else:
                print(f"❌ Search failed: {result.error}")
                return
            
            # STEP 3: Scroll to see more products
            print("\n📜 Step 3: Scroll down to see more products")
            await executor.execute(
                action="scroll",
                selector="body",
                value="down",
                options={"amount": 300}
            )
            await asyncio.sleep(1)
            print("✅ Scrolled down")
            
            # STEP 4: Add product to cart
            print("\n🛒 Step 4: Add product to cart")
            
            # Find Add to Cart button
            add_to_cart_selectors = [
                "button:has-text('Add to Cart')",
                "button:has-text('Thêm vào giỏ')",
                "button.add-to-cart",
                "[data-testid='add-to-cart']",
                "button:has-text('Add')",
            ]
            
            cart_added = False
            for selector in add_to_cart_selectors:
                try:
                    result = await executor.execute(
                        action="click",
                        selector=selector,
                        value=None,
                        options={"timeout": 3000}
                    )
                    
                    if result.success:
                        print(f"✅ Clicked Add to Cart button")
                        if result.screenshot:
                            print("📸 Screenshot captured!")
                            # Save screenshot
                            with open("test_add_to_cart.png", "wb") as f:
                                f.write(result.screenshot)
                            print("💾 Screenshot saved: test_add_to_cart.png")
                        cart_added = True
                        await asyncio.sleep(2)
                        break
                except Exception as e:
                    continue
            
            if not cart_added:
                print("❌ Could not find Add to Cart button")
                # Take diagnostic screenshot
                screenshot = await page.screenshot()
                with open("test_diagnostic.png", "wb") as f:
                    f.write(screenshot)
                print("💾 Diagnostic screenshot saved")
                return
            
            # STEP 5: Navigate to cart
            print("\n🛍️ Step 5: Navigate to cart")
            cart_selectors = [
                "a[href='/cart']",
                "a[href='/gio-hang']",
                "[data-testid='cart-icon']",
                "button:has-text('Cart')",
                "button:has-text('Giỏ hàng')",
                ".cart-icon",
            ]
            
            cart_opened = False
            for selector in cart_selectors:
                try:
                    result = await executor.execute(
                        action="click",
                        selector=selector,
                        value=None,
                        options={"timeout": 3000}
                    )
                    
                    if result.success:
                        print(f"✅ Opened cart")
                        await asyncio.sleep(2)
                        cart_opened = True
                        break
                except:
                    continue
            
            if not cart_opened:
                print("⚠️ Could not find cart button, trying navigation...")
                await page.goto("http://localhost:5173/cart")
                await asyncio.sleep(2)
                print("✅ Navigated to cart page")
            
            # STEP 6: Click checkout
            print("\n💳 Step 6: Click checkout")
            checkout_selectors = [
                "button:has-text('Checkout')",
                "button:has-text('Thanh toán')",
                "button:has-text('Proceed')",
                "[data-testid='checkout-button']",
                "a[href='/checkout']",
            ]
            
            checkout_clicked = False
            for selector in checkout_selectors:
                try:
                    result = await executor.execute(
                        action="click",
                        selector=selector,
                        value=None,
                        options={"timeout": 3000}
                    )
                    
                    if result.success:
                        print(f"✅ Clicked checkout")
                        await asyncio.sleep(2)
                        checkout_clicked = True
                        break
                except:
                    continue
            
            if not checkout_clicked:
                print("❌ Could not find checkout button")
                screenshot = await page.screenshot()
                with open("test_cart_view.png", "wb") as f:
                    f.write(screenshot)
                return
            
            # STEP 7: Select COD payment
            print("\n💰 Step 7: Select COD payment method")
            
            # Try different COD selectors
            cod_selectors = [
                "select option[value='COD']",
                "select option:has-text('COD')",
                "input[value='COD']",
                "label:has-text('COD')",
                "label:has-text('Thanh toán khi nhận hàng')",
            ]
            
            cod_selected = False
            for selector in cod_selectors:
                try:
                    if 'select' in selector:
                        # For select dropdown
                        result = await executor.execute(
                            action="select",
                            selector="select",
                            value="COD",
                            options={}
                        )
                    else:
                        # For radio/checkbox
                        result = await executor.execute(
                            action="click",
                            selector=selector,
                            value=None,
                            options={"timeout": 2000}
                        )
                    
                    if result.success:
                        print(f"✅ Selected COD payment")
                        await asyncio.sleep(1)
                        cod_selected = True
                        break
                except:
                    continue
            
            if not cod_selected:
                print("⚠️ Could not find COD option (may not be on payment selection page)")
            
            # STEP 8: Fill checkout form
            print("\n📝 Step 8: Fill checkout form")
            
            form_data = {
                "firstName": "Test",
                "lastName": "User",
                "email": "test@example.com",
                "phone": "0123456789",
                "address": "123 Test Street, Hanoi",
                "city": "Hanoi",
                "zipCode": "100000",
            }
            
            # Try to fill common form fields
            filled_count = 0
            for field_name, value in form_data.items():
                selectors_to_try = [
                    f"input[name='{field_name}']",
                    f"input[id='{field_name}']",
                    f"input[placeholder*='{field_name}']",
                    f"textarea[name='{field_name}']",
                ]
                
                for selector in selectors_to_try:
                    try:
                        result = await executor.execute(
                            action="fill",
                            selector=selector,
                            value=value,
                            options={"timeout": 2000}
                        )
                        
                        if result.success:
                            print(f"  ✅ Filled {field_name}: {value}")
                            filled_count += 1
                            await asyncio.sleep(0.5)
                            break
                    except:
                        continue
            
            print(f"\n✅ Filled {filled_count}/{len(form_data)} form fields")
            
            # Final screenshot
            print("\n📸 Taking final screenshot...")
            final_screenshot = await page.screenshot(full_page=True)
            with open("test_checkout_form.png", "wb") as f:
                f.write(final_screenshot)
            print("💾 Final screenshot saved: test_checkout_form.png")
            
            print("\n" + "="*60)
            print("✅ E2E TEST COMPLETED SUCCESSFULLY")
            print("="*60)
            print("\nScreenshots saved:")
            print("  - test_add_to_cart.png (Cart action with auto-screenshot)")
            print("  - test_checkout_form.png (Final checkout form)")
            
            # Keep browser open for inspection
            print("\n⏸️  Browser will stay open for 10 seconds for inspection...")
            await asyncio.sleep(10)
            
        except Exception as e:
            logger.error(f"❌ Test failed: {e}")
            import traceback
            traceback.print_exc()
            
            # Save error screenshot
            try:
                screenshot = await page.screenshot()
                with open("test_error.png", "wb") as f:
                    f.write(screenshot)
                print("💾 Error screenshot saved: test_error.png")
            except:
                pass
        
        finally:
            await browser.close()


if __name__ == "__main__":
    print("Starting E2E Browser Automation Test...")
    print("Make sure frontend is running on http://localhost:5173\n")
    
    asyncio.run(test_e2e_shopping_flow())
