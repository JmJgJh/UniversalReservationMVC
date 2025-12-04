using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Haslo jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Zapamietaj mnie")]
        public bool RememberMe { get; set; }
    }
}
