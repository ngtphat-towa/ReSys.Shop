using ReSys.Core.Common.Mailing;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class TestMailService : IMailService
{
    public string? LastTo { get; private set; }
    public string? LastSubject { get; private set; }
    public string? LastBody { get; private set; }
    
    public string? LastToken { get; private set; }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        LastTo = to;
        LastSubject = subject;
        LastBody = body;
        
        // Extract token from body if possible (simple heuristic for tests)
        if (body.Contains("token="))
        {
            var parts = body.Split("token=");
            if (parts.Length > 1)
            {
                LastToken = parts[1].Split("&")[0].Split(" ")[0].Trim();
            }
        }
        else if (body.Contains("Token: "))
        {
            var parts = body.Split("Token: ");
            if (parts.Length > 1)
            {
                LastToken = parts[1].Trim();
            }
        }

        return Task.CompletedTask;
    }
}
