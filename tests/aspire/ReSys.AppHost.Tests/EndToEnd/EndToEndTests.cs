using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.EndToEnd;

[Trait("Category", "E2E")]
public class EndToEndTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact(DisplayName = "Full request flow through gateway to API should succeed")]
    public async Task FullRequestFlow_Should_Succeed()
    {
        // Arrange
        var gatewayClient = fixture.App.CreateHttpClient("gateway");

        // Act
        var response = await gatewayClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "User accessing shop should receive successful response")]
    public async Task UserAccessingShop_Should_ReceiveSuccessfulResponse()
    {
        // Arrange
        await fixture.WaitForResourceAsync("shop", TimeSpan.FromSeconds(90));
        var shopClient = fixture.App.CreateHttpClient("shop");

        // Act
        var response = await shopClient.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeNullOrWhiteSpace();
    }
}
