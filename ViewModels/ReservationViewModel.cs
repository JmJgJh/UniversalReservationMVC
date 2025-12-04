using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class ReservationViewModel
    {
        public int ResourceId { get; set; }
        public int? SeatId { get; set; }

        [Required(ErrorMessage = "E-mail lub telefon wymagany dla rezerwacji bez konta")]
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }
}
