using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imie jest wymagane.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Haslo jest wymagane.")]
        [StringLength(100, ErrorMessage = "Haslo musi byc co najmniej {2} znakow.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasla nie zgadzaja sie.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Account type: "user" or "owner"
        [Required(ErrorMessage = "Typ konta jest wymagany.")]
        public string AccountType { get; set; } = "user";

        // Company info (required if AccountType == "owner")
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Nazwa firmy musi zawierać od 2 do 200 znaków")]
        public string? CompanyName { get; set; }

        [StringLength(200)]
        public string? CompanyAddress { get; set; }

        [StringLength(20)]
        public string? CompanyPhone { get; set; }

        [StringLength(100)]
        public string? CompanyEmail { get; set; }

        [StringLength(1000)]
        public string? CompanyDescription { get; set; }
    }
}
