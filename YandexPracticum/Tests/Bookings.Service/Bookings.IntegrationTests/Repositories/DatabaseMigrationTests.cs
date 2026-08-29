using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bookings.IntegrationTests.Repositories;

[Collection("Database collection")]
public class DatabaseMigrationTests(DbFixture dbFixture)
{
    /// <summary>
    /// Проверяет, что миграция корректно создала таблицы.
    /// </summary>
    [Fact]
    public async Task Migrate_WhenApplied_ShouldCreateRequiredTables()
    {
        // Arrange
        await using var context = dbFixture.CreateContext();

        var sqlQuery = """
                       SELECT table_name 
                       FROM information_schema.tables 
                       WHERE table_schema = 'public';
                       """;

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sqlQuery;

        if (context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            await context.Database.GetDbConnection().OpenAsync();

        // Act
        var tablesInDb = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tablesInDb.Add(reader.GetString(0));
            }
        }

        // Assert
        tablesInDb.Should().Contain(["bookings"]);
    }
}