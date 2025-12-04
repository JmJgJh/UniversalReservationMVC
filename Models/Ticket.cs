using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Available;
        public string? PurchaseReference { get; set; }
    }
}
