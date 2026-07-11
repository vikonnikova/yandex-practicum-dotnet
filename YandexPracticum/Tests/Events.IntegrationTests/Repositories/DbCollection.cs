using Events.IntegrationTests.Infrastructure;

namespace Events.IntegrationTests.Repositories;

[CollectionDefinition("Database collection")]
public class DbCollection : ICollectionFixture<DbFixture>
{
}