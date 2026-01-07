using Xunit;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class TestOutputHelperProxy : ITestOutputHelper
{
    private readonly AsyncLocal<ITestOutputHelper?> _currentOutput = new();

    public ITestOutputHelper? Current
    {
        get => _currentOutput.Value;
        set => _currentOutput.Value = value;
    }

    public string Output => _currentOutput.Value?.Output ?? string.Empty;

    public void Write(string message)
    {
        _currentOutput.Value?.Write(message);
    }

    public void Write(string format, params object[] args)
    {
        _currentOutput.Value?.Write(format, args);
    }

    public void WriteLine(string message)
    {
        _currentOutput.Value?.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        _currentOutput.Value?.WriteLine(format, args);
    }
}
