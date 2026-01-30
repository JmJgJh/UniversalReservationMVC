using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class ReservationCreateViewModel : IValidatableObject
    {
        public int ResourceId { get; set; }
        public int? SeatId { get; set; }
        public int? EventId { get; set; }
        
        /// <summary>URL, na który powrócić po rezerwacji (np. strona partnera).</summary>
        public string? ReturnUrl { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

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
