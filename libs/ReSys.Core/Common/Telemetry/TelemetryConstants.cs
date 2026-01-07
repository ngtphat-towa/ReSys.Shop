using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ReSys.Core.Common.Telemetry;

public static class TelemetryConstants
{
    public const string ServiceName = "ReSys.Shop";
    
    // Tracing
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    // Metrics
    public static readonly Meter Meter = new(ServiceName);
    
    // 1. Application / Shared Instruments
    public static class App
    {
        public static readonly Histogram<double> UseCaseDuration = Meter.CreateHistogram<double>(
            "usecase.duration", "ms", "Duration of use case execution");
            
        public static readonly Counter<long> UseCaseErrors = Meter.CreateCounter<long>(
            "usecase.errors", "count", "Total number of use case failures");
    }

    // 2. Storage Domain
    public static class Storage
    {
        public static readonly Counter<long> FilesUploaded = Meter.CreateCounter<long>(
            "storage.files_uploaded", "count", "Number of files uploaded to storage");
            
        public static readonly Histogram<long> FileSize = Meter.CreateHistogram<long>(
            "storage.file_size", "bytes", "Size of uploaded files");
    }
}
