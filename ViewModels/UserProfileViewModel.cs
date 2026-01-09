using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class UserProfileViewModel
    {
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }
    }
}
