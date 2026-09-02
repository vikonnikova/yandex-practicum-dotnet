using System.Text.Json;
using Confluent.Kafka;
using Events.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Events.IntegrationTests.Kafka;

[Collection("Kafka collection")]
public class BookingConfirmedConsumerTests(KafkaFixture kafkaFixture) : IAsyncLifetime
{
    /// <summary>
    /// Проверяет, что консьюмер резервирует места по сообщению о подтверждённой брони.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenBookingConfirmed_ShouldReserveSeats()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingConfirmed = new BookingConfirmedEvent(
            Guid.NewGuid(),
            eventId,
            TestData.UserId,
            SeatsCount: 1,
            ConfirmedAt: new DateTime(2026, 01, 15, 12, 00, 00, DateTimeKind.Utc));

        await kafkaFixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(
                eventId,
                TestData.Event1Title,
                TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt),
                TestData.Event1TotalSeats));
            await Task.CompletedTask;
        });

        // Act
        await ProduceAsync(bookingConfirmed);

        // Assert
        await WaitUntilAsync(async () =>
        {
            Event? @event = null;
            await kafkaFixture.ExecuteDbContextAsync(async dbContext =>
            {
                @event = await dbContext.Events.AsNoTracking().SingleAsync(x => x.Id == eventId);
            });

            return @event is { AvailableSeats: TestData.Event1TotalSeats - 1 };
        }, TimeSpan.FromSeconds(30));
    }

    public Task InitializeAsync() => kafkaFixture.ClearTablesAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ProduceAsync(BookingConfirmedEvent bookingConfirmed)
    {
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = kafkaFixture.BootstrapServers
        }).Build();

        var result = await producer.ProduceAsync(
            KafkaConstants.BookingConfirmedTopic,
            new Message<string, string>
            {
                Key = bookingConfirmed.EventId.ToString(),
                Value = JsonSerializer.Serialize(bookingConfirmed)
            });

        result.Status.Should().Be(PersistenceStatus.Persisted);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException("Консьюмер Kafka не обработал сообщение за отведённое время.");
    }
}
