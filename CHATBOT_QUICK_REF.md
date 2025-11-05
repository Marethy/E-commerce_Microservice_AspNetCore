# 🎯 Chatbot Service - Quick Reference

## 🚀 Start Service
```bash
# Start chatbot only
docker-compose up -d chatbot-api

# Start all services
docker-compose up -d

# View logs
docker logs -f chatbot-api
```

## 🔍 Health Checks
```bash
# Direct check
curl http://localhost:6009/health

# Via Gateway
curl http://localhost:6000/api/chat

# Web UI
http://localhost:6010
```

## 💬 Test Chat
```bash
curl -X POST http://localhost:6009/api/v1/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Tìm laptop dưới 20 triệu",
    "session_id": "test-session-123"
  }'
```

## 🔧 Common Commands
```bash
# Rebuild image
docker-compose build chatbot-api

# Restart service
docker-compose restart chatbot-api

# Stop service
docker-compose stop chatbot-api

# Remove and rebuild
docker-compose down chatbot-api
docker-compose up -d chatbot-api

# Check running containers
docker ps | grep chatbot
```

## 📊 Ports
- **6009** - Chatbot API (direct)
- **6000** - API Gateway (recommended)
- **6010** - Health Status UI

## 📁 Important Files
- **Config**: `src/Services/Chatbot/config.py`
- **Main**: `src/Services/Chatbot/main.py`
- **Dockerfile**: `src/Services/Chatbot/Dockerfile`
- **Dependencies**: `src/Services/Chatbot/requirements.txt`
- **Gateway Routes**: `src/ApiGateways/OcelotApiGw/ocelot.Local.json`

## 🔑 Environment Variables
Required:
- `GROK_KEY` - Grok API key

Optional (have defaults):
- `PRODUCT_API_URL`
- `BASKET_API_URL`
- `ORDER_API_URL`
- `LOG_LEVEL`

## 🐛 Troubleshooting
```bash
# Check logs
docker logs chatbot-api --tail 50

# Check all services
docker-compose ps

# Verify network
docker network inspect microservices

# Test connectivity
docker exec chatbot-api ping product-api
```

## 📖 Docs
- **Integration Guide**: `src/Services/Chatbot/INTEGRATION_GUIDE.md`
- **Summary**: `CHATBOT_INTEGRATION_SUMMARY.md`
- **API Docs**: http://localhost:6009/docs
