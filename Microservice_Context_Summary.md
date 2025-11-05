# 📘 Microservice Context Summary — E-Commerce Platform

**Author:** Trần Thiện Khiêm  
**Project:** AI-Driven E-Commerce Microservice System  
**Tech Stack:** .NET 8 | RabbitMQ | Ocelot | Docker | EF Core 8 | MongoDB | Redis | PostgreSQL | SQL Server  

---

## 🧩 1. System Overview
The system is a **distributed e-commerce platform** built with **8 independent microservices**.  
Each service owns its own **database, business logic, and API**.  
They communicate via **REST/gRPC** and **RabbitMQ (MassTransit)** in an **event-driven architecture**.  
The system follows **Clean Architecture + CQRS + DDD principles**, containerized with Docker and exposed via **Ocelot API Gateway**.

---

## 🏛️ 2. Service Contexts

| Service | Purpose | Database | Key Technology |
|----------|----------|-----------|----------------|
| 🛒 **Product.API** | Manage product catalog (CRUD, categories, pricing, images). | MySQL | EF Core 8, AutoMapper |
| 👤 **Customer.API** | Manage customer info, addresses, loyalty. | PostgreSQL | EF Core 8 |
| 🧺 **Basket.API** | Handle shopping cart sessions, cache items before checkout. | Redis | Redis Stack |
| 📦 **Ordering.API** | Core domain: place orders, process status flow, emit domain events. | SQL Server | Clean Architecture + CQRS + MediatR |
| 🏬 **Inventory.Product.API** | Track product stock, adjust quantities when orders confirmed. | MongoDB | Repository Pattern |
| 🔗 **Inventory.Grpc** | Provide gRPC endpoints for real-time stock queries. | MongoDB | gRPC 2.57 |
| 🔁 **Saga.Orchestrator** | Manage distributed transactions across services (Order–Payment–Inventory). | — | Saga Pattern + MassTransit |
| 🚪 **OcelotApiGw** | Central API Gateway: routing, JWT auth, service discovery. | — | Ocelot 24.0 |

---

## ⚙️ 3. Infrastructure Components

| Component | Description |
|------------|-------------|
| 🧱 **BuildingBlocks** | Shared libraries (contracts, abstractions, common utilities). |
| 📨 **Event Bus** | MassTransit + RabbitMQ handling domain/integration events. |
| 🪵 **Common.Logging** | Centralized logging via Serilog → Elasticsearch → Kibana. |
| ❤️ **Health Checks** | Each service exposes `/health` endpoint, monitored via UI. |
| ⏱️ **Hangfire Jobs** | Background jobs for async operations (cleanup, retries). |

---

## 🔗 4. Communication & Flow

### 🔄 Event-Driven Integration
- **Publisher:** `Ordering.API` emits `OrderCreatedEvent`
- **Subscribers:**  
  - `Inventory.Product.API` → reduce stock  
  - `Payment.API` → process payment  
  - `Notification.Service` → send confirmation

All events handled asynchronously through **RabbitMQ exchanges** managed by MassTransit.

### 📡 gRPC Inter-Service
- `Ordering.API` ↔ `Inventory.Grpc` → stock availability queries.
- gRPC chosen for **low-latency, strongly-typed** communication.

---

## 🧱 5. Cross-Cutting Concerns

| Aspect | Implementation |
|--------|----------------|
| Authentication | JWT Bearer Tokens through Ocelot Gateway |
| Validation | FluentValidation 11.11.0 |
| Mapping | AutoMapper 14.0.0 |
| Pipeline | MediatR Behaviors: Validation, Performance, Exception |
| Observability | Serilog + Elasticsearch + Kibana |
| Resilience | Polly retry policies, Saga compensation |
| Deployment | Docker Compose (multi-container stack) |

---

## 🧠 6. Next Phase — AI Integration

| Module | Description | Technology |
|---------|--------------|-------------|
| 🤖 **Recommendation Engine** | Suggest products based on purchase & behavior history | Python FastAPI + Scikit-Learn |
| 💬 **Chatbot Assistant** | Conversational product search & order support | LangChain + GPT API |
| 📈 **Demand Forecasting** | Predict inventory demand from order trends | Prophet / LSTM |
| 🕵️ **Anomaly Detection** | Detect abnormal orders/payments | Isolation Forest |

Integration planned via **REST/gRPC endpoints** or **RabbitMQ events**.

---

## 🧩 7. Deployment Topology

```
        ┌──────────────┐
        │ Ocelot Gateway│
        └──────┬───────┘
               │
      ┌────────┴────────┐
      │                 │
 ┌───────────┐     ┌─────────────┐
 │ Product   │     │ Customer    │
 │  API      │     │  API        │
 └───────────┘     └─────────────┘
      │                 │
 ┌───────────┐     ┌─────────────┐
 │ Ordering  │◄───►│ Inventory   │
 │  API      │ gRPC │ .Grpc/API   │
 └───────────┘     └─────────────┘
      │
 ┌───────────┐
 │ Saga.Orch │
 └───────────┘
      │
 ┌───────────┐
 │ RabbitMQ  │
 └───────────┘
      │
 ┌───────────┐
 │ Logging   │
 │ (ELK)     │
 └───────────┘
```

---

## 🧩 8. Current Strengths

✅ Clean Architecture & CQRS in Ordering Service  
✅ Repository & Unit of Work Patterns  
✅ Domain + Integration Events via RabbitMQ  
✅ Saga Pattern for distributed transactions  
✅ gRPC inter-service calls  
✅ Centralized Logging (Serilog → ELK)  
✅ Health Monitoring + Dockerized Infrastructure  
✅ API Gateway (Ocelot + JWT Auth)

---

## 🏁 9. Conclusion

The system is now **fully production-ready at the backend level**, verified with:  
- **8 microservices operational**  
- **Stable inter-service messaging**  
- **Containerized environment**  
- **Complete monitoring & logging**  

The next step focuses on **AI integration, testing, and documentation** for final thesis submission (Dec 2025).
