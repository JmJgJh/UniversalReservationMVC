using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalReservationMVC.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;
        
        [Required]
        [Range(0, 999999.99, ErrorMessage = "Cena musi być między 0 a 999999.99")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Available;
        
        [StringLength(100)]
        public string? PurchaseReference { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PurchasedAt { get; set; }
    }
}
