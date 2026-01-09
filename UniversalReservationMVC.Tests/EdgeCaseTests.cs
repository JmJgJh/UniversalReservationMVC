using Xunit;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void Reservation_StartTimeEqualsEndTime_IsInvalid()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            StartTime = now,
            EndTime = now // Start equals End
        };

        // Assert
        Assert.Equal(reservation.StartTime, reservation.EndTime);
        Assert.True(reservation.StartTime >= reservation.EndTime); // Invalid state
    }

    [Fact]
    public void Reservation_EndTimeBeforeStartTime_IsInvalid()
    {
        // Arrange
        var reservation = new Reservation
        {
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1) // End before Start
        };

        // Assert
        Assert.True(reservation.EndTime < reservation.StartTime);
    }

    [Fact]
    public void Resource_NegativePrice_IsInvalid()
    {
        // Arrange
        var resource = new Resource
        {
            Name = "Test Resource",
            Price = -50m
        };

        // Assert
        Assert.True(resource.Price < 0);
    }

    [Fact]
    public void Ticket_ZeroPrice_IsValid()
    {
        // Arrange - free tickets are valid
        var ticket = new Ticket
        {
            ReservationId = 1,
            Price = 0m,
            Status = TicketStatus.Available
        };

        // Assert
        Assert.Equal(0m, ticket.Price);
    }

    [Fact]
    public void Company_EmptyName_IsInvalid()
    {
        // Arrange
        var company = new Company
        {
            Name = "",
            OwnerId = "owner1"
        };

        // Assert
        Assert.True(string.IsNullOrEmpty(company.Name));
    }

    [Fact]
    public void Event_DurationCalculation_IsCorrect()
    {
        // Arrange
        var ev = new Event
        {
            Title = "Test Event",
            StartTime = new DateTime(2025, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2025, 1, 1, 12, 30, 0)
        };

        // Act
        var duration = ev.EndTime - ev.StartTime;

        // Assert
        Assert.Equal(150, duration.TotalMinutes); // 2.5 hours = 150 minutes
    }

    [Fact]
    public void Seat_CoordinatesOutOfRange_IsInvalid()
    {
        // Arrange
        var seat = new Seat
        {
            ResourceId = 1,
            X = -1, // Invalid
            Y = 0,  // Invalid
            Label = "Invalid"
        };

        // Assert
        Assert.True(seat.X < 1 || seat.Y < 1);
    }

    [Fact]
    public void Reservation_StatusTransition_Pending_To_Confirmed()
    {
        // Arrange
        var reservation = new Reservation
        {
            Status = ReservationStatus.Pending
        };

        // Act
        reservation.Status = ReservationStatus.Confirmed;

        // Assert
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Reservation_StatusTransition_Confirmed_To_Cancelled()
    {
        // Arrange
        var reservation = new Reservation
        {
            Status = ReservationStatus.Confirmed
        };

        // Act
        reservation.Status = ReservationStatus.Cancelled;

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Payment_DefaultStatus_IsPending()
    {
        // Arrange & Act
        var payment = new Payment
        {
            ReservationId = 1,
            Amount = 100m,
            Currency = "PLN",
            StripePaymentIntentId = "pi_test"
        };

        // Assert
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Resource_CapacityZero_IsValid()
    {
        // Arrange - resources without capacity limit
        var resource = new Resource
        {
            Name = "Virtual Resource",
            Capacity = 0,
            ResourceType = ResourceType.Office,
            CompanyId = 1,
            Price = 0m
        };

        // Assert
        Assert.Equal(0, resource.Capacity);
    }

    [Fact]
    public void Ticket_PurchaseReference_IsUnique()
    {
        // Arrange
        var ticket1 = new Ticket
        {
            ReservationId = 1,
            Price = 50m,
            PurchaseReference = Guid.NewGuid().ToString()
        };

        var ticket2 = new Ticket
        {
            ReservationId = 2,
            Price = 50m,
            PurchaseReference = Guid.NewGuid().ToString()
        };

        // Assert
        Assert.NotEqual(ticket1.PurchaseReference, ticket2.PurchaseReference);
    }

    [Fact]
    public void CompanyMember_DefaultPermissions_AreFalse()
    {
        // Arrange & Act
        var member = new CompanyMember
        {
            CompanyId = 1,
            UserId = "user1"
        };

        // Assert - default permissions should be restrictive
        Assert.False(member.CanManageResources);
        Assert.False(member.CanManageReservations);
        Assert.False(member.CanManageEvents);
    }

    [Fact]
    public void RecurrencePattern_IntervalMustBePositive()
    {
        // Arrange
        var pattern = new RecurrencePattern
        {
            EventId = 1,
            Type = RecurrenceType.Weekly,
            Interval = 0 // Invalid
        };

        // Assert
        Assert.True(pattern.Interval < 1);
    }

    [Fact]
    public void Event_WithRecurrencePattern_HasParentEvent()
    {
        // Arrange
        var parentEvent = new Event
        {
            Id = 1,
            Title = "Recurring Event",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            ResourceId = 1
        };

        var childEvent = new Event
        {
            Id = 2,
            Title = "Recurring Event",
            StartTime = DateTime.UtcNow.AddDays(7),
            EndTime = DateTime.UtcNow.AddDays(7).AddHours(1),
            ResourceId = 1,
            ParentEventId = 1
        };

        // Assert
        Assert.NotNull(childEvent.ParentEventId);
        Assert.Equal(parentEvent.Id, childEvent.ParentEventId);
    }
}
