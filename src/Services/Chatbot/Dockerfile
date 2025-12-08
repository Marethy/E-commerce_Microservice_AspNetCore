FROM python:3.11-slim AS base
WORKDIR /app
EXPOSE 80

FROM base AS build
WORKDIR /app

COPY src/Services/Chatbot/requirements.txt .
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt && \
    pip install --no-cache-dir grpcio-tools

FROM base AS final
WORKDIR /app

COPY --from=build /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=build /usr/local/bin /usr/local/bin

COPY src/Services/Chatbot/ .

RUN python -m grpc_tools.protoc -I./protos --python_out=./protos --grpc_python_out=./protos ./protos/mcp.proto && \
    sed -i 's/import mcp_pb2/from protos import mcp_pb2/' ./protos/mcp_pb2_grpc.py

RUN mkdir -p /app/data

ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1
ENV SQLITE_DB_PATH=/app/data/chatbot.db

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD python -c "import httpx; httpx.get('http://localhost/health', timeout=5.0)" || exit 1

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]
