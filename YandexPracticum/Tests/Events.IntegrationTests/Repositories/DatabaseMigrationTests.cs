using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class DatabaseMigrationTests(DbFixture dbFixture)
{
    /// <summary>
    /// Проверяет, что миграция корректно создала таблицы bookings и events.
    /// </summary>
    [Fact]
    public async Task Migrations_ShouldCreateRequiredTables()
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
        tablesInDb.Should().Contain(["events", "bookings"]);
    }
    
	/// <summary>
    /// Проверяет, что миграция корректно создала связь Foreign Key между таблицами bookings и events.
    /// </summary>
    [Fact]
    public async Task Migrations_ShouldCreateForeignKeyConstraint()
    {
        // Arrange
        await using var context = dbFixture.CreateContext();
        
        var sqlQuery = """
            SELECT 
                tc.constraint_name AS ConstraintName
            FROM 
                information_schema.table_constraints AS tc 
                JOIN information_schema.key_column_usage AS kcu
                  ON tc.constraint_name = kcu.constraint_name
                  AND tc.table_schema = kcu.table_schema
                JOIN information_schema.constraint_column_usage AS ccu
                  ON ccu.constraint_name = tc.constraint_name
                  AND ccu.table_schema = tc.table_schema
            WHERE 
                tc.constraint_type = 'FOREIGN KEY' 
                AND tc.table_name = 'bookings' 
                AND kcu.column_name = 'EventId'
                AND ccu.table_name = 'events';
            """;

        // Act
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sqlQuery;
        
        if (context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await context.Database.GetDbConnection().OpenAsync();
        }

        var constraintName = (string?)await command.ExecuteScalarAsync();

        // Assert
        constraintName.Should().NotBeNullOrEmpty();
        constraintName.Should().Contain("FK_bookings_events");
    }
}