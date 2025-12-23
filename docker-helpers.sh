#!/bin/bash
# Docker Compose Helper Scripts for Linux/Mac
# Use these to manage infrastructure and services separately

# Colors
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# ============================================================
# INFRASTRUCTURE (Databases, Cache, Search, etc.)
# ============================================================

infra-up() {
    echo -e "${CYAN}🔧 Starting infrastructure services...${NC}"
    docker-compose -f docker-compose.infrastructure.yml up -d
    echo -e "${GREEN}✅ Infrastructure started!${NC}"
}

infra-down() {
    echo -e "${YELLOW}⏸️  Stopping infrastructure services...${NC}"
    docker-compose -f docker-compose.infrastructure.yml stop
    echo -e "${GREEN}✅ Infrastructure stopped (data preserved)!${NC}"
}

infra-remove() {
    echo -e "${RED}🗑️  Removing infrastructure and ALL DATA...${NC}"
    read -p "Are you sure? This will delete all databases! (yes/no): " confirm
    if [ "$confirm" == "yes" ]; then
        docker-compose -f docker-compose.infrastructure.yml down -v
        echo -e "${GREEN}✅ Infrastructure removed!${NC}"
    else
        echo -e "${YELLOW}❌ Cancelled${NC}"
    fi
}

# ============================================================
# SERVICES (Microservices - rebuild often)
# ============================================================

services-up() {
    echo -e "${CYAN}🚀 Starting microservices...${NC}"
    docker-compose -f docker-compose.services.yml -f docker-compose.override.yml up -d
    echo -e "${GREEN}✅ Services started!${NC}"
}

services-rebuild() {
    echo -e "${CYAN}🔨 Rebuilding and starting microservices...${NC}"
    docker-compose -f docker-compose.services.yml -f docker-compose.override.yml up -d --build
    echo -e "${GREEN}✅ Services rebuilt and started!${NC}"
}

services-down() {
    echo -e "${YELLOW}⏸️  Stopping microservices...${NC}"
    docker-compose -f docker-compose.services.yml -f docker-compose.override.yml stop
    echo -e "${GREEN}✅ Services stopped!${NC}"
}

services-remove() {
    echo -e "${YELLOW}🗑️  Removing microservices...${NC}"
    docker-compose -f docker-compose.services.yml -f docker-compose.override.yml down
    echo -e "${GREEN}✅ Services removed (infrastructure still running)!${NC}"
}

service-restart() {
    echo -e "${CYAN}🔄 Restarting $1...${NC}"
    docker-compose -f docker-compose.services.yml -f docker-compose.override.yml restart $1
    echo -e "${GREEN}✅ $1 restarted!${NC}"
}

# ============================================================
# COMBINED OPERATIONS
# ============================================================

all-up() {
    infra-up
    echo "⏳ Waiting 10 seconds for databases to be ready..."
    sleep 10
    services-up
}

all-down() {
    services-down
    infra-down
}

logs() {
    if [ -n "$1" ]; then
        docker-compose -f docker-compose.services.yml -f docker-compose.override.yml logs -f $1
    else
        docker-compose -f docker-compose.services.yml -f docker-compose.override.yml logs -f
    fi
}

status() {
    echo -e "\n${CYAN}📊 Infrastructure:${NC}"
    docker-compose -f docker-compose.infrastructure.yml ps
    echo -e "\n${CYAN}📊 Services:${NC}"
    docker-compose -f docker-compose.services.yml ps
}

# ============================================================
# USAGE
# ============================================================

echo -e "${YELLOW}"
cat << "EOF"

🐳 Docker Compose Helper Loaded!

📖 Common Commands:
   infra-up              # Start databases (once)
   services-up           # Start microservices
   services-rebuild      # Rebuild & start services
   services-down         # Stop services only
   all-up                # Start everything
   status                # Check what's running
   logs product-api      # View specific service logs
   service-restart product-api

💡 Recommended Workflow:
   1. infra-up           # First time only
   2. services-rebuild   # Build and start services
   3. services-down      # Stop services when done
   4. services-rebuild   # Restart with new code
   5. infra-down         # Stop databases (optional)

EOF
echo -e "${NC}"
