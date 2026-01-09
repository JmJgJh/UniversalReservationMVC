using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class ReservationEditViewModel : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ResourceId { get; set; }

        public int? SeatId { get; set; }

        public int? EventId { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        public DateTime EndTime { get; set; }

        public string? ResourceName { get; set; }
        public string? SeatLabel { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    "Data zakończenia musi być późniejsza niż data rozpoczęcia",
                    new[] { nameof(EndTime) });
            }

            if (StartTime < DateTime.Now.AddMinutes(-5))
            {
                yield return new ValidationResult(
                    "Nie można ustawić rezerwacji w przeszłości",
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
