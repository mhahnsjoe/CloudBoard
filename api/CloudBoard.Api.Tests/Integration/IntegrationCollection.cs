using Xunit;

namespace CloudBoard.Api.Tests.Integration;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>
{
    // This class has no code. Its purpose is to define the Collection 
    // and attach the Factory fixture to it.
}