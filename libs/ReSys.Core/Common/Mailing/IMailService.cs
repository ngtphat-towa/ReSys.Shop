namespace ReSys.Core.Common.Mailing;

public interface IMailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class FakeMailService : IMailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        return Task.CompletedTask;
    }
}
