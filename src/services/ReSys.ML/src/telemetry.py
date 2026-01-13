import os
import logging
from opentelemetry import trace, metrics
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

# OpenTelemetry Metrics
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter

# OpenTelemetry Logging
from opentelemetry._logs import set_logger_provider
from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter

from settings import settings

logger = logging.getLogger(__name__)

def setup_telemetry():
    endpoint = settings.OTEL_EXPORTER_OTLP_ENDPOINT
    
    # Check if already configured to avoid "Overriding" warnings on reload
    if not endpoint or isinstance(trace.get_tracer_provider(), TracerProvider):
        if not endpoint:
            # Fallback logger if no OTel
            logging.basicConfig(level=logging.INFO)
        return

    resource = Resource.create({"service.name": os.getenv("OTEL_SERVICE_NAME", settings.SERVICE_NAME)})
    
    # 1. Configure Credentials (if HTTPS)
    credentials = None
    if endpoint.startswith("https://"):
        # Look for local dev cert in the project root (up 2 levels from src)
        cert_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), "local-cert.pem")
        if os.path.exists(cert_path):
            with open(cert_path, "rb") as f:
                trusted_certs = f.read()
            import grpc
            credentials = grpc.ssl_channel_credentials(root_certificates=trusted_certs)
            logger.info("Loaded local SSL certificate for OpenTelemetry.")
        else:
            logger.warning("OTLP endpoint is HTTPS but local-cert.pem not found. SSL verification may fail.")

    # 2. Configure Exporters
    if credentials:
        trace_exporter = OTLPSpanExporter(endpoint=endpoint, credentials=credentials, insecure=False)
        metric_exporter = OTLPMetricExporter(endpoint=endpoint, credentials=credentials, insecure=False)
        log_exporter = OTLPLogExporter(endpoint=endpoint, credentials=credentials, insecure=False)
    else:
        trace_exporter = OTLPSpanExporter(endpoint=endpoint, insecure=True)
        metric_exporter = OTLPMetricExporter(endpoint=endpoint, insecure=True)
        log_exporter = OTLPLogExporter(endpoint=endpoint, insecure=True)
    
    # 3. Setup Tracing
    tracer_provider = TracerProvider(resource=resource)
    tracer_provider.add_span_processor(BatchSpanProcessor(trace_exporter))
    trace.set_tracer_provider(tracer_provider)

    # 4. Setup Metrics
    meter_provider = MeterProvider(
        resource=resource,
        metric_readers=[PeriodicExportingMetricReader(metric_exporter)]
    )
    metrics.set_meter_provider(meter_provider)

    # 5. Setup Logging
    logger_provider = LoggerProvider(resource=resource)
    set_logger_provider(logger_provider)
    logger_provider.add_log_record_processor(BatchLogRecordProcessor(log_exporter))

    # Attach OTel handler to root logger
    handler = LoggingHandler(level=logging.NOTSET, logger_provider=logger_provider)
    logging.getLogger().addHandler(handler)
    logging.getLogger().setLevel(logging.INFO)
    
    logger.info(f"OpenTelemetry initialized for {settings.SERVICE_NAME} at {endpoint}")
