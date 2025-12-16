# ---------------------------------------------
# Stage 1: Base
# ---------------------------------------------
FROM python:3.11-slim AS base
WORKDIR /app
EXPOSE 80

# ---------------------------------------------
# Stage 2: Build & Setup
# ---------------------------------------------
FROM base AS build
WORKDIR /app

# A. COPY REQUIREMENTS & INSTALL
# Chỉ copy file requirements trước.
# Nếu file này không đổi, Docker sẽ dùng Cache và bỏ qua bước pip install (tiết kiệm rất nhiều thời gian).
COPY src/Services/Chatbot/requirements.txt .

RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt && \
    pip install --no-cache-dir grpcio-tools

# B. COPY SOURCE CODE & GENERATE PROTOS
# Copy toàn bộ source code của Chatbot vào
COPY src/Services/Chatbot/ .

# Chạy lệnh generate proto ngay tại bước build để đảm bảo file sinh ra đã sẵn sàng
RUN python -m grpc_tools.protoc -I./protos --python_out=./protos --grpc_python_out=./protos ./protos/mcp.proto && \
    sed -i 's/import mcp_pb2/from protos import mcp_pb2/' ./protos/mcp_pb2_grpc.py

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app

# 1. Copy thư viện đã cài từ stage build
COPY --from=build /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=build /usr/local/bin /usr/local/bin

# 2. Copy source code (bao gồm cả file proto đã generate) từ stage build
# Lưu ý: Ta copy từ 'build' chứ không copy từ máy thật để lấy luôn các file .py đã generate ở bước trên.
COPY --from=build /app /app

# Setup Environment
RUN mkdir -p /app/data
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1
ENV SQLITE_DB_PATH=/app/data/chatbot.db

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD python -c "import httpx; httpx.get('http://localhost/health', timeout=5.0)" || exit 1

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]