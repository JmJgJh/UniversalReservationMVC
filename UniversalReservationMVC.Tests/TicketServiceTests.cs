using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Tests.Fakes;

namespace UniversalReservationMVC.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task BuyTicket_CreatesTicketSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<TicketService>>();
        var service = new TicketService(unitOfWork, logger);

        var reservation = new Reservation
        {
            ResourceId = 1,
            UserId = "user1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = ReservationStatus.Confirmed
        };
        await unitOfWork.Reservations.AddAsync(reservation);
        await unitOfWork.SaveAsync();

        // Act
        var result = await service.BuyTicketAsync(reservation.Id, 50.00m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50.00m, result.Price);
        Assert.Equal(TicketStatus.Purchased, result.Status);
        Assert.NotNull(result.PurchaseReference);
    }

    [Fact]
    public async Task BuyTicket_ThrowsException_WhenReservationNotFound()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<TicketService>>();
        var service = new TicketService(unitOfWork, logger);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            service.BuyTicketAsync(999, 50.00m));
    }

    [Fact]
    public async Task BuyTicket_ThrowsException_WhenTicketAlreadyPurchased()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<TicketService>>();
        var service = new TicketService(unitOfWork, logger);

        var reservation = new Reservation
        {
            ResourceId = 1,
            UserId = "user1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = ReservationStatus.Confirmed
        };
        await unitOfWork.Reservations.AddAsync(reservation);
        await unitOfWork.SaveAsync();

        // Buy first ticket
        await service.BuyTicketAsync(reservation.Id, 50.00m);

        // Act & Assert - Try to buy again
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.BuyTicketAsync(reservation.Id, 50.00m));
    }

    [Fact]
    public async Task CancelTicket_CancelsTicketSuccessfully()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<TicketService>>();
        var service = new TicketService(unitOfWork, logger);

        var ticket = new Ticket
        {
            ReservationId = 1,
            Price = 50.00m,
            Status = TicketStatus.Purchased
        };
        await unitOfWork.Tickets.AddAsync(ticket);
        await unitOfWork.SaveAsync();

        // Act
        await service.CancelTicketAsync(ticket.Id);

        // Assert
        var cancelled = await unitOfWork.Tickets.GetByIdAsync(ticket.Id);
        Assert.NotNull(cancelled);
        Assert.Equal(TicketStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task GetTicketsForUser_ReturnsUserTickets()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();
        var logger = Mock.Of<ILogger<TicketService>>();
        var service = new TicketService(unitOfWork, logger);

        var reservation1 = new Reservation
        {
            ResourceId = 1,
            UserId = "user1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = ReservationStatus.Confirmed
        };
        var reservation2 = new Reservation
        {
            ResourceId = 1,
            UserId = "user2",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
            Status = ReservationStatus.Confirmed
        };
        await unitOfWork.Reservations.AddAsync(reservation1);
        await unitOfWork.Reservations.AddAsync(reservation2);
        await unitOfWork.SaveAsync();

        var ticket1 = new Ticket
        {
            ReservationId = reservation1.Id,
            Price = 50.00m,
            Status = TicketStatus.Purchased,
            Reservation = reservation1
        };
        var ticket2 = new Ticket
        {
            ReservationId = reservation2.Id,
            Price = 60.00m,
            Status = TicketStatus.Purchased,
            Reservation = reservation2
        };
        await unitOfWork.Tickets.AddAsync(ticket1);
        await unitOfWork.Tickets.AddAsync(ticket2);
        await unitOfWork.SaveAsync();

        // Act
        var result = await service.GetTicketsForUserAsync("user1");

        // Assert
        Assert.Single(result);
        Assert.Equal(50.00m, result.First().Price);
    }
}
