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

# Install system dependencies for Playwright
RUN apt-get update && apt-get install -y \
    wget \
    ca-certificates \
    fonts-liberation \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libatspi2.0-0 \
    libcups2 \
    libdbus-1-3 \
    libdrm2 \
    libgbm1 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libwayland-client0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    xdg-utils \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python packages
COPY src/Services/Chatbot/requirements.txt .
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Install Playwright browsers
RUN playwright install chromium

# Copy source code
COPY src/Services/Chatbot/ .

# ---------------------------------------------
# Stage 3: Final
# ---------------------------------------------
FROM base AS final
WORKDIR /app

# Copy installed packages from build stage
COPY --from=build /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=build /usr/local/bin /usr/local/bin

# Copy Playwright browsers
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# Copy source code
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