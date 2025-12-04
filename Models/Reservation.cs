using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        // User or guest
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }

        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }

        public int? SeatId { get; set; }
        public Seat? Seat { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Optional link to event
        public int? EventId { get; set; }
        public Event? Event { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
