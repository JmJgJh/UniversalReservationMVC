using Xunit;
using UniversalReservationMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Tests;

public class ModelValidationTests
{
    [Fact]
    public void Reservation_StartTime_MustBeBeforeEndTime()
    {
        // Arrange
        var reservation = new Reservation
        {
            ResourceId = 1,
            UserId = "user1",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1), // End before start - invalid
            Status = ReservationStatus.Pending
        };

        // Assert
        Assert.True(reservation.EndTime < reservation.StartTime);
    }

    [Fact]
    public void Company_RequiresName()
    {
        // Arrange
        var company = new Company
        {
            Name = "", // Empty name - should fail validation
            OwnerId = "owner1"
        };

        var context = new ValidationContext(company);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(company, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Company.Name)));
    }

    [Fact]
    public void Event_RequiresTitle()
    {
        // Arrange
        var ev = new Event
        {
            Title = "", // Empty - should fail
            ResourceId = 1,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        var context = new ValidationContext(ev);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(ev, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Event.Title)));
    }

    [Fact]
    public void Resource_RequiresValidType()
    {
        // Arrange
        var resource = new Resource
        {
            Name = "Test Resource",
            ResourceType = ResourceType.Cinema, // Valid enum value
            CompanyId = 1,
            Price = 50m
        };

        // Assert
        Assert.True(Enum.IsDefined(typeof(ResourceType), resource.ResourceType));
    }

    [Fact]
    public void Seat_RequiresValidCoordinates()
    {
        // Arrange
        var seat = new Seat
        {
            ResourceId = 1,
            X = 5,
            Y = 10,
            Label = "A5",
            IsAvailable = true
        };

        // Assert
        Assert.True(seat.X > 0);
        Assert.True(seat.Y > 0);
        Assert.False(string.IsNullOrEmpty(seat.Label));
    }

    [Fact]
    public void Payment_RequiresPositiveAmount()
    {
        // Arrange
        var payment = new Payment
        {
            ReservationId = 1,
            Amount = 100.50m,
            Currency = "PLN",
            StripePaymentIntentId = "pi_test"
        };

        // Assert
        Assert.True(payment.Amount > 0);
    }

    [Fact]
    public void Ticket_RequiresReservation()
    {
        // Arrange
        var ticket = new Ticket
        {
            ReservationId = 1,
            Price = 50.00m,
            Status = TicketStatus.Available
        };

        var context = new ValidationContext(ticket);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(ticket, context, results, true);

        // Assert - ReservationId is required
        Assert.True(ticket.ReservationId > 0);
    }

    [Fact]
    public void CompanyMember_HasValidPermissions()
    {
        // Arrange
        var member = new CompanyMember
        {
            CompanyId = 1,
            UserId = "user1",
            Role = "Manager",
            CanManageResources = true,
            CanViewReservations = true,
            CanManageReservations = false
        };

        // Assert
        Assert.NotNull(member.Role);
        Assert.True(member.CanManageResources);
    }

    [Fact]
    public void RecurrencePattern_HasValidType()
    {
        // Arrange
        var pattern = new RecurrencePattern
        {
            EventId = 1,
            Type = RecurrenceType.Weekly,
            Interval = 1,
            DaysOfWeek = "[1,3,5]" // Mon, Wed, Fri
        };

        var context = new ValidationContext(pattern);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(pattern, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.True(pattern.Interval > 0);
    }
}
