# AspNetCore Microservices - Basic E-Commerce System

## 📌 Overview

This project is a **basic e-commerce system** built using **microservices architecture** with ASP.NET Core. The system consists of multiple independent services that communicate with each other via REST APIs and messaging.

---

## 🛠 Development Environment

To develop and run this project, ensure that your environment is set up with the following tools:

### **Required Software**
- **.NET Core** (Check `global.json` for the required version)
- **IDE**: Visual Studio 2022+, Rider, or Visual Studio Code
- **Docker Desktop** (for running services in containers)

### **Important Notes**
- Some **Docker images are not compatible** with Apple Silicon (M1, M2). If you are using an Apple chip, replace them with the appropriate versions:
  - **SQL Server**: `mcr.microsoft.com/azure-sql-edge`
  - **MySQL**: `arm64v8/mysql:oracle`

---

## 🚀 How to Run the Project

### **1️⃣ Build the Project**
Run the following command to build the project:
```powershell
 dotnet build
```

### **2️⃣ Run with Docker-Compose**
Navigate to the folder containing `docker-compose.yml`, then run:
```powershell
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --remove-orphans --build
```

To stop and remove containers:
```powershell
docker-compose down
```

### **3️⃣ Run with Visual Studio**
- Open `aspnetcore-microservices.sln`
- Set **Compound** configuration to start multiple projects
- Run the solution

---

## 🌐 Application URLs

### **LOCAL Environment (Docker Container)**
- **Product API**: [http://localhost:6002/api/products](http://localhost:6002/api/products)
- **Customer API**: [http://localhost:6003/api/customers](http://localhost:6003/api/customers)
- **Basket API**: [http://localhost:6004/api/baskets](http://localhost:6004/api/baskets)

### **Development Environment**
- **Product API**: [http://localhost:5002/api/products](http://localhost:5002/api/products)
- **Customer API**: [http://localhost:5003/api/customers](http://localhost:5003/api/customers)
- **Basket API**: [http://localhost:5004/api/baskets](http://localhost:5004/api/baskets)

### **Production Environment**
(🍯 **To be defined**)

### **Docker Application URLs**
- **Portainer**: [http://localhost:9000](http://localhost:9000) (User: `admin`, Pass: `admin1234`)
- **Kibana**: [http://localhost:5601](http://localhost:5601) (User: `elastic`, Pass: `admin`)
- **RabbitMQ**: [http://localhost:15672](http://localhost:15672) (User: `guest`, Pass: `guest`)
- **PgAdmin**: [http://localhost:5050](http://localhost:5050) (User: `admin@example.com`, Pass: `Marethyu2004!`)

---

## 💎 Microservices & Infrastructure

### **Databases**
- **OrderDB** (SQL Server)
- **ProductDB** (MySQL)
- **CustomerDB** (PostgreSQL)
- **BasketDB** (Redis)
- **InventoryDB** (MongoDB)

### **Infrastructure Services**
- **RabbitMQ** (Message Broker)
- **PgAdmin** (PostgreSQL Management)
- **Portainer** (Container Management UI)
- **Elasticsearch** (Search Engine)
- **Kibana** (Visualization for Elasticsearch)

---

## 🛋️ Useful Commands

### **General Commands**
```powershell
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update
```
```powershell
dotnet watch run --environment "Development"
```
```powershell
dotnet restore
```
```powershell
dotnet build
```

### **Migration Commands for Ordering API**
```powershell
# Navigate to Ordering folder
cd Ordering

# Add new migration
 dotnet ef migrations add "SampleMigration" -p Ordering.Infrastructure --startup-project Ordering.API --output-dir Persistence/Migrations

# Remove last migration
 dotnet ef migrations remove -p Ordering.Infrastructure --startup-project Ordering.API

# Apply migrations to database
 dotnet ef database update -p Ordering.Infrastructure --startup-project Ordering.API
```

---

## 📖 References
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Microservices with .NET](https://dotnet.microsoft.com/en-us/apps/aspnet/microservices)
- [Docker Documentation](https://docs.docker.com/)

---

### ✅ **Contributors**
This project is actively maintained. Feel free to submit issues or pull requests to contribute! 🚀

# AspNetCore Microservices - Basic E-Commerce System

## 📌 Overview

This project is a **basic e-commerce system** built using **microservices architecture** with ASP.NET Core. The system consists of multiple independent services that communicate with each other via REST APIs and messaging.

---

## 🛠 Development Environment

To develop and run this project, ensure that your environment is set up with the following tools:

### **Required Software**
- **.NET Core** (Check `global.json` for the required version)
- **IDE**: Visual Studio 2022+, Rider, or Visual Studio Code
- **Docker Desktop** (for running services in containers)

### **Important Notes**
- Some **Docker images are not compatible** with Apple Silicon (M1, M2). If you are using an Apple chip, replace them with the appropriate versions:
  - **SQL Server**: `mcr.microsoft.com/azure-sql-edge`
  - **MySQL**: `arm64v8/mysql:oracle`

---

## 🚀 How to Run the Project

### **1️⃣ Build the Project**
Run the following command to build the project:
```powershell
 dotnet build
```

### **2️⃣ Run with Docker-Compose**
Navigate to the folder containing `docker-compose.yml`, then run:
```powershell
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --remove-orphans --build
```

To stop and remove containers:
```powershell
docker-compose down
```

### **3️⃣ Run with Visual Studio**
- Open `aspnetcore-microservices.sln`
- Set **Compound** configuration to start multiple projects
- Run the solution

---

## 🌐 Application URLs

### **LOCAL Environment (Docker Container)**
- **Product API**: [http://localhost:6002/api/products](http://localhost:6002/api/products)
- **Customer API**: [http://localhost:6003/api/customers](http://localhost:6003/api/customers)
- **Basket API**: [http://localhost:6004/api/baskets](http://localhost:6004/api/baskets)

### **Development Environment**
- **Product API**: [http://localhost:5002/api/products](http://localhost:5002/api/products)
- **Customer API**: [http://localhost:5003/api/customers](http://localhost:5003/api/customers)
- **Basket API**: [http://localhost:5004/api/baskets](http://localhost:5004/api/baskets)

### **Production Environment**
(🍯 **To be defined**)

### **Docker Application URLs**
- **Portainer**: [http://localhost:9000](http://localhost:9000) (User: `admin`, Pass: `admin1234`)
- **Kibana**: [http://localhost:5601](http://localhost:5601) (User: `elastic`, Pass: `admin`)
- **RabbitMQ**: [http://localhost:15672](http://localhost:15672) (User: `guest`, Pass: `guest`)
- **PgAdmin**: [http://localhost:5050](http://localhost:5050) (User: `admin@example.com`, Pass: `Marethyu2004!`)

---

## 💎 Microservices & Infrastructure

### **Databases**
- **OrderDB** (SQL Server)
- **ProductDB** (MySQL)
- **CustomerDB** (PostgreSQL)
- **BasketDB** (Redis)
- **InventoryDB** (MongoDB)

### **Infrastructure Services**
- **RabbitMQ** (Message Broker)
- **PgAdmin** (PostgreSQL Management)
- **Portainer** (Container Management UI)
- **Elasticsearch** (Search Engine)
- **Kibana** (Visualization for Elasticsearch)

---

## 🛋️ Useful Commands

### **General Commands**
```powershell
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update
```
```powershell
dotnet watch run --environment "Development"
```
```powershell
dotnet restore
```
```powershell
dotnet build
```

### **Migration Commands for Ordering API**
```powershell
# Navigate to Ordering folder
cd Ordering

# Add new migration
 dotnet ef migrations add "SampleMigration" -p Ordering.Infrastructure --startup-project Ordering.API --output-dir Persistence/Migrations

# Remove last migration
 dotnet ef migrations remove -p Ordering.Infrastructure --startup-project Ordering.API

# Apply migrations to database
 dotnet ef database update -p Ordering.Infrastructure --startup-project Ordering.API
```

---

## 📖 References
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Microservices with .NET](https://dotnet.microsoft.com/en-us/apps/aspnet/microservices)
- [Docker Documentation](https://docs.docker.com/)

---

### ✅ **Contributors**
This project is actively maintained. Feel free to submit issues or pull requests to contribute! 🚀

