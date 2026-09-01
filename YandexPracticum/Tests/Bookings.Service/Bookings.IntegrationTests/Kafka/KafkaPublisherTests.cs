using System.Text.Json;
using Bookings.Infrastructure.Services;
using Confluent.Kafka;
using FluentAssertions;
using Shared.Contracts;

namespace Bookings.IntegrationTests.Kafka;

[Collection("Kafka collection")]
public class KafkaPublisherTests(KafkaFixture kafkaFixture)
{
    /// <summary>
    /// Проверяет публикацию события подтверждения брони в Kafka.
    /// </summary>
    [Fact]
    public async Task PublishBookingConfirmedAsync_WhenValidData_ShouldProduceMessage()
    {
        // Arrange
        var bookingConfirmed = new BookingConfirmedEvent(
            TestData.BookingId,
            TestData.EventId,
            TestData.UserId,
            SeatsCount: 1,
            ConfirmedAt: new DateTime(2026, 01, 15, 12, 00, 00, DateTimeKind.Utc));

        using var publisher = new KafkaPublisher(new KafkaSettings
        {
            BootstrapServers = kafkaFixture.BootstrapServers
        });

        // Act
        await publisher.PublishBookingConfirmedAsync(bookingConfirmed, CancellationToken.None);

        // Assert
        using var consumer = CreateConsumer(kafkaFixture.BootstrapServers);
        consumer.Subscribe(KafkaConstants.BookingConfirmedTopic);
        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(20));
        consumeResult.Should().NotBeNull();
        consumeResult.Message.Key.Should().Be(TestData.EventId.ToString());

        var payload = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value);
        payload.Should().BeEquivalentTo(bookingConfirmed);
    }

    private static IConsumer<string, string> CreateConsumer(string bootstrapServers)
    {
        return new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"bookings-it-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        }).Build();
    }
}
