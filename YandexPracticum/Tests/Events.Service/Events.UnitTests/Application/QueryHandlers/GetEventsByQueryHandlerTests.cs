using Events.Application;
using Events.Application.Contracts.Queries;
using Events.Application.QueryHandlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.QueryHandlers;

public class GetEventsByQueryHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет получение событий с пагинацией и фильтрацией.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFiltersAndPaginationProvided_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetEventsByQueryHandler>();
        var filters = new Filters(Title: "День", EventStartAt, EventEndAt);

        //Act
        var result = await handler.Handle(new GetEventsByQuery(Page, PageSize, filters), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.GetFiltered(Page, PageSize,
                It.Is<Filters>(x => x.Title == filters.Title && x.From == filters.From && x.To == filters.To),
                CancellationToken.None), Times.Once);

        result.Should().NotBeNull();
        result.TotalItems.Should().Be(100);
        result.CurrentPage.Should().Be(3);

        var item = result.Items.Should().ContainSingle().Subject;
        item.Title.Should().Be(EventTitle);
        item.Description.Should().Be(EventDescription);
        item.Period.StartAt.Should().Be(EventStartAt);
        item.Period.EndAt.Should().Be(EventEndAt);
        item.TotalSeats.Should().Be(EventTotalSeats);
        item.AvailableSeats.Should().Be(EventTotalSeats);
    }
}