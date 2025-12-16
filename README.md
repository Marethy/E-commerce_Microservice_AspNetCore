# 🛍️ E-Commerce Microservices Platform

> Modern, scalable e-commerce platform built with microservices architecture and AI-powered chatbot assistant

[![.NET](https://img.shields.io/badge/.NET-7.0-512BD4)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.12-3776AB)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.100+-009688)](https://fastapi.tiangolo.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Key Features](#key-features)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [Services](#services)
- [AI Chatbot](#ai-chatbot)
- [Documentation](#documentation)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

This is a production-ready **E-commerce platform** built using **microservices architecture** with the following highlights:

- ✅ **9 Independent Microservices** - Product, Basket, Order, Customer, Identity, Chatbot, MCP, Inventory, ScheduledJob
- ✅ **AI-Powered Shopping Assistant** - Grok-3-mini with reasoning capabilities
- ✅ **Model Context Protocol (MCP)** - Dynamic tool discovery and execution
- ✅ **Event-Driven Architecture** - RabbitMQ for async communication
- ✅ **API Gateway** - Ocelot for unified API access
- ✅ **Saga Orchestrator** - Distributed transaction management
- ✅ **Real-time Communication** - gRPC + WebSocket
- ✅ **Containerized** - Docker & Docker Compose ready

---

## 🏗️ Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND                                 │
│                    (React/Vue/Angular)                          │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ HTTPS
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API GATEWAY (Ocelot)                       │
│                        Port: 5000                                │
└─────┬────────┬─────────┬─────────┬──────────┬────────┬─────────┘
      │        │         │         │          │        │
      │        │         │         │          │        │
      ▼        ▼         ▼         ▼          ▼        ▼
┌──────────┐ ┌────────┐ ┌───────┐ ┌────────┐ ┌──────┐ ┌────────┐
│ Product  │ │ Basket │ │ Order │ │Customer│ │ AI   │ │Identity│
│ Service  │ │Service │ │Service│ │Service │ │Chatbot│ │Service│
│          │ │        │ │       │ │        │ │      │ │        │
│ :5004    │ │ :5003  │ │ :5002 │ │ :5001  │ │ :80  │ │        │
└────┬─────┘ └───┬────┘ └───┬───┘ └────┬───┘ └──┬───┘ └────────┘
     │           │           │          │        │
     │           │           │          │        │ gRPC
     │           │           │          │        ▼
     │           │           │          │   ┌─────────┐
     │           │           │          │   │   MCP   │
     │           │           │          │   │ Service │
     │           │           │          │   │ :8000   │
     │           │           │          │   └─────────┘
     │           │           │          │
     └───────────┴───────────┴──────────┴────────┐
                                                  │
                         ▼                        ▼
              ┌──────────────────┐    ┌──────────────────┐
              │   PostgreSQL     │    │    RabbitMQ      │
              │   Databases      │    │  Message Queue   │
              └──────────────────┘    └──────────────────┘
```

### Microservices Details

| Service | Technology | Database | Port | Description |
|---------|-----------|----------|------|-------------|
| **Product** | ASP.NET Core | PostgreSQL | 5004 | Product catalog, categories, brands |
| **Basket** | ASP.NET Core | Redis | 5003 | Shopping cart management |
| **Order** | ASP.NET Core | SQL Server | 5002 | Order processing & tracking |
| **Customer** | ASP.NET Core | PostgreSQL | 5001 | Customer profiles |
| **Identity** | ASP.NET Identity | SQL Server | - | Authentication & authorization |
| **Inventory** | ASP.NET Core | PostgreSQL | 5005 | Stock management |
| **Chatbot** | Python/FastAPI | PostgreSQL | 80 | AI shopping assistant |
| **MCP** | Python/FastAPI+gRPC | - | 8000/8001 | Tool orchestration |
| **ScheduledJob** | ASP.NET Core + Hangfire | SQL Server | - | Background jobs |

---

## ✨ Key Features

### 🛒 E-Commerce Core
- **Product Management**
  - Advanced search & filtering
  - Categories & brands
  - Product reviews & ratings
  - Wishlist functionality
  - SEO-friendly URLs (slugs)

- **Shopping Cart**
  - Real-time cart updates
  - Guest cart support
  - Cart merge after login
  - Stock validation before checkout

- **Order Management**
  - Order creation & tracking
  - Multiple order statuses
  - Order history
  - Cancel orders
  - Admin statistics

- **Customer Management**
  - User profiles
  - Address management
  - Order history

### 🤖 AI-Powered Features

- **Intelligent Chatbot**
  - Natural language understanding
  - Product recommendations
  - Order tracking assistance
  - Shopping assistant
  - Multi-turn conversations
  
- **Advanced AI Capabilities**
  - **Reasoning Mode**: AI thinks before responding
  - **Tool Discovery**: Automatically finds required tools
  - **Tool Execution**: Performs actions on behalf of users
  - **Context Awareness**: Remembers conversation history
  - **Real-time Streaming**: See AI think and respond in real-time

### 🔧 Technical Features

- **Microservices Architecture**
  - Independent scaling
  - Technology diversity
  - Fault isolation
  - Easy maintenance

- **Event-Driven Communication**
  - Async messaging with RabbitMQ
  - Event sourcing ready
  - Saga pattern for distributed transactions

- **API Gateway**
  - Unified entry point
  - Request routing
  - Rate limiting
  - Authentication

- **Security**
  - JWT authentication
  - Role-based authorization
  - API key support
  - HTTPS ready

---

## 🛠️ Technologies

### Backend Services (.NET)
- **ASP.NET Core 7.0** - Web APIs
- **Entity Framework Core** - ORM
- **MediatR** - CQRS pattern
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Ocelot** - API Gateway
- **MassTransit** - Message bus abstraction
- **Hangfire** - Background job processing

### AI Services (Python)
- **FastAPI** - Modern web framework
- **OpenAI SDK** - AI model integration (Grok-3-mini via x.ai)
- **gRPC** - High-performance RPC
- **Playwright** - Browser automation
- **httpx** - Async HTTP client
- **SQLAlchemy** - Database ORM

### Databases
- **PostgreSQL** - Product, Customer, Inventory
- **SQL Server** - Order, Identity
- **Redis** - Basket cache

### Message Queue
- **RabbitMQ** - Async messaging

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD (optional)

---

## 🚀 Getting Started

### Prerequisites

- **.NET 7.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Python 3.12+** - [Download](https://www.python.org/downloads/)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 15+** - [Download](https://www.postgresql.org/download/)
- **SQL Server** - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Redis** - [Download](https://redis.io/download)
- **RabbitMQ** - [Download](https://www.rabbitmq.com/download.html)

### Quick Start with Docker Compose

```bash
# Clone repository
git clone https://github.com/your-repo/ecommerce-microservices.git
cd ecommerce-microservices

# Start all services
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f chatbot
```

### Manual Setup

#### 1. Setup Databases

```bash
# PostgreSQL
createdb ecommerce_products
createdb ecommerce_customers
createdb ecommerce_inventory
createdb ecommerce_chatbot

# SQL Server (via Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123" \
   -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Redis
docker run -d -p 6379:6379 redis:alpine
```

#### 2. Setup RabbitMQ

```bash
docker run -d --hostname rabbitmq --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

#### 3. Run Backend Services

```bash
cd backend_microservices/src

# Product Service
cd Services/Product/Product.API
dotnet restore
dotnet run

# Basket Service
cd Services/Basket/Basket.API
dotnet restore
dotnet run

# Order Service
cd Services/Ordering/Ordering.API
dotnet restore
dotnet run

# Customer Service
cd Services/Customer/Customer.API
dotnet restore
dotnet run

# API Gateway
cd ApiGateways/OcelotApiGw
dotnet restore
dotnet run
```

#### 4. Run AI Services

```bash
# MCP Service
cd backend_microservices/src/Services/MCP
pip install -r requirements.txt
python server.py

# Chatbot Service
cd backend_microservices/src/Services/Chatbot
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 80
```

#### 5. Environment Variables

**Chatbot Service** (`.env`):
```env
XAI_API_KEY=your_grok_api_key_here
PRODUCT_API_URL=http://localhost:5004
BASKET_API_URL=http://localhost:5003
ORDER_API_URL=http://localhost:5002
CUSTOMER_API_URL=http://localhost:5001
MCP_GRPC_URL=localhost:8000
DATABASE_URL=postgresql://user:password@localhost/ecommerce_chatbot
LOG_LEVEL=INFO
```

**MCP Service** (`.env`):
```env
GRPC_PORT=8000
HTTP_PORT=8001
LOG_LEVEL=INFO
API_GATEWAY_URL=http://localhost:5000
PRODUCT_API_URL=http://localhost:5004
BASKET_API_URL=http://localhost:5003
ORDER_API_URL=http://localhost:5002
```

---

## 📦 Services

### Product Service

**Endpoints:**
- `GET /api/Products` - List products
- `GET /api/Products/{id}` - Get product details
- `GET /api/Products/search` - Search products
- `POST /api/Products` - Create product (Admin)
- `PUT /api/Products/{id}` - Update product (Admin)
- `DELETE /api/Products/{id}` - Delete product (Admin)

**Features:**
- Product catalog
- Categories & brands
- Image management
- SEO-friendly URLs
- Product reviews

### Basket Service

**Endpoints:**
- `GET /api/Baskets/{username}` - Get cart
- `POST /api/Baskets` - Update cart
- `DELETE /api/Baskets/{username}` - Clear cart
- `POST /api/Baskets/checkout` - Checkout
- `POST /api/Baskets/validate` - Validate stock

**Features:**
- Shopping cart
- Guest cart support
- Cart merge after login
- Stock validation
- Auto-save

### Order Service

**Endpoints:**
- `GET /api/v1/orders/users/{username}` - Get user orders
- `GET /api/v1/orders/{id}` - Get order details
- `POST /api/v1/orders` - Create order
- `PATCH /api/v1/orders/{id}/status` - Update status (Admin)
- `POST /api/v1/orders/{id}/cancel` - Cancel order

**Features:**
- Order processing
- Order tracking
- Status updates
- Order history
- Statistics (Admin)

### Customer Service

**Endpoints:**
- `GET /api/Customers/{username}` - Get customer info
- `PUT /api/Customers/{username}` - Update customer info

**Features:**
- Customer profiles
- Address management
- Contact information

---

## 🤖 AI Chatbot

### Overview

The AI Chatbot is powered by **Grok-3-mini** and uses **Model Context Protocol (MCP)** for dynamic tool discovery and execution.

### Architecture

```
User Message
     ↓
Chatbot Service (FastAPI)
     ↓
AI Agent (Grok-3-mini)
     ↓
┌─────────────────┐
│  1. Reasoning   │ → Think about user request
└─────────────────┘
     ↓
┌─────────────────┐
│ 2. Tool Search  │ → [[SEARCH: find products]]
└─────────────────┘
     ↓
MCP Service
     ↓
┌─────────────────────────┐
│ Tool Discovery          │
│ - API Tools (E-commerce)│
│ - Browser Tools         │
└─────────────────────────┘
     ↓
Return: [search_products, get_cart, ...]
     ↓
┌─────────────────┐
│ 3. Tool Exec    │ → Execute search_products(...)
└─────────────────┘
     ↓
Product Service API
     ↓
Return: Product list
     ↓
┌─────────────────┐
│ 4. Response     │ → "I found 10 laptops..."
└─────────────────┘
     ↓
User receives answer
```

### Capabilities

1. **Product Search**
   - Natural language search
   - Filter by price, category, brand
   - Product recommendations

2. **Shopping Assistant**
   - Add to cart
   - Check cart contents
   - Checkout assistance

3. **Order Tracking**
   - View order status
   - Track delivery
   - Order history

4. **Personalized Recommendations**
   - Based on user history
   - Context-aware suggestions

### Example Conversations

**Example 1: Product Search**
```
User: "Tìm laptop giá dưới 20 triệu"
AI: [Thinking] User needs laptop under 20M
    [Searching] Tools for product search
    [Executing] search_products(query="laptop", max_price=20000000)
    [Response] "Tôi tìm thấy 15 laptop phù hợp..."
```

**Example 2: Add to Cart**
```
User: "Thêm Dell XPS 13 vào giỏ"
AI: [Searching] Product search + cart tools
    [Executing] search_products(query="Dell XPS 13")
    [Executing] update_cart(items=[...])
    [Response] "Đã thêm Dell XPS 13 vào giỏ hàng!"
```

**Example 3: Order Tracking**
```
User: "Đơn hàng của tôi đến đâu rồi?"
AI: [Executing] get_user_orders(username="john_doe")
    [Executing] get_order_detail(order_id=123)
    [Response] "Đơn hàng ORD-123 đang giao, dự kiến 28/12"
```

---

## 📚 Documentation

Detailed documentation available:

- **[API Documentation](./API_DOCUMENTATION.md)** - Complete API reference for all services
- **[AI Workflow](./AI_WORKFLOW.md)** - In-depth explanation of AI chatbot workflow and MCP

### API Documentation Includes:
- All REST endpoints
- Request/response formats
- Authentication
- Error handling
- Examples

### AI Workflow Documentation Includes:
- System architecture
- Reasoning process
- Tool discovery mechanism
- Tool execution flow
- Real-world examples
- Debugging guide

---

## 💻 Development

### Project Structure

```
.
├── backend_microservices/
│   ├── src/
│   │   ├── ApiGateways/
│   │   │   └── OcelotApiGw/           # API Gateway
│   │   ├── BuildingBlocks/            # Shared libraries
│   │   │   ├── Contracts/
│   │   │   ├── EventBus/
│   │   │   ├── Infrastructure/
│   │   │   └── Shared/
│   │   ├── Services/
│   │   │   ├── Basket/
│   │   │   │   └── Basket.API/
│   │   │   ├── Chatbot/               # AI Chatbot (Python)
│   │   │   │   ├── main.py
│   │   │   │   ├── agent.py
│   │   │   │   ├── routers/
│   │   │   │   └── database/
│   │   │   ├── Customer/
│   │   │   │   └── Customer.API/
│   │   │   ├── Identity/
│   │   │   │   └── Presentation/
│   │   │   ├── Inventory/
│   │   │   │   └── Inventory.Product.API/
│   │   │   ├── MCP/                   # MCP Service (Python)
│   │   │   │   ├── server.py
│   │   │   │   ├── api_tools.py
│   │   │   │   ├── playwright_server.py
│   │   │   │   └── protos/
│   │   │   ├── Ordering/
│   │   │   │   ├── Ordering.API/
│   │   │   │   ├── Ordering.Application/
│   │   │   │   └── Ordering.Domain/
│   │   │   ├── Product/
│   │   │   │   └── Product.API/
│   │   │   └── ScheduledJob/
│   │   │       └── Hangfire.API/
│   │   ├── Saga.Orchestrator/         # Saga pattern
│   │   └── WebApps/
│   │       └── WebHealthStatus/
│   ├── API_DOCUMENTATION.md
│   ├── AI_WORKFLOW.md
│   └── README.md (this file)
├── frontend/                          # Frontend application
└── docker-compose.yml
```

### Coding Standards

**C# (.NET Services):**
- Follow Microsoft C# coding conventions
- Use async/await for I/O operations
- Implement CQRS with MediatR
- Use AutoMapper for object mapping
- Validate inputs with FluentValidation

**Python (AI Services):**
- Follow PEP 8 style guide
- Use type hints
- Async/await with asyncio
- Structured logging
- Error handling with try/except

### Database Migrations

**Entity Framework Core:**
```bash
cd Services/Product/Product.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Testing

```bash
# .NET tests
dotnet test

# Python tests
pytest
```

---

## 🚢 Deployment

### Docker Deployment

**Build all services:**
```bash
docker-compose build
```

**Deploy:**
```bash
docker-compose up -d
```

**Scale services:**
```bash
docker-compose up -d --scale product-service=3
```

### Kubernetes (Coming Soon)

Kubernetes manifests for production deployment will be added.

### Environment Configuration

**Production checklist:**
- [ ] Update connection strings
- [ ] Configure JWT secrets
- [ ] Set up SSL certificates
- [ ] Configure CORS origins
- [ ] Set up monitoring (e.g., Prometheus)
- [ ] Configure logging aggregation
- [ ] Set up API rate limiting
- [ ] Configure backups

---

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Contribution Guidelines

- Write clear commit messages
- Add tests for new features
- Update documentation
- Follow coding standards
- Ensure all tests pass

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Grok-3-mini** by x.ai for powerful AI reasoning
- **OpenAI** for SDK compatibility
- **Microsoft** for .NET ecosystem
- **FastAPI** community
- **Ocelot** for API Gateway
- **MassTransit** for messaging abstraction

---

## 📞 Support

For questions or issues:

- **Documentation**: See [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) and [AI_WORKFLOW.md](./AI_WORKFLOW.md)
- **Issues**: Open an issue on GitHub
- **Email**: support@example.com

---

## 🗺️ Roadmap

- [x] Core microservices
- [x] AI Chatbot with MCP
- [x] API Gateway
- [x] Event-driven architecture
- [ ] Kubernetes deployment
- [ ] Admin dashboard
- [ ] Mobile app
- [ ] Analytics & reporting
- [ ] Recommendation engine
- [ ] Multi-language support
- [ ] Payment integration
- [ ] Shipping integration

---

## 📊 Statistics

- **Microservices**: 9
- **Technologies**: 15+
- **APIs**: 50+
- **AI Tools**: 10+
- **Lines of Code**: 20,000+

---

<div align="center">

**Built with ❤️ using .NET, Python, and AI**

[⭐ Star this repo](https://github.com/your-repo) | [📖 Documentation](./API_DOCUMENTATION.md) | [🤖 AI Workflow](./AI_WORKFLOW.md)

</div>
