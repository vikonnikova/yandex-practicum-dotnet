using Events.IntegrationTests.Infrastructure;

namespace Events.IntegrationTests.Repositories.Base;

[CollectionDefinition("Database collection")]
public class DbCollection : ICollectionFixture<DbFixture>
{
}