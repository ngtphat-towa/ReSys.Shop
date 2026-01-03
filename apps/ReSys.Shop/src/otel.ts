import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { LoggerProvider, BatchLogRecordProcessor } from '@opentelemetry/sdk-logs';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-http';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';

export function setupOpenTelemetry(serviceName: string) {
    if (import.meta.env.VITE_OTEL_ENABLED !== 'true') {
        return;
    }

    const exporter = new OTLPTraceExporter({
        url: 'http://localhost:4318/v1/traces', // Standard Aspire Dashboard OTLP endpoint
    });

    const provider = new WebTracerProvider({
        resource: resourceFromAttributes({
            [ATTR_SERVICE_NAME]: serviceName,
        }),
        spanProcessors: [
            new SimpleSpanProcessor(exporter),
        ],
    });

    // --- Logging Setup ---
    const logExporter = new OTLPLogExporter({
        url: 'http://localhost:4318/v1/logs',
    });

    const loggerProvider = new LoggerProvider({
        resource: resourceFromAttributes({
            [ATTR_SERVICE_NAME]: serviceName,
        }),
        processors: [
            new BatchLogRecordProcessor(logExporter),
        ],
    });

    // Hook into console
    const originalConsoleLog = console.log;
    const originalConsoleWarn = console.warn;
    const originalConsoleError = console.error;

    const logger = loggerProvider.getLogger('default', '1.0.0');

    console.log = (...args) => {
        originalConsoleLog(...args);
        logger.emit({
            body: args.map(String).join(' '),
            severityNumber: 9, // INFO
            severityText: 'INFO',
        });
    };

    console.warn = (...args) => {
        originalConsoleWarn(...args);
        logger.emit({
            body: args.map(String).join(' '),
            severityNumber: 13, // WARN
            severityText: 'WARN',
        });
    };

    console.error = (...args) => {
        originalConsoleError(...args);
        logger.emit({
            body: args.map(String).join(' '),
            severityNumber: 17, // ERROR
            severityText: 'ERROR',
        });
    };
    // ---------------------

    // ... endpoint comments ...

    provider.register();

    registerInstrumentations({
        instrumentations: [
            new DocumentLoadInstrumentation(),
            new FetchInstrumentation({
                propagateTraceHeaderCorsUrls: [/.+/g], // Propagate headers to all URLs (Backend API)
            }),
        ],
    });

    console.log(`OpenTelemetry initialized for ${serviceName}`);
}
