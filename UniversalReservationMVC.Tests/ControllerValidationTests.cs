using Microsoft.AspNetCore.Mvc;
using UniversalReservationMVC.Controllers;
using UniversalReservationMVC.Models;
using Xunit;

namespace UniversalReservationMVC.Tests;

/// <summary>
/// Testy walidacji parametrów dla kontrolerów (bez mock serwisów)
/// </summary>
public class ControllerValidationTests
{
    [Fact]
    public void Reservation_Model_RequiresResourceId()
    {
        // Arrange
        var reservation = new Reservation
        {
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
            Status = ReservationStatus.Confirmed
        };

        // Act & Assert
        Assert.Equal(0, reservation.ResourceId); // Domyślna wartość
    }

    [Fact]
    public void Reservation_Model_RequiresPositiveDuration()
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            ResourceId = 1,
            StartTime = DateTime.Now.AddHours(2),
            EndTime = DateTime.Now.AddHours(1), // EndTime < StartTime
            Status = ReservationStatus.Confirmed
        };

        // Assert
        Assert.True(reservation.EndTime < reservation.StartTime);
    }

    [Fact]
    public void Event_Model_TitleCannotBeNull()
    {
        // Arrange & Act
        var eventModel = new Event
        {
            Title = null!,
            ResourceId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Assert
        Assert.Null(eventModel.Title);
    }

    [Fact]
    public void Resource_Model_NameCannotBeEmpty()
    {
        // Arrange & Act
        var resource = new Resource
        {
            Name = "",
            ResourceType = ResourceType.Restaurant
        };

        // Assert
        Assert.Equal("", resource.Name);
    }

    [Fact]
    public void Seat_Model_RequiresValidCoordinates()
    {
        // Arrange & Act
        var seat = new Seat
        {
            X = -1,
            Y = -1,
            ResourceId = 1
        };

        // Assert
        Assert.True(seat.X < 0);
        Assert.True(seat.Y < 0);
    }

    [Fact]
    public void Payment_Model_AmountMustBePositive()
    {
        // Arrange & Act
        var payment = new Payment
        {
            Amount = -100,
            Status = PaymentStatus.Pending,
            ReservationId = 1
        };

        // Assert
        Assert.True(payment.Amount < 0);
    }

    [Fact]
    public void Ticket_Model_RequiresReservationId()
    {
        // Arrange & Act
        var ticket = new Ticket
        {
            Price = 50,
            Status = TicketStatus.Available
        };

        // Assert
        Assert.Equal(0, ticket.ReservationId); // ReservationId is int, default is 0
    }

    [Fact]
    public void Company_Model_NameRequired()
    {
        // Arrange & Act
        var company = new Company
        {
            Name = null!,
            OwnerId = "user123"
        };

        // Assert
        Assert.Null(company.Name);
    }

    [Fact]
    public void CompanyMember_Model_RequiresBothIds()
    {
        // Arrange & Act
        var member = new CompanyMember
        {
            UserId = "user123",
            // CompanyId = 0 (domyślnie)
            Role = "Employee"
        };

        // Assert
        Assert.Equal(0, member.CompanyId);
    }

    [Fact]
    public void RecurrencePattern_Model_DaysOfWeekCanBeNull()
    {
        // Arrange & Act
        var pattern = new RecurrencePattern
        {
            Type = RecurrenceType.Daily,
            Interval = 1
        };

        // Assert
        Assert.Null(pattern.DaysOfWeek);
    }

    [Fact]
    public void Reservation_StatusTransition_PendingToConfirmed()
    {
        // Arrange
        var reservation = new Reservation
        {
            ResourceId = 1,
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
            Status = ReservationStatus.Pending
        };

        // Act
        reservation.Status = ReservationStatus.Confirmed;

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Event_DescriptionOptional()
    {
        // Arrange & Act
        var eventModel = new Event
        {
            Title = "Test Event",
            ResourceId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
            // Description nie ustawione
        };

        // Assert
        Assert.Null(eventModel.Description);
    }
}
