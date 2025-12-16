# ---------------------------------------------
# Stage 1: Base
# ---------------------------------------------
FROM python:3.11-slim AS base
WORKDIR /app
EXPOSE 50051

# ---------------------------------------------
# Stage 2: Build & Setup
# ---------------------------------------------
FROM base AS build
WORKDIR /app

# [FIX LỖI] Cài đặt công cụ biên dịch C++ để build grpcio
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    gcc \
    g++ \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

# A. COPY REQUIREMENTS & INSTALL
COPY src/Services/MCP/requirements.txt .

# Thêm --prefer-binary để ưu tiên dùng file cài sẵn thay vì compile lại
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir --prefer-binary -r requirements.txt && \
    pip install --no-cache-dir --prefer-binary grpcio-tools && \
    playwright install chromium

# B. COPY SOURCE CODE & GENERATE PROTOS
COPY src/Services/MCP/ .

# Generate code gRPC
RUN python -m grpc_tools.protoc -I./protos --python_out=./protos --grpc_python_out=./protos ./protos/mcp.proto && \
    sed -i 's/import mcp_pb2/from protos import mcp_pb2/' ./protos/mcp_pb2_grpc.py

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app

# 1. CÀI ĐẶT SYSTEM DEPENDENCIES (Runtime cho Playwright)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 \
    libxkbcommon0 libxcomposite1 libxdamage1 libxrandr2 libgbm1 \
    libasound2 libpango-1.0-0 libcairo2 libxfixes3 libxext6 \
    && rm -rf /var/lib/apt/lists/*

# 2. COPY Artifacts từ Stage Build
COPY --from=build /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=build /usr/local/bin /usr/local/bin
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright
COPY --from=build /app /app

# 3. Setup Environment
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD python -c "import grpc; ch=grpc.insecure_channel('localhost:50051'); grpc.channel_ready_future(ch).result(timeout=5)" || exit 1

CMD ["python", "server.py"]