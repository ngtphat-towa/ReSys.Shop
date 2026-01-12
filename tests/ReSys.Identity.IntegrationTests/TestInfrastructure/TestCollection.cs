using Xunit;

namespace ReSys.Identity.IntegrationTests.TestInfrastructure;

[CollectionDefinition("Shared Identity Database")]
public class TestCollection : ICollectionFixture<IdentityApiFactory>
{
}