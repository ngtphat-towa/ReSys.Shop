using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class TestOutputSink(ITestOutputHelper output) : ILogEventSink
{
    private readonly ITestOutputHelper _output = output;
    private readonly IFormatProvider? _formatProvider = null;

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _output.WriteLine($"[{logEvent.Level}] {message}");
        
        if (logEvent.Exception != null)
        {
            _output.WriteLine(logEvent.Exception.ToString());
        }
    }
}
