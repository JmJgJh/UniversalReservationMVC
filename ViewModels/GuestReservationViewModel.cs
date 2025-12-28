using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class GuestReservationViewModel : IValidatableObject
    {
        [EmailAddress(ErrorMessage = "Podaj poprawny e-mail.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Podaj poprawny numer telefonu.")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Required]
        public int ResourceId { get; set; }

        public int? SeatId { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Phone))
            {
                yield return new ValidationResult(
                    "Musisz podać e-mail lub numer telefonu",
                    new[] { nameof(Email), nameof(Phone) });
            }

            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    "Data zakończenia musi być późniejsza niż data rozpoczęcia",
                    new[] { nameof(EndTime) });
            }

            if (StartTime < DateTime.Now.AddMinutes(-5))
            {
                yield return new ValidationResult(
                    "Nie można utworzyć rezerwacji w przeszłości",
                    new[] { nameof(StartTime) });
            }

            if ((EndTime - StartTime).TotalHours > 24)
            {
                yield return new ValidationResult(
                    "Maksymalny czas rezerwacji to 24 godziny",
                    new[] { nameof(EndTime) });
            }
        }
    }
}
