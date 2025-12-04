using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class GuestReservationViewModel
    {
        [Required(ErrorMessage = "E-mail lub telefon wymagany.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny e-mail.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Podaj poprawny numer telefonu.")]
        public string? Phone { get; set; }

        [Required]
        public int ResourceId { get; set; }

        public int? SeatId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
