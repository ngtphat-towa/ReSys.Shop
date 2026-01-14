import random
import logging
from .settings import settings

logger = logging.getLogger(__name__)

class MLEngine:
    def __init__(self):
        self.model = None

    def load_model(self):
        if not settings.USE_MOCK_ML:
            logger.info(f"Loading model: {settings.MODEL_NAME}...")
            try:
                from sentence_transformers import SentenceTransformer
                self.model = SentenceTransformer(settings.MODEL_NAME)
                logger.info("Model loaded successfully.")
            except ImportError:
                logger.warning("sentence-transformers not found. Falling back to MOCK mode.")
                self.model = None
        else:
            logger.info("Running in MOCK mode (configured via env).")
            self.model = None

    def generate_embedding(self, text_or_image: str) -> list[float]:
        if self.model:
            # sentence-transformers encode returns a numpy array, convert to list
            return self.model.encode(text_or_image).tolist()
        else:
            # Mock embedding (384 dimensions to match MiniLM)
            dimension = 384
            return [random.random() for _ in range(dimension)]

    def is_mock(self) -> bool:
        return self.model is None

# Global instance
engine = MLEngine()
