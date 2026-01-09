using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, ErrorMessage = "Hasło musi być co najmniej {2} znaków długie.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie zgadzają się.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Account type: "user" or "owner"
        [Required(ErrorMessage = "Typ konta jest wymagany.")]
        [Display(Name = "Typ konta")]
        public string AccountType { get; set; } = "user";

        // Company info (required if AccountType == "owner")
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Nazwa firmy musi zawierać od 2 do 200 znaków")]
        [Display(Name = "Nazwa firmy")]
        public string? CompanyName { get; set; }

        [StringLength(200)]
        [Display(Name = "Adres")]
        public string? CompanyAddress { get; set; }

        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string? CompanyPhone { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
        [Display(Name = "Email firmy")]
        public string? CompanyEmail { get; set; }

        [StringLength(1000)]
        [Display(Name = "Opis firmy")]
        public string? CompanyDescription { get; set; }
    }
}
