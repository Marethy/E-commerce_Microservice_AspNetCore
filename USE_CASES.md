# 📚 USE CASES - E-Commerce Microservices Platform

## 📋 Mục lục
- [Giới thiệu](#giới-thiệu)
- [Use Cases Hiện Có](#use-cases-hiện-có)
- [Use Cases Được Đề Xuất](#use-cases-được-đề-xuất)

---

## 🎯 Giới thiệu

Tài liệu này mô tả chi tiết các use case (trường hợp sử dụng) của hệ thống E-Commerce Microservices, bao gồm:
- **Use Cases Hiện Có**: Các tính năng đã được triển khai trong hệ thống
- **Use Cases Được Đề Xuất**: Các tính năng nên được phát triển thêm để hoàn thiện hệ thống

---

## ✅ Use Cases Hiện Có

### 1️⃣ **Authentication & Authorization**

#### UC-AUTH-001: Đăng ký tài khoản
- **Mô tả**: Người dùng tạo tài khoản mới trên hệ thống
- **Actor**: Khách (Guest)
- **Service**: Identity Service
- **Luồng chính**:
  1. Người dùng cung cấp thông tin: username, email, password, firstname, lastname
  2. Hệ thống validate dữ liệu
  3. Tạo tài khoản mới trong database
  4. Gửi email xác nhận (nếu bật verification)
  5. Trả về thông báo thành công

#### UC-AUTH-002: Đăng nhập
- **Mô tả**: Người dùng đăng nhập vào hệ thống
- **Actor**: User
- **Service**: Identity Service
- **Luồng chính**:
  1. Người dùng cung cấp username và password
  2. Hệ thống xác thực thông tin
  3. Tạo JWT access token và refresh token
  4. Trả về tokens cho client
  5. Client lưu tokens để sử dụng cho các request tiếp theo

#### UC-AUTH-003: Làm mới token
- **Mô tả**: Gia hạn access token khi hết hạn
- **Actor**: User
- **Service**: Identity Service
- **Luồng chính**:
  1. Client gửi refresh token
  2. Hệ thống validate refresh token
  3. Tạo access token mới
  4. Trả về access token mới

---

### 2️⃣ **Product Management**

#### UC-PROD-001: Xem danh sách sản phẩm
- **Mô tả**: Người dùng xem danh sách sản phẩm với phân trang
- **Actor**: User/Guest
- **Service**: Product Service
- **Luồng chính**:
  1. Người dùng truy cập trang danh sách sản phẩm
  2. Hệ thống trả về danh sách sản phẩm theo trang (page, limit)
  3. Hiển thị thông tin cơ bản: tên, giá, hình ảnh, category, brand

#### UC-PROD-002: Tìm kiếm sản phẩm
- **Mô tả**: Người dùng tìm kiếm sản phẩm theo từ khóa và bộ lọc
- **Actor**: User/Guest
- **Service**: Product Service
- **Luồng chính**:
  1. Người dùng nhập từ khóa tìm kiếm
  2. Áp dụng bộ lọc: category, brand, khoảng giá (minPrice, maxPrice)
  3. Hệ thống tìm kiếm trong database
  4. Trả về kết quả phân trang

#### UC-PROD-003: Xem chi tiết sản phẩm
- **Mô tả**: Người dùng xem thông tin chi tiết sản phẩm
- **Actor**: User/Guest
- **Service**: Product Service
- **Luồng chính**:
  1. Người dùng click vào sản phẩm (theo id/no/slug)
  2. Hệ thống lấy thông tin đầy đủ: mô tả, hình ảnh, specifications, seller
  3. Hiển thị chi tiết sản phẩm

#### UC-PROD-004: Quản lý sản phẩm (Admin)
- **Mô tả**: Admin tạo, sửa, xóa sản phẩm
- **Actor**: Admin
- **Service**: Product Service
- **Luồng chính**:
  - **Tạo**: Cung cấp thông tin sản phẩm → validate → lưu vào DB
  - **Sửa**: Cập nhật thông tin → validate → lưu thay đổi
  - **Xóa**: Xác nhận → xóa khỏi DB

#### UC-PROD-005: Quản lý danh mục (Categories)
- **Mô tả**: Xem và quản lý danh mục sản phẩm
- **Actor**: User/Guest (xem), Admin (quản lý)
- **Service**: Product Service
- **Chức năng**:
  - Lấy danh sách categories
  - Hỗ trợ cấu trúc phân cấp (parent-child)
  - Đếm số sản phẩm trong category

#### UC-PROD-006: Quản lý thương hiệu (Brands)
- **Mô tả**: Xem và quản lý thương hiệu sản phẩm
- **Actor**: User/Guest (xem), Admin (quản lý)
- **Service**: Product Service
- **Chức năng**:
  - Lấy danh sách brands
  - Hiển thị logo, mô tả
  - Đếm số sản phẩm của brand

#### UC-PROD-007: Đánh giá sản phẩm (Product Reviews)
- **Mô tả**: Người dùng đánh giá và xem review sản phẩm
- **Actor**: User (authenticated)
- **Service**: Product Service
- **Luồng chính**:
  1. User đã mua sản phẩm có thể viết review
  2. Cung cấp rating (1-5 sao), comment
  3. Hệ thống kiểm tra quyền (đã mua chưa qua Order Service)
  4. Lưu review
  5. Hiển thị reviews của sản phẩm

#### UC-PROD-008: Wishlist (Danh sách yêu thích)
- **Mô tả**: Người dùng lưu sản phẩm yêu thích
- **Actor**: User (authenticated)
- **Service**: Product Service
- **Chức năng**:
  - Thêm sản phẩm vào wishlist
  - Xóa sản phẩm khỏi wishlist
  - Xem danh sách wishlist

#### UC-PROD-009: Quản lý Sellers
- **Mô tả**: Quản lý thông tin người bán
- **Actor**: Admin
- **Service**: Product Service
- **Chức năng**:
  - CRUD sellers
  - Liên kết sản phẩm với sellers

---

### 3️⃣ **Shopping Cart (Basket)**

#### UC-BASKET-001: Xem giỏ hàng
- **Mô tả**: Người dùng xem giỏ hàng hiện tại
- **Actor**: User/Guest
- **Service**: Basket Service
- **Luồng chính**:
  1. Lấy giỏ hàng theo username (hoặc guest_id)
  2. Hiển thị danh sách items: tên, giá, số lượng, hình ảnh
  3. Tính tổng giá trị giỏ hàng

#### UC-BASKET-002: Thêm/Cập nhật sản phẩm vào giỏ
- **Mô tả**: Người dùng thêm hoặc cập nhật số lượng sản phẩm
- **Actor**: User/Guest
- **Service**: Basket Service
- **Luồng chính**:
  1. Người dùng chọn sản phẩm và số lượng
  2. Gửi request cập nhật giỏ hàng
  3. Hệ thống cập nhật Redis cache
  4. Track activity cho AI analytics
  5. Trả về giỏ hàng đã cập nhật

#### UC-BASKET-003: Xóa giỏ hàng
- **Mô tả**: Xóa toàn bộ giỏ hàng
- **Actor**: User/Guest
- **Service**: Basket Service
- **Luồng chính**:
  1. Người dùng yêu cầu xóa giỏ
  2. Xóa dữ liệu từ Redis
  3. Trả về thông báo thành công

#### UC-BASKET-004: Checkout (Thanh toán)
- **Mô tả**: Người dùng thanh toán giỏ hàng
- **Actor**: User (authenticated)
- **Service**: Basket Service → Order Service (via RabbitMQ)
- **Luồng chính**:
  1. Người dùng cung cấp thông tin: địa chỉ, phương thức thanh toán
  2. Validate tồn kho (gọi Inventory Service qua gRPC)
  3. Tạo BasketCheckoutEvent
  4. Publish event lên RabbitMQ
  5. Order Service consume event và tạo đơn hàng
  6. Xóa giỏ hàng sau khi checkout thành công

#### UC-BASKET-005: Lấy số lượng items
- **Mô tả**: Hiển thị badge số lượng sản phẩm trong giỏ
- **Actor**: User/Guest
- **Service**: Basket Service
- **Luồng chính**:
  1. Client request số lượng items
  2. Trả về tổng số items trong giỏ (để hiển thị badge)

#### UC-BASKET-006: Validate giỏ hàng
- **Mô tả**: Kiểm tra tồn kho trước khi checkout
- **Actor**: User
- **Service**: Basket Service → Inventory Service (gRPC)
- **Luồng chính**:
  1. Lấy danh sách items trong giỏ
  2. Gọi Inventory Service để kiểm tra stock
  3. Trả về danh sách items không có sẵn (nếu có)
  4. Client hiển thị cảnh báo

#### UC-BASKET-007: Merge giỏ hàng Guest với User
- **Mô tả**: Gộp giỏ hàng guest vào user sau khi login
- **Actor**: User (vừa login)
- **Service**: Basket Service
- **Luồng chính**:
  1. User login (trước đó là guest)
  2. Lấy giỏ guest (guest_xxxxx) và giỏ user
  3. Merge items: cộng số lượng nếu trùng sản phẩm
  4. Lưu vào giỏ user
  5. Xóa giỏ guest

---

### 4️⃣ **Order Management**

#### UC-ORDER-001: Xem danh sách đơn hàng
- **Mô tả**: Người dùng xem đơn hàng của mình
- **Actor**: User (authenticated)
- **Service**: Order Service
- **Luồng chính**:
  1. Lấy danh sách đơn hàng theo username
  2. Hiển thị: order number, ngày, tổng giá, trạng thái
  3. Hỗ trợ phân trang

#### UC-ORDER-002: Xem chi tiết đơn hàng
- **Mô tả**: Xem thông tin chi tiết đơn hàng
- **Actor**: User (authenticated)
- **Service**: Order Service
- **Luồng chính**:
  1. Lấy đơn hàng theo ID
  2. Hiển thị: items, địa chỉ giao hàng, phương thức thanh toán, trạng thái

#### UC-ORDER-003: Tạo đơn hàng
- **Mô tả**: Tạo đơn hàng mới (thường qua Basket checkout)
- **Actor**: System (RabbitMQ Consumer) hoặc API call
- **Service**: Order Service
- **Luồng chính**:
  1. Nhận BasketCheckoutEvent từ RabbitMQ
  2. Tạo Order entity với items
  3. Lưu vào SQL Server database
  4. Log vào Elasticsearch
  5. Trả về order ID

#### UC-ORDER-004: Cập nhật trạng thái đơn hàng
- **Mô tả**: Admin cập nhật trạng thái đơn hàng
- **Actor**: Admin
- **Service**: Order Service
- **Luồng chính**:
  1. Admin chọn đơn hàng và trạng thái mới: Processing, Shipped, Delivered, Cancelled
  2. Cập nhật trong database
  3. Gửi notification cho khách hàng (nếu có)

#### UC-ORDER-005: Hủy đơn hàng
- **Mô tả**: Người dùng hoặc Admin hủy đơn hàng
- **Actor**: User/Admin
- **Service**: Order Service
- **Luồng chính**:
  1. Kiểm tra trạng thái đơn hàng (chỉ hủy được nếu Pending/Processing)
  2. Cung cấp lý do hủy (optional)
  3. Cập nhật trạng thái = Cancelled
  4. Hoàn lại stock (gọi Inventory Service)

#### UC-ORDER-006: Xem tất cả đơn hàng (Admin)
- **Mô tả**: Admin xem tất cả đơn hàng trong hệ thống
- **Actor**: Admin
- **Service**: Order Service
- **Luồng chính**:
  1. Lấy danh sách tất cả đơn hàng
  2. Hỗ trợ lọc theo status
  3. Phân trang

#### UC-ORDER-007: Thống kê đơn hàng (Admin)
- **Mô tả**: Xem thống kê tổng quan đơn hàng
- **Actor**: Admin
- **Service**: Order Service
- **Luồng chính**:
  1. Tính tổng số đơn hàng
  2. Phân loại theo trạng thái: Pending, Processing, Shipped, Delivered, Cancelled
  3. Tính tổng doanh thu

#### UC-ORDER-008: Kiểm tra lịch sử mua hàng
- **Mô tả**: Kiểm tra user đã mua sản phẩm chưa (cho phép review)
- **Actor**: System/User
- **Service**: Order Service
- **Luồng chính**:
  1. Nhận productNo và userName
  2. Tìm trong orders của user
  3. Trả về: đã mua (hasPurchased), ngày mua (purchaseDate)

#### UC-ORDER-009: Báo cáo đơn hàng
- **Mô tả**: Tạo báo cáo về đơn hàng
- **Actor**: Admin
- **Service**: Order Service (OrderReportsController)
- **Chức năng**:
  - Báo cáo theo thời gian
  - Báo cáo doanh thu
  - Export to Excel/PDF

---

### 5️⃣ **Customer Management**

#### UC-CUST-001: Xem thông tin khách hàng
- **Mô tả**: Người dùng xem profile của mình
- **Actor**: User (authenticated)
- **Service**: Customer Service
- **Luồng chính**:
  1. Lấy thông tin theo username
  2. Hiển thị: email, tên, số điện thoại, địa chỉ, thành phố, postal code, quốc gia

#### UC-CUST-002: Cập nhật thông tin khách hàng
- **Mô tả**: Người dùng cập nhật profile
- **Actor**: User (authenticated)
- **Service**: Customer Service
- **Luồng chính**:
  1. Người dùng chỉnh sửa thông tin
  2. Validate dữ liệu
  3. Cập nhật trong PostgreSQL database
  4. Trả về kết quả

#### UC-CUST-003: Quản lý Notifications
- **Mô tả**: Xem và quản lý thông báo
- **Actor**: User (authenticated)
- **Service**: Customer Service (NotificationsController)
- **Chức năng**:
  - Lấy danh sách notifications
  - Đánh dấu đã đọc
  - Xóa notification

---

### 6️⃣ **Inventory Management**

#### UC-INV-001: Kiểm tra tồn kho (gRPC)
- **Mô tả**: Các service khác kiểm tra số lượng tồn kho
- **Actor**: System (Basket Service, Order Service)
- **Service**: Inventory Service (gRPC)
- **Luồng chính**:
  1. Service gọi gRPC endpoint với productNo và requestedQuantity
  2. Inventory Service kiểm tra stock trong MongoDB
  3. Trả về: available (true/false), availableQuantity

#### UC-INV-002: Cập nhật tồn kho
- **Mô tả**: Cập nhật số lượng tồn kho (sau khi order, hoặc nhập hàng)
- **Actor**: Admin/System
- **Service**: Inventory Service
- **Luồng chính**:
  1. Nhận productNo và quantity thay đổi
  2. Cập nhật trong MongoDB
  3. Log vào Elasticsearch

#### UC-INV-003: Quản lý Inventory qua REST API
- **Mô tả**: Admin quản lý inventory qua REST API
- **Actor**: Admin
- **Service**: Inventory Product API
- **Chức năng**:
  - CRUD inventory entries
  - Xem lịch sử thay đổi stock

---

### 7️⃣ **AI Chatbot**

#### UC-CHAT-001: Chat với AI Assistant
- **Mô tả**: Người dùng chat với AI để tương tác với hệ thống
- **Actor**: User/Guest
- **Service**: Chatbot Service
- **Luồng chính**:
  1. User gửi message qua SSE endpoint
  2. Chatbot phân tích ý định (search, add to cart, view order...)
  3. Gọi MCP Service để discover tools
  4. Execute tool phù hợp (qua API hoặc browser automation)
  5. Stream response về client theo từng bước:
     - thinking: đang suy nghĩ
     - searching: tìm tools
     - searched: đã tìm thấy tools
     - executing: đang thực thi
     - executed: đã thực thi
     - content: nội dung phản hồi
     - done: hoàn thành

#### UC-CHAT-002: Xem lịch sử chat
- **Mô tả**: Xem lại cuộc trò chuyện trước đó
- **Actor**: User/Guest
- **Service**: Chatbot Service
- **Luồng chính**:
  1. Lấy session_id
  2. Truy vấn SQLite database
  3. Trả về danh sách messages

#### UC-CHAT-003: Xóa session chat
- **Mô tả**: Xóa lịch sử chat
- **Actor**: User/Guest
- **Service**: Chatbot Service
- **Luồng chính**:
  1. Nhận session_id
  2. Xóa messages trong database

---

### 8️⃣ **MCP (Model Context Protocol) Service**

#### UC-MCP-001: Discover Tools (WebSocket)
- **Mô tả**: Chatbot tìm kiếm công cụ phù hợp
- **Actor**: Chatbot Service
- **Service**: MCP Service
- **Luồng chính**:
  1. Nhận query từ Chatbot qua WebSocket
  2. Embedding query bằng OpenAI embedding model
  3. Tính cosine similarity với tool embeddings
  4. Trả về top 5 tools có similarity cao nhất
  5. Bao gồm: API tools (search_products, get_cart...) và Browser tools (click, fill...)

#### UC-MCP-002: Execute Tool (WebSocket)
- **Mô tả**: Thực thi công cụ
- **Actor**: Chatbot Service
- **Service**: MCP Service
- **Luồng chính**:
  1. Nhận tool_name và arguments từ Chatbot
  2. Inject auth_token nếu có
  3. Thực thi tool function (API call hoặc browser action)
  4. Trả về kết quả hoặc error

#### UC-MCP-003: Browser Automation
- **Mô tả**: Điều khiển trình duyệt để thực hiện actions
- **Actor**: Chatbot Service (via MCP)
- **Service**: MCP Service (Playwright)
- **Các actions**:
  - browser_navigate: điều hướng đến URL
  - browser_click: click element
  - browser_fill: nhập text vào input
  - browser_scroll: scroll trang
  - browser_screenshot: chụp màn hình
  - browser_get_text: lấy text từ element

#### UC-MCP-004: E-commerce API Tools
- **Mô tả**: Các tool để gọi E-commerce APIs
- **Actor**: Chatbot Service (via MCP)
- **Service**: MCP Service → API Gateway → Microservices
- **Các tools**:
  - search_products: tìm sản phẩm
  - get_product_detail: chi tiết sản phẩm
  - get_categories: danh mục
  - get_brands: thương hiệu
  - get_cart: giỏ hàng
  - update_cart: thêm/sửa giỏ
  - checkout_cart: thanh toán
  - get_user_orders: đơn hàng
  - get_order_detail: chi tiết đơn
  - get_customer: thông tin khách hàng

---

### 9️⃣ **Scheduled Jobs (Hangfire)**

#### UC-JOB-001: Quản lý Scheduled Jobs
- **Mô tả**: Admin quản lý jobs định kỳ
- **Actor**: Admin
- **Service**: Hangfire API
- **Chức năng**:
  - Tạo recurring jobs
  - Xem danh sách jobs
  - Xóa jobs
  - Xem lịch sử thực thi

#### UC-JOB-002: Hangfire Dashboard
- **Mô tả**: Xem dashboard để monitor jobs
- **Actor**: Admin
- **Service**: Hangfire API
- **Chức năng**:
  - Xem jobs đang chạy
  - Xem jobs thành công/thất bại
  - Retry failed jobs

---

### 🔟 **API Gateway & Health Monitoring**

#### UC-GW-001: Route Requests
- **Mô tả**: API Gateway route requests đến microservices
- **Actor**: Client (FE, Mobile)
- **Service**: API Gateway (Ocelot)
- **Luồng chính**:
  1. Client gửi request đến gateway endpoint
  2. Ocelot route đến downstream service tương ứng
  3. Áp dụng rate limiting, authentication
  4. Trả về response

#### UC-GW-002: Health Check Monitoring
- **Mô tả**: Monitoring health của các services
- **Actor**: Admin/System
- **Service**: Web Health Status
- **Luồng chính**:
  1. Web Health Status UI ping /hc endpoint của các services
  2. Hiển thị status: Healthy/Unhealthy
  3. Gửi alert nếu service down

---

## 🆕 Use Cases Được Đề Xuất

### 1️⃣ **Payment Integration**

#### UC-PAY-001: Thanh toán Online
- **Mô tả**: Tích hợp cổng thanh toán online (VNPay, MoMo, Stripe...)
- **Actor**: User
- **Service**: Payment Service (mới)
- **Lý do**: Hiện tại chỉ hỗ trợ COD, cần thêm thanh toán online
- **Luồng đề xuất**:
  1. User chọn phương thức thanh toán online
  2. Redirect đến payment gateway
  3. Xử lý callback
  4. Cập nhật trạng thái đơn hàng

#### UC-PAY-002: Quản lý Wallet
- **Mô tả**: Ví điện tử nội bộ
- **Actor**: User
- **Service**: Payment Service
- **Lý do**: Tăng trải nghiệm người dùng, giảm phí giao dịch
- **Chức năng**:
  - Nạp tiền vào ví
  - Thanh toán bằng ví
  - Xem lịch sử giao dịch

---

### 2️⃣ **Promotion & Discount**

#### UC-PROMO-001: Quản lý Coupons
- **Mô tả**: Hệ thống mã giảm giá
- **Actor**: Admin (tạo), User (sử dụng)
- **Service**: Promotion Service (mới)
- **Lý do**: Tăng conversion rate, marketing
- **Chức năng**:
  - Tạo coupon với điều kiện: giảm %, giảm tiền, min order
  - Áp dụng coupon khi checkout
  - Kiểm tra validity, số lần sử dụng

#### UC-PROMO-002: Flash Sale
- **Mô tả**: Khuyến mãi giới hạn thời gian
- **Actor**: Admin (tạo), User (mua)
- **Service**: Promotion Service
- **Lý do**: Tăng doanh thu trong thời gian ngắn
- **Chức năng**:
  - Tạo flash sale với thời gian bắt đầu/kết thúc
  - Giới hạn số lượng
  - Real-time countdown

---

### 3️⃣ **Product Recommendations**

#### UC-RECOM-001: Gợi ý sản phẩm cá nhân hóa
- **Mô tả**: AI gợi ý sản phẩm dựa trên hành vi
- **Actor**: User/Guest
- **Service**: Recommendation Service (mới, ML-based)
- **Lý do**: Tăng cross-selling, upselling
- **Thuật toán**:
  - Collaborative filtering
  - Content-based filtering
  - Hybrid approach

#### UC-RECOM-002: Sản phẩm liên quan
- **Mô tả**: Hiển thị sản phẩm tương tự
- **Actor**: User/Guest
- **Service**: Product Service + Recommendation
- **Chức năng**:
  - "Frequently bought together"
  - "Customers also viewed"
  - "Similar products"

---

### 4️⃣ **Shipping & Logistics**

#### UC-SHIP-001: Tích hợp đối tác vận chuyển
- **Mô tả**: Tích hợp GHN, GHTK, Viettel Post...
- **Actor**: System/Admin
- **Service**: Shipping Service (mới)
- **Lý do**: Tự động tính phí ship, tracking
- **Chức năng**:
  - Tính phí vận chuyển real-time
  - Tạo đơn vận chuyển
  - Tracking đơn hàng
  - Cập nhật trạng thái tự động

#### UC-SHIP-002: Địa chỉ giao hàng nhiều
- **Mô tả**: User lưu nhiều địa chỉ
- **Actor**: User
- **Service**: Customer Service (mở rộng)
- **Chức năng**:
  - Thêm/sửa/xóa địa chỉ
  - Đặt địa chỉ mặc định
  - Chọn địa chỉ khi checkout

---

### 5️⃣ **Advanced Search & Filters**

#### UC-SEARCH-001: Tìm kiếm nâng cao
- **Mô tả**: Tìm kiếm với nhiều tiêu chí
- **Actor**: User/Guest
- **Service**: Product Service (cải tiến) hoặc Elasticsearch
- **Lý do**: Cải thiện UX, tìm sản phẩm nhanh hơn
- **Chức năng**:
  - Faceted search (filters đa cấp)
  - Auto-suggest khi gõ
  - Search history
  - Popular searches

#### UC-SEARCH-002: Visual Search
- **Mô tả**: Tìm kiếm bằng hình ảnh
- **Actor**: User/Guest
- **Service**: Search Service (mới, AI-based)
- **Lý do**: Xu hướng mới, tăng conversion
- **Công nghệ**: Computer Vision, Image Embedding

---

### 6️⃣ **Social & Community**

#### UC-SOCIAL-001: Chia sẻ sản phẩm lên mạng xã hội
- **Mô tả**: Share lên Facebook, Twitter, Pinterest
- **Actor**: User/Guest
- **Service**: Product Service (mở rộng)
- **Lý do**: Marketing tự nhiên, viral

#### UC-SOCIAL-002: Q&A sản phẩm
- **Mô tả**: Hỏi đáp về sản phẩm
- **Actor**: User
- **Service**: Product Service (mở rộng)
- **Chức năng**:
  - Đặt câu hỏi
  - Seller/Admin trả lời
  - Vote câu hỏi hữu ích

---

### 7️⃣ **Customer Support**

#### UC-SUPPORT-001: Live Chat với nhân viên
- **Mô tả**: Chat trực tiếp với support team
- **Actor**: User
- **Service**: Support Service (mới)
- **Lý do**: Hỗ trợ khách hàng real-time
- **Công nghệ**: WebSocket, Queue system

#### UC-SUPPORT-002: Ticket System
- **Mô tả**: Hệ thống ticket hỗ trợ
- **Actor**: User (tạo), Admin (xử lý)
- **Service**: Support Service
- **Chức năng**:
  - Tạo ticket với category
  - Assign cho agent
  - Update status
  - Close ticket

---

### 8️⃣ **Loyalty & Rewards**

#### UC-LOYALTY-001: Chương trình tích điểm
- **Mô tả**: Tích điểm khi mua hàng
- **Actor**: User
- **Service**: Loyalty Service (mới)
- **Lý do**: Giữ chân khách hàng, tăng retention
- **Chức năng**:
  - Tích điểm theo đơn hàng
  - Quy đổi điểm thành voucher
  - Xem lịch sử điểm

#### UC-LOYALTY-002: Membership Tiers
- **Mô tả**: Phân hạng khách hàng (Silver, Gold, Platinum)
- **Actor**: User
- **Service**: Loyalty Service
- **Chức năng**:
  - Tự động nâng hạng
  - Ưu đãi riêng theo hạng
  - Free shipping cho hạng cao

---

### 9️⃣ **Analytics & Reporting**

#### UC-ANALYTICS-001: Dashboard Admin
- **Mô tả**: Dashboard tổng quan doanh nghiệp
- **Actor**: Admin
- **Service**: Analytics Service (mới)
- **Metrics**:
  - Doanh thu theo thời gian
  - Top selling products
  - Conversion rate
  - User growth
  - Cart abandonment rate

#### UC-ANALYTICS-002: User Behavior Tracking
- **Mô tả**: Phân tích hành vi người dùng
- **Actor**: System
- **Service**: Analytics Service
- **Lý do**: Cải thiện UX, marketing
- **Chức năng**:
  - Track page views, clicks
  - Funnel analysis
  - Heatmap
  - Session recording

---

### 🔟 **Mobile App Support**

#### UC-MOBILE-001: Push Notifications
- **Mô tả**: Gửi thông báo đến mobile app
- **Actor**: System
- **Service**: Notification Service (mới)
- **Lý do**: Tăng engagement
- **Chức năng**:
  - Order updates
  - Promotion alerts
  - Personalized recommendations

#### UC-MOBILE-002: Deep Linking
- **Mô tả**: Mở app đến màn hình cụ thể
- **Actor**: User
- **Service**: Mobile Backend (mới)
- **Chức năng**:
  - Link từ email/SMS vào app
  - Share product link

---

### 1️⃣1️⃣ **Security & Fraud Prevention**

#### UC-SEC-001: Two-Factor Authentication (2FA)
- **Mô tả**: Xác thực 2 lớp
- **Actor**: User
- **Service**: Identity Service (mở rộng)
- **Lý do**: Bảo mật tài khoản
- **Chức năng**:
  - SMS OTP
  - Email OTP
  - Authenticator app

#### UC-SEC-002: Fraud Detection
- **Mô tả**: Phát hiện giao dịch gian lận
- **Actor**: System
- **Service**: Fraud Detection Service (mới, ML-based)
- **Lý do**: Giảm rủi ro
- **Chức năng**:
  - Phát hiện đơn hàng bất thường
  - Block suspicious users
  - Manual review queue

---

### 1️⃣2️⃣ **Multi-tenant & Vendor Management**

#### UC-VENDOR-001: Marketplace cho nhiều sellers
- **Mô tả**: Chuyển từ single-seller sang marketplace
- **Actor**: Vendor/Seller
- **Service**: Vendor Service (mới)
- **Lý do**: Scale business model
- **Chức năng**:
  - Đăng ký vendor
  - Vendor dashboard
  - Commission management
  - Payout system

---

### 1️⃣3️⃣ **Internationalization (i18n)**

#### UC-I18N-001: Đa ngôn ngữ
- **Mô tả**: Hỗ trợ nhiều ngôn ngữ
- **Actor**: User/Guest
- **Service**: Tất cả services
- **Lý do**: Mở rộng thị trường
- **Chức năng**:
  - Translate UI
  - Localized content
  - Language switcher

#### UC-I18N-002: Đa tiền tệ
- **Mô tả**: Hỗ trợ nhiều loại tiền
- **Actor**: User/Guest
- **Service**: Product Service, Order Service
- **Chức năng**:
  - Display prices in multiple currencies
  - Real-time exchange rates
  - Currency converter

---

### 1️⃣4️⃣ **Content Management**

#### UC-CMS-001: Quản lý nội dung động
- **Mô tả**: CMS cho banners, landing pages
- **Actor**: Admin
- **Service**: CMS Service (mới)
- **Lý do**: Marketing linh hoạt không cần dev
- **Chức năng**:
  - Create/edit banners
  - Landing page builder
  - Content scheduling

---

## 📊 Tổng kết

### Use Cases Hiện Có: **45+ use cases**
Hệ thống đã có đầy đủ các chức năng cơ bản cho một nền tảng E-commerce:
- ✅ Authentication & User Management
- ✅ Product Catalog & Search
- ✅ Shopping Cart & Checkout
- ✅ Order Management
- ✅ Customer Profiles
- ✅ AI Chatbot với Tool Execution
- ✅ Inventory Management
- ✅ Scheduled Jobs
- ✅ Health Monitoring

### Use Cases Được Đề Xuất: **30+ use cases**
Các tính năng nên được phát triển tiếp theo theo thứ tự ưu tiên:

**Priority HIGH** (Critical for business):
1. 💳 **Payment Integration** - Thanh toán online
2. 🎫 **Promotion & Coupons** - Marketing tools
3. 🚚 **Shipping Integration** - Tối ưu logistics

**Priority MEDIUM** (Enhance UX):
4. 🔍 **Advanced Search** - Cải thiện trải nghiệm tìm kiếm
5. ⭐ **Product Recommendations** - AI-based
6. 📱 **Push Notifications** - Mobile engagement
7. 🎁 **Loyalty Program** - Customer retention

**Priority LOW** (Nice to have):
8. 🌐 **i18n** - Đa ngôn ngữ/tiền tệ
9. 🏪 **Marketplace** - Multi-vendor
10. 📊 **Advanced Analytics** - Business intelligence
11. 🔒 **2FA** - Enhanced security
12. 📸 **Visual Search** - AI innovation

---

## 📚 Tham khảo
- [API Documentation](./API_DOCUMENTATION.md)
- [Services Architecture](./SERVICES_ARCHITECTURE.md)
- [AI Workflow](./AI_WORKFLOW.md)
