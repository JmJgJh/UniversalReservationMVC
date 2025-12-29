using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Tests.Fakes;

namespace UniversalReservationMVC.Tests;

public class EventServiceTests
{
    [Fact]
    public async Task GetUpcomingEvents_ReturnsOnlyFutureEvents()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        var pastEvent = new Event
        {
            Title = "Past Event",
            Description = "Past",
            StartTime = DateTime.UtcNow.AddDays(-5),
            EndTime = DateTime.UtcNow.AddDays(-5).AddHours(2),
            ResourceId = 1
        };

        var futureEvent = new Event
        {
            Title = "Future Event",
            Description = "Future",
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(2),
            ResourceId = 1
        };

        await unitOfWork.Events.AddAsync(pastEvent);
        await unitOfWork.Events.AddAsync(futureEvent);
        await unitOfWork.SaveAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Future Event", result.First().Title);
    }

    [Fact]
    public async Task CreateEvent_CreatesEventSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        var newEvent = new Event
        {
            Title = "New Event",
            Description = "Description",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            ResourceId = 1
        };

        // Act
        var result = await service.CreateEventAsync(newEvent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Event", result.Title);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateEvent_UpdatesEventSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        var existingEvent = new Event
        {
            Title = "Original Title",
            Description = "Original",
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(2),
            ResourceId = 1
        };
        await unitOfWork.Events.AddAsync(existingEvent);
        await unitOfWork.SaveAsync();

        // Act
        existingEvent.Title = "Updated Title";
        await service.UpdateEventAsync(existingEvent);

        // Assert
        var updated = await unitOfWork.Events.GetByIdAsync(existingEvent.Id);
        Assert.Equal("Updated Title", updated?.Title);
    }

    [Fact]
    public async Task GetEventById_ReturnsNull_WhenEventDoesNotExist()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        // Act
        var result = await service.GetEventByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteEvent_RemovesEvent()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        var eventToDelete = new Event
        {
            Title = "Event to Delete",
            Description = "To be deleted",
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(2),
            ResourceId = 1
        };
        await unitOfWork.Events.AddAsync(eventToDelete);
        await unitOfWork.SaveAsync();

        // Act
        await service.DeleteEventAsync(eventToDelete.Id);

        // Assert
        var deleted = await unitOfWork.Events.GetByIdAsync(eventToDelete.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetUpcomingEvents_FiltersByResourceId()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<EventService>>();
        var service = new EventService(unitOfWork, logger);

        var event1 = new Event
        {
            Title = "Event for Resource 1",
            Description = "Resource 1",
            ResourceId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        var event2 = new Event
        {
            Title = "Event for Resource 2",
            Description = "Resource 2",
            ResourceId = 2,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        await unitOfWork.Events.AddAsync(event1);
        await unitOfWork.Events.AddAsync(event2);
        await unitOfWork.SaveAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Event for Resource 1", result.First().Title);
    }
}
