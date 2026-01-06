using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Infrastructure.Notifications.Services;
using Sinch;
using Sinch.SMS;
using Sinch.SMS.Batches;
using Sinch.SMS.Batches.Send;
using ReSys.Core.Common.Notifications.Errors;
using Xunit;

namespace ReSys.Infrastructure.UnitTests.Notifications.Services;

public class SmsSenderServiceTests
{
    private readonly ISinchClient _sinchClient;
    private readonly ILogger<SmsSenderService> _logger;
    private readonly SmsSenderService _service;

    public SmsSenderServiceTests()
    {
        _sinchClient = Substitute.For<ISinchClient>();
        _logger = Substitute.For<ILogger<SmsSenderService>>();
        
        var options = Options.Create(new SmsOptions
        {
            DefaultSenderNumber = "12345",
            EnableSmsNotifications = true,
            Provider = "sinch",
            SinchConfig = new SinchConfig { SenderPhoneNumber = "12345" }
        });

        _service = new SmsSenderService(options, _sinchClient, _logger);
    }

    [Fact(DisplayName = "Sms Sender should return success when SMS is sent successfully via Sinch")]
    public async Task SendAsync_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var to = NotificationRecipient.Create("+1234567890");
        var content = NotificationContent.Create("", "Test Body");
        var metadata = NotificationMetadata.Default;
        
        var smsApi = Substitute.For<ISinchSms>();
        _sinchClient.Sms.Returns(smsApi);
        
        smsApi.Batches.Send(Arg.Any<SendTextBatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IBatch>());

        // Act
        var result = await _service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact(DisplayName = "Sms Sender should return failure when Sinch SDK throws exception")]
    public async Task SendAsync_WhenSinchFails_ShouldReturnFailure()
    {
        // Arrange
        var to = NotificationRecipient.Create("+1234567890");
        var content = NotificationContent.Create("", "Test Body");
        var metadata = NotificationMetadata.Default;
        
        _sinchClient.Sms.Returns(x => throw new Exception("Sinch Error"));

        // Act
        var result = await _service.SendAsync(to, content, metadata, ct: TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(NotificationErrors.Sms.SendFailed("").Code);
    }
}