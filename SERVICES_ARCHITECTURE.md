# 🏗️ SERVICES ARCHITECTURE - E-Commerce Microservices

## 📋 Mục lục
- [Tổng quan kiến trúc](#tổng-quan-kiến-trúc)
- [Chi tiết từng Service](#chi-tiết-từng-service)
- [Giao tiếp giữa các Services](#giao-tiếp-giữa-các-services)
- [Database Architecture](#database-architecture)
- [Frontend Integration](#frontend-integration)
- [Infrastructure Components](#infrastructure-components)

---

## 🎯 Tổng quan kiến trúc

### Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        FE[Frontend - React/Next.js<br/>Port: 5173]
    end

    subgraph "API Gateway Layer"
        AGW[API Gateway - Ocelot<br/>Port: 5000]
    end

    subgraph "Microservices Layer (.NET)"
        IDP[Identity Service<br/>Port: 6001]
        PROD[Product Service<br/>Port: 6002]
        CUST[Customer Service<br/>Port: 6003]
        BASKET[Basket Service<br/>Port: 6004]
        ORDER[Order Service<br/>Port: 6005]
        INV_API[Inventory API<br/>Port: 6006]
        INV_GRPC[Inventory gRPC<br/>Port: 6007]
        HF[Hangfire API<br/>Port: 6008]
    end

    subgraph "Python Services"
        CHAT[Chatbot Service<br/>Port: 6009]
        MCP[MCP Server<br/>gRPC: 50051, HTTP: 8001]
    end

    subgraph "Databases"
        MSSQL_ID[(Identity DB<br/>SQL Server)]
        MSSQL_ORD[(Order DB<br/>SQL Server)]
        PG_PROD[(Product DB<br/>PostgreSQL)]
        PG_CUST[(Customer DB<br/>PostgreSQL)]
        REDIS[(Basket DB<br/>Redis)]
        MONGO_INV[(Inventory DB<br/>MongoDB)]
        MONGO_HF[(Hangfire DB<br/>MongoDB)]
        SQLITE[(Chatbot DB<br/>SQLite)]
    end

    subgraph "Infrastructure"
        RMQ[RabbitMQ<br/>Port: 5672, 15672]
        ES[Elasticsearch<br/>Port: 9200]
        KB[Kibana<br/>Port: 5601]
    end

    %% Frontend connections
    FE -->|HTTP/REST| AGW
    FE -->|WebSocket| MCP
    FE -->|SSE| CHAT

    %% API Gateway routing
    AGW --> IDP
    AGW --> PROD
    AGW --> CUST
    AGW --> BASKET
    AGW --> ORDER
    AGW --> INV_API
    AGW --> CHAT

    %% Service to Database
    IDP --> MSSQL_ID
    ORDER --> MSSQL_ORD
    PROD --> PG_PROD
    CUST --> PG_CUST
    BASKET --> REDIS
    INV_API --> MONGO_INV
    INV_GRPC --> MONGO_INV
    HF --> MONGO_HF
    CHAT --> SQLITE
    CHAT --> REDIS

    %% Service to Service
    BASKET -->|gRPC| INV_GRPC
    BASKET -->|RabbitMQ| ORDER
    CHAT -->|gRPC| MCP
    MCP --> AGW

    %% Logging
    IDP --> ES
    ORDER --> ES
    CUST --> ES
    INV_API --> ES
    INV_GRPC --> ES
    HF --> ES
    ES --> KB
```

### Tổng quan Services

| Service | Port | Technology | Database | Purpose |
|---------|------|------------|----------|---------|
| **Frontend** | 5173 | React/Next.js | - | UI/UX cho người dùng |
| **API Gateway** | 5000 | Ocelot (.NET) | - | API Gateway, routing, rate limiting |
| **Identity Service** | 6001 | ASP.NET Core | SQL Server | Xác thực, phân quyền, JWT |
| **Product Service** | 6002 | ASP.NET Core | PostgreSQL | Quản lý sản phẩm, categories, brands |
| **Customer Service** | 6003 | ASP.NET Core | PostgreSQL | Quản lý khách hàng, notifications |
| **Basket Service** | 6004 | ASP.NET Core | Redis | Giỏ hàng, checkout |
| **Order Service** | 6005 | ASP.NET Core | SQL Server | Quản lý đơn hàng, order lifecycle |
| **Inventory API** | 6006 | ASP.NET Core | MongoDB | REST API quản lý tồn kho |
| **Inventory gRPC** | 6007 | ASP.NET Core | MongoDB | gRPC service kiểm tra stock |
| **Hangfire API** | 6008 | ASP.NET Core | MongoDB | Scheduled jobs, background tasks |
| **Chatbot Service** | 6009 | Python/FastAPI | SQLite, Redis | AI Chatbot với Grok/GPT |
| **MCP Server** | 50051, 8001 | Python/FastAPI | - | Tool discovery & execution |
| **Web Status** | 6010 | ASP.NET Core | - | Health monitoring dashboard |

---

## 🔧 Chi tiết từng Service

### 1️⃣ Identity Service

**📍 Port**: 6001  
**🛠️ Technology**: ASP.NET Core Identity, IdentityServer4  
**💾 Database**: SQL Server (`identitydb:1436`)  
**🎯 Purpose**: Xác thực và phân quyền người dùng

**Chức năng chính**:
- ✅ User registration & login
- ✅ JWT token generation (access token + refresh token)
- ✅ Token refresh
- ✅ Password management
- ✅ Role-based authorization
- ✅ Email verification (SMTP integration)

**Database Schema**:
- `Users`: Thông tin user (username, email, password hash)
- `Roles`: Phân quyền (Admin, User, Guest)
- `UserRoles`: Mapping user-role
- `RefreshTokens`: Lưu refresh tokens

**Environment Variables**:
```
ConnectionStrings__IdentitySqlConnection: SQL Server connection
JwtSettings__Key: JWT secret key
JwtSettings__Issuer: Token issuer
JwtSettings__Audience: Token audience
JwtSettings__ExpirationInMinutes: Token expiration (default: 60)
SMTPEmailSetting__*: SMTP configuration
```

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Frontend
- 📤 **Downstream**: Elasticsearch (logging)

---

### 2️⃣ Product Service

**📍 Port**: 6002  
**🛠️ Technology**: ASP.NET Core, EF Core  
**💾 Database**: PostgreSQL (`productdb:5434`)  
**🎯 Purpose**: Quản lý catalog sản phẩm

**Chức năng chính**:
- ✅ CRUD Products
- ✅ Search & Filter sản phẩm (by category, brand, price range, keyword)
- ✅ Categories management (hỗ trợ parent-child)
- ✅ Brands management
- ✅ Product reviews & ratings
- ✅ Wishlist
- ✅ Sellers management
- ✅ Product images

**Database Schema**:
- `Products`: Sản phẩm (id, no, name, slug, price, salePrice, stock, categoryId, brandId, sellerId)
- `Categories`: Danh mục (id, name, slug, parentId)
- `Brands`: Thương hiệu (id, name, slug, logoUrl)
- `ProductImages`: Hình ảnh sản phẩm
- `ProductReviews`: Đánh giá sản phẩm
- `Wishlists`: Danh sách yêu thích
- `Sellers`: Người bán

**Controllers**:
1. `ProductsController`: CRUD products, search
2. `CategoriesController`: CRUD categories
3. `BrandsController`: CRUD brands
4. `ProductReviewsController`: Reviews management
5. `WishlistController`: Wishlist operations
6. `SellersController`: Sellers management

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Frontend, MCP Service
- 🔗 **Peer**: Order Service (verify purchase for reviews)
- 📤 **Downstream**: PostgreSQL

---

### 3️⃣ Customer Service

**📍 Port**: 6003  
**🛠️ Technology**: ASP.NET Core, EF Core  
**💾 Database**: PostgreSQL (`customerdb:5433`)  
**🎯 Purpose**: Quản lý thông tin khách hàng

**Chức năng chính**:
- ✅ Customer profile management (CRUD)
- ✅ Address management
- ✅ Notifications management
- ✅ Hangfire background jobs

**Database Schema**:
- `Customers`: Thông tin khách hàng (username, email, firstName, lastName, phone, address, city, postalCode, country)
- `Notifications`: Thông báo cho user

**Controllers**:
1. `CustomersController`: Profile management
2. `NotificationsController`: Notifications CRUD

**Environment Variables**:
```
DatabaseSettings__ConnectionString: PostgreSQL connection
HangFireSettings__Storage__ConnectionString: Hangfire storage
```

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Frontend, MCP Service
- 📤 **Downstream**: PostgreSQL, Elasticsearch

---

### 4️⃣ Basket Service

**📍 Port**: 6004  
**🛠️ Technology**: ASP.NET Core, Redis, MassTransit  
**💾 Database**: Redis (`basketdb:6379`)  
**🎯 Purpose**: Quản lý giỏ hàng và checkout

**Chức năng chính**:
- ✅ Get/Update/Delete basket (per username)
- ✅ Add/remove items
- ✅ Cart validation (check stock via Inventory gRPC)
- ✅ Checkout → publish event to RabbitMQ
- ✅ Merge guest cart với user cart
- ✅ Get cart item count (for badge display)
- ✅ Track user activities (for AI analytics)

**Data Model** (Redis):
```json
{
  "username": "john_doe",
  "items": [
    {
      "itemNo": "PROD001",
      "itemName": "Laptop",
      "quantity": 1,
      "itemPrice": 20000000,
      "totalPrice": 20000000,
      "imageUrl": "..."
    }
  ],
  "totalPrice": 20000000,
  "totalItems": 1
}
```

**Event Publishing**:
- `BasketCheckoutEvent` → RabbitMQ → Order Service

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Frontend, MCP Service
- 🔗 **Downstream gRPC**: Inventory gRPC (stock validation)
- 🔗 **Downstream MQ**: RabbitMQ (checkout event)
- 📤 **Cache**: Redis

---

### 5️⃣ Order Service

**📍 Port**: 6005  
**🛠️ Technology**: ASP.NET Core, MediatR, MassTransit  
**💾 Database**: SQL Server (`orderdb:1435`)  
**🎯 Purpose**: Quản lý đơn hàng

**Chức năng chính**:
- ✅ Create order (consume BasketCheckoutEvent from RabbitMQ)
- ✅ Get orders by username
- ✅ Get order by ID
- ✅ Update order status (Pending → Processing → Shipped → Delivered)
- ✅ Cancel order
- ✅ Admin: view all orders, filter by status
- ✅ Order statistics (total orders, revenue by status)
- ✅ Check user purchase history (for product reviews)
- ✅ Order reports

**Database Schema**:
- `Orders`: Đơn hàng (id, orderNo, username, totalPrice, firstName, lastName, email, shippingAddress, invoiceAddress, paymentMethod, status, createdDate)
- `OrderItems`: Chi tiết sản phẩm trong đơn (orderId, productNo, productName, quantity, price)

**Order Statuses**:
- `Pending`: Chờ xử lý
- `Processing`: Đang xử lý
- `Shipped`: Đã giao vận
- `Delivered`: Đã giao hàng
- `Cancelled`: Đã hủy

**Controllers**:
1. `OrdersController`: CRUDL orders, status management
2. `OrderReportsController`: Reports & analytics

**Event Consumption**:
- `BasketCheckoutEvent` từ RabbitMQ (Basket Service)

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Frontend, MCP Service
- 🔗 **Event Source**: RabbitMQ (consume BasketCheckoutEvent)
- 📤 **Downstream**: SQL Server, Elasticsearch

---

### 6️⃣ Inventory Service (Dual Mode)

#### A. Inventory Product API (REST)

**📍 Port**: 6006  
**🛠️ Technology**: ASP.NET Core, MongoDB Driver  
**💾 Database**: MongoDB (`inventorydb:27017`)  
**🎯 Purpose**: REST API quản lý tồn kho (CRUD)

**Chức năng**:
- ✅ CRUD inventory entries
- ✅ View stock levels
- ✅ Stock history

#### B. Inventory gRPC Service

**📍 Port**: 6007  
**🛠️ Technology**: ASP.NET Core gRPC  
**💾 Database**: MongoDB (`inventorydb:27017`)  
**🎯 Purpose**: High-performance stock validation

**Chức năng**:
- ✅ Check stock availability (gRPC method)
- ✅ Reserve stock
- ✅ Release stock

**gRPC Methods**:
```protobuf
service InventoryService {
  rpc CheckStock(CheckStockRequest) returns (CheckStockResponse);
  rpc ReserveStock(ReserveStockRequest) returns (ReserveStockResponse);
}
```

**Database Schema** (MongoDB):
```json
{
  "_id": "ObjectId",
  "productNo": "PROD001",
  "quantity": 100,
  "reserved": 5,
  "available": 95,
  "lastUpdated": "ISODate"
}
```

**Giao tiếp**:
- 🔗 **Upstream gRPC**: Basket Service (stock validation)
- 🔗 **Upstream REST**: API Gateway, Admin tools
- 📤 **Downstream**: MongoDB, Elasticsearch

---

### 7️⃣ Hangfire API (Scheduled Jobs)

**📍 Port**: 6008  
**🛠️ Technology**: ASP.NET Core, Hangfire  
**💾 Database**: MongoDB (`hangfiredb:27018`)  
**🎯 Purpose**: Background jobs và scheduled tasks

**Chức năng**:
- ✅ Recurring jobs (daily, weekly, monthly)
- ✅ Fire-and-forget jobs
- ✅ Delayed jobs
- ✅ Continuations
- ✅ Hangfire Dashboard (monitoring)

**Use Cases**:
- 📧 Send email notifications
- 🧹 Clean up expired baskets
- 📊 Generate daily/weekly reports
- 🔄 Sync data with external systems
- ⏰ Reminder emails (abandoned carts)

**Giao tiếp**:
- 🔗 **Upstream**: API Gateway, Admin Dashboard
- 🔗 **Downstream**: MongoDB, Other services (via HTTP/gRPC)

---

### 8️⃣ Chatbot Service (AI)

**📍 Port**: 6009  
**🛠️ Technology**: Python, FastAPI, OpenAI/Grok  
**💾 Database**: SQLite (chat history), Redis (session cache)  
**🎯 Purpose**: AI-powered chatbot với tool execution

**Chức năng chính**:
- ✅ Chat với AI assistant (SSE streaming)
- ✅ Intent recognition & reasoning
- ✅ Tool discovery (via MCP Service)
- ✅ Tool execution (API calls + browser automation)
- ✅ Session management
- ✅ Chat history

**Workflow**:
1. User gửi message
2. AI phân tích ý định
3. Gọi MCP Service để discover tools
4. Execute tools phù hợp
5. Tổng hợp kết quả và trả lời user

**API Endpoints**:
- `POST /api/v1/chat`: Chat endpoint (SSE stream)
- `GET /api/v1/sessions/{session_id}`: Get chat history
- `DELETE /api/v1/chat/{session_id}`: Delete session

**SSE Event Types**:
- `thinking`: AI đang suy nghĩ
- `searching`: Tìm tools
- `searched`: Đã tìm thấy tools
- `executing`: Đang thực thi tool
- `executed`: Đã thực thi
- `content`: Nội dung phản hồi
- `done`: Hoàn thành

**Environment Variables**:
```
XAI_API_KEY: Grok API key
OPENAI_API_KEY: OpenAI API key
MCP_GRPC_URL: MCP gRPC URL (mcp-server:50051)
API_GATEWAY_URL: API Gateway URL
REDIS_HOST: Redis host for session cache
SQLITE_DB_PATH: SQLite database path
```

**Giao tiếp**:
- 🔗 **Upstream**: Frontend (SSE), API Gateway
- 🔗 **Downstream gRPC**: MCP Service
- 📤 **Storage**: SQLite, Redis

---

### 9️⃣ MCP Service (Model Context Protocol)

**📍 Ports**: 50051 (gRPC), 8001 (HTTP/WebSocket)  
**🛠️ Technology**: Python, FastAPI, gRPC, Playwright  
**💾 Database**: None (stateless)  
**🎯 Purpose**: Tool discovery & execution hub

**Chức năng chính**:
- ✅ **Tool Discovery**: Semantic search cho tools phù hợp
- ✅ **Tool Execution**: Thực thi API tools hoặc Browser tools
- ✅ **Embedding Service**: OpenAI text-embedding-3-small
- ✅ **Browser Automation**: Playwright (headless mode)

**Available Tools**:

**API Tools** (10 tools):
1. `search_products`: Tìm sản phẩm
2. `get_product_detail`: Chi tiết sản phẩm
3. `get_categories`: Danh mục
4. `get_brands`: Thương hiệu
5. `get_cart`: Giỏ hàng
6. `update_cart`: Thêm/sửa giỏ
7. `checkout_cart`: Thanh toán
8. `get_user_orders`: Đơn hàng
9. `get_order_detail`: Chi tiết đơn
10. `get_customer`: Thông tin KH

**Browser Tools** (6 tools):
1. `browser_navigate`: Điều hướng URL
2. `browser_click`: Click element
3. `browser_fill`: Nhập text
4. `browser_scroll`: Scroll trang
5. `browser_screenshot`: Chụp màn hình
6. `browser_get_text`: Lấy text

**Architecture**:
```
MCP Service
├── api_server.py (Tool discovery & execution logic)
├── api_tools.py (E-commerce API tool definitions)
├── embeddings.py (OpenAI embedding service)
├── server.py (gRPC server)
├── playwright_server.py (HTTP/WS server)
├── websocket_handler.py (WebSocket handler)
└── browser/
    ├── executor.py (Browser action executor)
    ├── manager.py (Playwright manager)
    └── scanner.py (DOM scanner)
```

**Protocols**:
- **gRPC**: `GetRelevantTools`, `ExecuteTool`
- **WebSocket**: `ws://localhost:8001/ws/mcp`

**Tool Discovery Algorithm**:
1. Nhận query từ Chatbot
2. Create embedding cho query
3. Tính cosine similarity với tool embeddings
4. Return top 5 tools (similarity > 0.5 hoặc top 3)

**Giao tiếp**:
- 🔗 **Upstream gRPC**: Chatbot Service
- 🔗 **Upstream WebSocket**: Frontend (client-side browser control)
- 🔗 **Downstream HTTP**: API Gateway → Microservices
- 🔗 **Downstream Browser**: Playwright → Frontend (localhost:5173)

---

### 🔟 API Gateway (Ocelot)

**📍 Port**: 5000  
**🛠️ Technology**: Ocelot (.NET)  
**🎯 Purpose**: API Gateway, routing, rate limiting

**Routing Configuration**:

| Upstream Path | Downstream Service | Downstream Port |
|---------------|-------------------|-----------------|
| `/api/products/*` | Product Service | 6002 |
| `/api/categories/*` | Product Service | 6002 |
| `/api/brands/*` | Product Service | 6002 |
| `/api/baskets/*` | Basket Service | 6004 |
| `/api/orders/*` | Order Service | 6005 |
| `/api/customers/*` | Customer Service | 6003 |
| `/api/chat/*` | Chatbot Service | 6009 |
| `/token` | Identity Service | 6001 |

**Features**:
- ✅ Request routing
- ✅ Load balancing
- ✅ Rate limiting (100 req/min default, 500 req/min authenticated)
- ✅ Authentication/Authorization
- ✅ CORS handling

**Giao tiếp**:
- 🔗 **Upstream**: Frontend, Mobile Apps, MCP Service
- 🔗 **Downstream**: All microservices

---

### 1️⃣1️⃣ Web Health Status

**📍 Port**: 6010  
**🛠️ Technology**: ASP.NET Core Health Checks UI  
**🎯 Purpose**: Health monitoring dashboard

**Monitored Services**:
- Identity Service (`/hc`)
- Product Service (`/hc`)
- Customer Service (`/hc`)
- Basket Service (`/hc`)
- Order Service (`/hc`)
- Inventory Service (`/hc`)
- Hangfire Service (`/hc`)
- Chatbot Service (`/health`)

**Features**:
- ✅ Real-time health status
- ✅ Historical data
- ✅ Alerts on failures
- ✅ Response time monitoring

---

## 🔄 Giao tiếp giữa các Services

### 1. Synchronous Communication

#### A. HTTP/REST
- Frontend → API Gateway → Microservices
- MCP Service → API Gateway → Microservices
- Service-to-service (minimal, prefer async)

#### B. gRPC
- **Basket Service → Inventory gRPC**: Stock validation
  ```
  CheckStock(productNo, quantity) → { available: bool, stock: int }
  ```
- **Chatbot Service → MCP Service**: Tool discovery & execution
  ```
  GetRelevantTools(query) → { tools: [...] }
  ExecuteTool(tool_name, args) → { result: {...} }
  ```

### 2. Asynchronous Communication (Event-Driven)

#### RabbitMQ Events

**BasketCheckoutEvent**:
- **Publisher**: Basket Service
- **Consumer**: Order Service
- **Payload**:
  ```json
  {
    "username": "john_doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "shippingAddress": "123 Street",
    "invoiceAddress": "123 Street",
    "paymentMethod": "COD",
    "items": [
      {
        "productNo": "PROD001",
        "productName": "Laptop",
        "quantity": 1,
        "price": 20000000
      }
    ]
  }
  ```

**Flow**:
```
User clicks Checkout 
  → Basket Service validates stock (gRPC to Inventory)
  → Publish BasketCheckoutEvent to RabbitMQ
  → Order Service consumes event
  → Create Order in database
  → Delete basket (optional)
  → Send confirmation email (via Hangfire)
```

### 3. WebSocket Communication

**Frontend ↔ MCP Service**:
- Client-side browser control
- Real-time action execution
- Bidirectional messaging

**Messages**:
```json
// Discover tools
{ "type": "discover", "query": "search laptop", "auth_token": "..." }

// Execute tool
{ "type": "execute", "tool_name": "search_products", "arguments": {...} }
```

### 4. Server-Sent Events (SSE)

**Frontend ← Chatbot Service**:
- Streaming chat responses
- Real-time updates

---

## 💾 Database Architecture

### 1. SQL Server (MSSQL)

**Identity Database** (`identitydb:1436`):
- Used by: Identity Service
- Tables: Users, Roles, UserRoles, RefreshTokens
- Connection: `Server=identitydb,1433;Database=IdentityDb;User=sa;Password=***`

**Order Database** (`orderdb:1435`):
- Used by: Order Service
- Tables: Orders, OrderItems
- Connection: `Server=orderdb,1433;Database=OrderDb;User=sa;Password=***`

### 2. PostgreSQL

**Product Database** (`productdb:5434`):
- Used by: Product Service
- Tables: Products, Categories, Brands, ProductImages, ProductReviews, Wishlists, Sellers
- Connection: `server=productdb;port=5432;database=ProductDb;uid=postgres;pwd=***`

**Customer Database** (`customerdb:5433`):
- Used by: Customer Service
- Tables: Customers, Notifications
- Connection: `server=customerdb;port=5432;database=CustomerDb;uid=postgres;pwd=***`

### 3. Redis

**Basket Cache** (`basketdb:6379`):
- Used by: Basket Service, Chatbot Service
- Data structure: Key-value (username → basket JSON)
- TTL: Configurable (e.g., 24 hours for guest carts)

### 4. MongoDB

**Inventory Database** (`inventorydb:27017`):
- Used by: Inventory API, Inventory gRPC
- Collections: Inventory, StockHistory
- Connection: `mongodb://inventorydb:27017`

**Hangfire Database** (`hangfiredb:27018`):
- Used by: Hangfire API
- Collections: Jobs, State, Set, Hash, List
- Connection: `mongodb://admin:password@hangfiredb:27017/hangfire-webapi?authSource=admin`

### 5. SQLite

**Chatbot Database** (`/app/data/chatbot.db`):
- Used by: Chatbot Service
- Tables: Sessions, Messages
- Location: Docker volume `chatbot-data`

### Database per Service Pattern

```mermaid
graph LR
    IDP[Identity Service] --> SQLID[(SQL Server<br/>Identity DB)]
    ORD[Order Service] --> SQLORD[(SQL Server<br/>Order DB)]
    PROD[Product Service] --> PGPROD[(PostgreSQL<br/>Product DB)]
    CUST[Customer Service] --> PGCUST[(PostgreSQL<br/>Customer DB)]
    BASKET[Basket Service] --> REDIS[(Redis<br/>Basket Cache)]
    INV[Inventory Services] --> MONGOINV[(MongoDB<br/>Inventory DB)]
    HF[Hangfire] --> MONGOHF[(MongoDB<br/>Hangfire DB)]
    CHAT[Chatbot] --> SQLITE[(SQLite<br/>Chat DB)]
```

**Benefits**:
- ✅ Database isolation
- ✅ Independent scaling
- ✅ Technology freedom (polyglot persistence)
- ✅ Fault isolation

---

## 🌐 Frontend Integration

### Architecture

```
Frontend (React/Next.js) @ localhost:5173
│
├─ REST API calls → API Gateway @ localhost:5000
│   ├─ Authentication (login, register)
│   ├─ Product listing, search
│   ├─ Shopping cart CRUD
│   ├─ Order management
│   └─ Customer profile
│
├─ SSE Stream → Chatbot Service @ localhost:6009
│   └─ AI Chat (streaming responses)
│
└─ WebSocket → MCP Service @ localhost:8001
    └─ Client-side browser automation
```

### Integration Points

#### 1. API Gateway (Primary Channel)
**Base URL**: `http://localhost:5000`

All business logic APIs go through gateway:
- Authentication: `POST /token`
- Products: `GET /api/products`, `GET /api/products/search`
- Cart: `GET /api/baskets/{username}`, `POST /api/baskets`
- Orders: `GET /api/orders/users/{username}`
- Customer: `GET /api/customers/{username}`

**Authentication**:
```javascript
// Login
const response = await fetch('http://localhost:5000/token', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password, grant_type: 'password' })
});

// Use token
const headers = {
  'Authorization': `Bearer ${token}`
};
```

#### 2. Chatbot Service (SSE Direct)
**URL**: `http://localhost:6009/api/v1/chat`

Direct connection for AI chat:
```javascript
const eventSource = new EventSource('/api/v1/chat', {
  method: 'POST',
  body: JSON.stringify({
    session_id: 'session-123',
    message: 'Tìm laptop',
    user_token: token
  })
});

eventSource.onmessage = (event) => {
  const data = JSON.parse(event.data);
  switch(data.type) {
    case 'thinking': // Show thinking indicator
    case 'content': // Append to chat
    case 'done': // Complete
  }
};
```

#### 3. MCP Service WebSocket (Advanced Feature)
**URL**: `ws://localhost:8001/ws/mcp?userId={userId}`

For client-side browser automation (optional):
```javascript
const ws = new WebSocket('ws://localhost:8001/ws/mcp?userId=user123');

ws.onopen = () => {
  // Request browser action
  ws.send(JSON.stringify({
    type: 'execute',
    tool_name: 'browser_click',
    arguments: { selector: '#add-to-cart' }
  }));
};

ws.onmessage = (event) => {
  const result = JSON.parse(event.data);
  console.log('Action result:', result);
};
```

### Data Flow Examples

#### Example 1: Product Search
```
User types "laptop" in search box
  ↓
Frontend: GET /api/products/search?q=laptop
  ↓
API Gateway: Route to Product Service
  ↓
Product Service: Query PostgreSQL
  ↓
Return products → Frontend displays
```

#### Example 2: Add to Cart
```
User clicks "Add to Cart"
  ↓
Frontend: POST /api/baskets
  ↓
API Gateway: Route to Basket Service
  ↓
Basket Service: Update Redis cache
  ↓
Return updated cart → Frontend updates badge
```

#### Example 3: Checkout
```
User clicks "Checkout"
  ↓
Frontend: POST /api/baskets/checkout
  ↓
API Gateway: Route to Basket Service
  ↓
Basket Service:
  1. Validate stock (gRPC to Inventory)
  2. Publish BasketCheckoutEvent (RabbitMQ)
  3. Delete basket
  ↓
Order Service (consume event):
  1. Create order in SQL Server
  2. Log to Elasticsearch
  ↓
Return success → Frontend shows order confirmation
```

#### Example 4: AI Chat
```
User asks "Tìm laptop giá dưới 20 triệu"
  ↓
Frontend: POST /api/v1/chat (SSE)
  ↓
Chatbot Service:
  1. Analyze intent
  2. Call MCP gRPC: GetRelevantTools("search laptop")
  3. MCP returns: [search_products tool]
  4. Execute: search_products(query="laptop", max_price=20000000)
  5. MCP calls: API Gateway /api/products/search
  6. Get products
  ↓
Stream response to Frontend:
  - thinking: "Đang tìm kiếm..."
  - searching: "search products"
  - executing: "search_products"
  - content: "Tôi tìm thấy 5 laptop..."
  - done
```

---

## 🏗️ Infrastructure Components

### 1. RabbitMQ (Message Broker)

**Port**: 5672 (AMQP), 15672 (Management UI)  
**Image**: `rabbitmq:3-management-alpine`  
**Purpose**: Asynchronous event-driven communication

**Exchanges & Queues**:
- `basket-checkout-queue`: BasketCheckoutEvent

**Management UI**: `http://localhost:15672`  
Credentials: `guest/guest`

---

### 2. Elasticsearch (Logging & Search)

**Port**: 9200 (HTTP), 9300 (Transport)  
**Image**: `docker.elastic.co/elasticsearch/elasticsearch:7.17.2`  
**Purpose**: Centralized logging, full-text search

**Indexed Logs**:
- Identity Service logs
- Order Service logs
- Customer Service logs
- Inventory Service logs
- Hangfire Service logs

**Configuration**:
```
Single-node mode
Security: enabled (elastic/admin)
JVM Heap: 512MB
```

---

### 3. Kibana (Log Visualization)

**Port**: 5601  
**Image**: `docker.elastic.co/kibana/kibana:7.17.2`  
**Purpose**: Elasticsearch dashboard

**URL**: `http://localhost:5601`  
**Login**: `elastic/admin`

**Features**:
- Search logs by service
- Create dashboards
- Set up alerts

---

### 4. Portainer (Container Management)

**Port**: 9000 (UI), 8080 (Edge Agent)  
**Image**: `portainer/portainer-ce`  
**Purpose**: Docker container management UI

**URL**: `http://localhost:9000`

**Features**:
- View all containers
- Start/stop/restart services
- View logs
- Resource monitoring

---

## 🚀 Deployment & Networking

### Docker Network

All services run in a single Docker network: `microservices` (bridge driver)

**Benefits**:
- Services can communicate by service name (e.g., `http://product-api`)
- Isolated from host network
- Easy service discovery

### Port Mapping

| Service | Internal Port | External Port | Public Access |
|---------|--------------|---------------|---------------|
| API Gateway | 80 | 5000 | ✅ Yes |
| Identity | 80 | 6001 | ✅ Yes |
| Product | 80 | 6002 | ✅ Yes |
| Customer | 80 | 6003 | ✅ Yes |
| Basket | 80 | 6004 | ✅ Yes |
| Order | 80 | 6005 | ✅ Yes |
| Inventory API | 80 | 6006 | ✅ Yes |
| Inventory gRPC | 80 | 6007 | 🔒 No (internal) |
| Hangfire | 80 | 6008 | ✅ Yes (admin only) |
| Chatbot | 80 | 6009 | ✅ Yes |
| MCP HTTP/WS | 8001 | 8001 | ✅ Yes |
| MCP gRPC | 50051 | 50051 | 🔒 No (internal) |
| Web Status | 80 | 6010 | ✅ Yes |

### Volumes

Persistent data stored in named volumes:
- `order_data`: Order database
- `identity_data`: Identity database
- `product_data`: Product database
- `customer_data`: Customer database
- `inventory_data`: Inventory database
- `hangfire_mongo_data`: Hangfire database
- `elasticsearch_data`: Elasticsearch indices
- `portainer_data`: Portainer config
- `chatbot-data`: Chatbot SQLite database

---

## 🔐 Security

### Authentication Flow

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant AGW as API Gateway
    participant IDP as Identity Service
    participant SVC as Microservice

    FE->>AGW: POST /token (username, password)
    AGW->>IDP: Forward request
    IDP->>IDP: Validate credentials
    IDP->>AGW: Return JWT tokens
    AGW->>FE: Return tokens
    
    FE->>AGW: GET /api/orders (Bearer token)
    AGW->>AGW: Validate JWT
    AGW->>SVC: Forward with token
    SVC->>SVC: Validate token & authorize
    SVC->>AGW: Return data
    AGW->>FE: Return data
```

### Authorization

- **Public endpoints**: Products, Categories, Brands (no auth required)
- **User endpoints**: Basket, Orders, Customer (JWT required)
- **Admin endpoints**: Product CRUD, Order management (JWT + Admin role)

---

## 📊 Monitoring & Observability

### Health Checks

All services expose `/hc` or `/health` endpoints:
- Identity: `http://identity-api/hc`
- Product: `http://product-api/hc`
- Basket: `http://basket-api/hc`
- Order: `http://order-api/hc`
- Customer: `http://customer-api/hc`
- Inventory: `http://inventory-product-api/hc`
- Hangfire: `http://hangfire-api/hc`
- Chatbot: `http://chatbot-api/health`

### Logging

**Elasticsearch Indexes**:
- `identity-logs-*`
- `order-logs-*`
- `customer-logs-*`
- `inventory-logs-*`
- `hangfire-logs-*`

**View in Kibana**: `http://localhost:5601`

---

## 📈 Scalability Considerations

### Stateless Services (Can Scale Horizontally)
- ✅ Product Service
- ✅ Order Service
- ✅ Customer Service
- ✅ Inventory API
- ✅ Chatbot Service
- ✅ MCP Service

### Stateful Services (Require Coordination)
- ⚠️ Basket Service (uses Redis, can scale with Redis Cluster)
- ⚠️ Hangfire (uses MongoDB locks for job coordination)

### Databases (Vertical Scaling Preferred)
- SQL Server: Master-slave replication
- PostgreSQL: Read replicas
- Redis: Redis Cluster for horizontal scaling
- MongoDB: Replica sets

---

## 🎯 Key Architectural Patterns

1. **Database per Service**: Each microservice owns its database
2. **API Gateway**: Single entry point for clients
3. **Event-Driven Architecture**: Async communication via RabbitMQ
4. **CQRS (partial)**: Order Service uses MediatR for command/query separation
5. **Circuit Breaker**: (Can add Polly for resilience)
6. **Service Discovery**: Docker DNS within bridge network
7. **Centralized Logging**: Elasticsearch + Kibana
8. **Health Monitoring**: Health Checks UI

---

## 📚 Tham khảo

- [API Documentation](./API_DOCUMENTATION.md)
- [Use Cases Documentation](./USE_CASES.md)
- [AI Workflow](./AI_WORKFLOW.md)
- [Docker Compose Configuration](./docker-compose.yml)
