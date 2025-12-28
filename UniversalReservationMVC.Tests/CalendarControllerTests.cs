using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Controllers;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class CalendarControllerTests
    {
        [Fact]
        public async Task GetReservations_ReturnsEventsAndReservations()
        {
            // Arrange: in-memory DbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            context.Resources.Add(new Resource { Id = 1, Name = "Sala" });
            context.Events.Add(new Event
            {
                Id = 10,
                ResourceId = 1,
                Title = "Konferencja",
                StartTime = new DateTime(2025, 1, 10, 9, 0, 0),
                EndTime = new DateTime(2025, 1, 10, 17, 0, 0)
            });
            context.Reservations.Add(new Reservation
            {
                Id = 20,
                ResourceId = 1,
                StartTime = new DateTime(2025, 1, 11, 12, 0, 0),
                EndTime = new DateTime(2025, 1, 11, 14, 0, 0),
                Status = ReservationStatus.Confirmed,
                User = new ApplicationUser { FirstName = "Jan", LastName = "Kowalski" }
            });
            await context.SaveChangesAsync();

            var controller = new CalendarController(context);

            // Act
            var result = await controller.GetReservations(resourceId: 1, year: 2025, month: 1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var payload = JsonSerializer.Deserialize<CalendarResponse>(JsonSerializer.Serialize(json.Value));
            Assert.NotNull(payload);
            Assert.True(payload!.success);
            Assert.Single(payload.events);
            Assert.Single(payload.reservations);
        }

        [Fact]
        public async Task GetReservations_ReturnsNotFound_WhenResourceDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            var controller = new CalendarController(context);

            // Act - request non-existent resource
            var result = await controller.GetReservations(resourceId: 999, year: 2025, month: 1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var payload = JsonSerializer.Deserialize<ErrorResponse>(JsonSerializer.Serialize(json.Value));
            Assert.NotNull(payload);
            Assert.False(payload!.success);
            Assert.Contains("nie został znaleziony", payload.message);
        }

        private sealed record CalendarResponse(bool success, List<object> events, List<object> reservations, int daysInMonth, int firstDayOfWeek);
        private sealed record ErrorResponse(bool success, string message);
    }
}
