# 🤖 Chatbot Service Integration Guide

## 📋 Overview
The Chatbot service is a **Python FastAPI** application that uses **LangGraph + Grok AI** to provide intelligent conversational assistance for the e-commerce platform.

**Technology Stack:**
- Python 3.11
- FastAPI + Uvicorn
- LangChain + LangGraph
- Grok AI (xAI)
- Docker

**Port:** 6009 (Docker), 8000 (Local)

---

## 🚀 Quick Start

### 1️⃣ Prerequisites
- Docker & Docker Compose installed
- Grok API key from [xAI](https://x.ai)
- Backend microservices running

### 2️⃣ Configuration

Create `.env` file in Chatbot directory:
```bash
cd src/Services/Chatbot
cp .env.example .env
```

Edit `.env` and add your Grok API key:
```env
GROK_KEY=your-actual-grok-key-here
```

### 3️⃣ Run with Docker Compose

From repository root:
```bash
docker-compose up -d chatbot-api
```

Check logs:
```bash
docker logs -f chatbot-api
```

### 4️⃣ Verify Service

Health check:
```bash
curl http://localhost:6009/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "E-commerce Chatbot Service",
  "version": "1.0.0"
}
```

API docs:
```
http://localhost:6009/docs
```

---

## 🔗 API Endpoints

### Via API Gateway (Recommended)
- **POST** `http://localhost:6000/api/chat` - Send chat message
- **DELETE** `http://localhost:6000/api/chat/{sessionId}` - Clear session

### Direct Access
- **POST** `http://localhost:6009/api/v1/chat` - Send chat message
- **DELETE** `http://localhost:6009/api/v1/chat/{sessionId}` - Clear session
- **GET** `http://localhost:6009/health` - Health check
- **GET** `http://localhost:6009/docs` - API documentation

---

## 📤 Request Format

```json
{
  "message": "Tìm laptop dưới 20 triệu",
  "session_id": "user-123-session",
  "user_token": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Parameters:**
- `message` (required): User's message
- `session_id` (required): Unique session identifier
- `user_token` (optional): JWT token for authenticated actions (add to cart, etc.)

---

## 📥 Response Format

**Streaming Response:**
```
data: {"chunk": "{\"reply\":\"Tôi tìm thấy 5 laptop...\",\"suggested_url\":\"/products\"}"}
data: {"done": true}
```

**Parsed Response:**
```json
{
  "reply": "Tôi tìm thấy 5 laptop dưới 20 triệu...",
  "suggested_url": "/products?maxPrice=20000000"
}
```

---

## 🧪 Testing

### Test Chat Endpoint
```bash
curl -X POST http://localhost:6009/api/v1/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Xin chào",
    "session_id": "test-123"
  }'
```

### Clear Session
```bash
curl -X DELETE http://localhost:6009/api/v1/chat/test-123
```

---

## 🏗️ Architecture

**LangGraph Workflow:**
```
User Message → Analyze Intent → Route Decision
                                     ↓
                    ┌────────────────┼────────────────┐
                    ↓                ↓                ↓
              Search Products   Call Tool      Finish Response
                    ↓                ↓
              Present Results  Update Context
                    ↓                ↓
              Navigate UI ─────────→ Finish
```

**Service Integration:**
- **Product.API** - Search products, get details, brands, categories
- **Basket.API** - Add/remove cart items, checkout
- **Ordering.API** - View orders, order details, create orders
- **RabbitMQ** - Event-driven communication (future)

---

## 🔧 Configuration Options

**Environment Variables:**

| Variable | Description | Default |
|----------|-------------|---------|
| `GROK_KEY` | Grok API key | *required* |
| `GROK_MODEL` | Grok model name | `grok-beta` |
| `PRODUCT_API_URL` | Product service URL | `http://product-api` |
| `BASKET_API_URL` | Basket service URL | `http://basket-api` |
| `ORDER_API_URL` | Order service URL | `http://order-api` |
| `REDIS_HOST` | Redis host | `basketdb` |
| `REDIS_PORT` | Redis port | `6379` |
| `REDIS_DB` | Redis database | `1` |
| `LOG_LEVEL` | Logging level | `INFO` |

---

## 🐛 Troubleshooting

### Service not starting
Check logs:
```bash
docker logs chatbot-api
```

Common issues:
- Missing `GROK_KEY` environment variable
- Network connectivity to other services
- Port 6009 already in use

### Can't connect to backend services
Verify services are running:
```bash
docker ps | grep -E "product-api|basket-api|order-api"
```

Check network:
```bash
docker network inspect microservices
```

### Slow responses
- Check Grok API status
- Increase timeout in `config.py`
- Check backend service performance

---

## 📊 Monitoring

**Health Check via WebStatus:**
```
http://localhost:6010
```

**Logs:**
```bash
# Real-time logs
docker logs -f chatbot-api

# Last 100 lines
docker logs --tail 100 chatbot-api
```

**Metrics:**
- Response time: Typically 2-5 seconds
- Rate limit: 60 requests/minute via API Gateway
- Timeout: 30 seconds

---

## 🔐 Security

**Authentication:**
- Public endpoints: `/health`, `/docs`, `/`
- Protected actions: Add to cart, create order (requires JWT token)

**Rate Limiting:**
- Gateway: 60 requests/minute per client
- QoS: Circuit breaker after 3 failures

**CORS:**
- Currently allows all origins (development mode)
- Configure for production in `main.py`

---

## 🚧 Future Enhancements

- [ ] Redis session storage (replace in-memory)
- [ ] RabbitMQ event publishing
- [ ] Elasticsearch logging integration
- [ ] Response caching
- [ ] Multi-language support
- [ ] Voice input/output
- [ ] Product recommendations
- [ ] Order tracking integration

---

## 📞 Support

**Issues?**
- Check logs: `docker logs chatbot-api`
- Review configuration: `src/Services/Chatbot/config.py`
- Test dependencies: `docker-compose ps`

**Need Help?**
- API Docs: http://localhost:6009/docs
- Health Status: http://localhost:6010
- Repository: [E-commerce_Microservice_AspNetCore]
