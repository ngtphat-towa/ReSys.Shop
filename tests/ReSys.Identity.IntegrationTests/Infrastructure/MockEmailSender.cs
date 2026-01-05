using ReSys.Core.Common.Mailing;

namespace ReSys.Identity.IntegrationTests.Infrastructure;

public class MockEmailSender : IMailService
{
    public string LastToken { get; private set; } = string.Empty;

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Extract token from body "Token: {token}"
        if (body.StartsWith("Token: "))
        {
            LastToken = body.Substring(7);
        }
        return Task.CompletedTask;
    }
}
