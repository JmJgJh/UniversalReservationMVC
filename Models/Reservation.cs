using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        // User or guest
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        [EmailAddress(ErrorMessage = "Podaj poprawny adres email")]
        [StringLength(255)]
        public string? GuestEmail { get; set; }
        
        [Phone(ErrorMessage = "Podaj poprawny numer telefonu")]
        [StringLength(50)]
        public string? GuestPhone { get; set; }

        [Required]
        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public int? SeatId { get; set; }
        public Seat? Seat { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Optional link to event
        public int? EventId { get; set; }
        public Event? Event { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
