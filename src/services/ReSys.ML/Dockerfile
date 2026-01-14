FROM python:3.11-slim

WORKDIR /app

# Install build dependencies and uv
RUN apt-get update && apt-get install -y \
    build-essential \
    curl \
    && rm -rf /var/lib/apt/lists/* \
    && pip install uv

COPY requirements.txt .

# Create venv and install dependencies with uv
RUN uv venv .venv && \
    . .venv/bin/activate && \
    uv pip install --no-cache-dir -r requirements.txt

COPY src/ ./src/

EXPOSE 8000

ENV PYTHONPATH=/app/src
ENV USE_MOCK_ML=false
# Ensure venv binaries are on PATH
ENV PATH="/app/.venv/bin:$PATH"

CMD ["uvicorn", "src.main:app", "--host", "0.0.0.0", "--port", "8000"]