using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imie jest wymagane.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Haslo jest wymagane.")]
        [StringLength(100, ErrorMessage = "Haslo musi byc co najmniej {2} znakow.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasla nie zgadzaja sie.")]
        public string ConfirmPassword { get; set; }
    }
}
