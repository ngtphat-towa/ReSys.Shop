import os

class Settings:
    PORT: int = int(os.getenv("PORT", "8000"))
    ROOT_PATH: str = os.getenv("ROOT_PATH", "")
    USE_MOCK_ML: bool = os.getenv("USE_MOCK_ML", "false").lower() == "true"
    MODEL_NAME: str = "all-MiniLM-L6-v2"
    OTEL_EXPORTER_OTLP_ENDPOINT: str = os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "")
    SERVICE_NAME: str = "ml"

settings = Settings()
