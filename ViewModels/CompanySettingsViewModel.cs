using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class CompanySettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa firmy jest wymagana")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Nazwa firmy musi zawierać od 2 do 200 znaków")]
        [Display(Name = "Nazwa firmy")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [StringLength(200)]
        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [StringLength(20)]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Strona WWW")]
        public string? Website { get; set; }

        [Display(Name = "Logo")]
        public IFormFile? LogoFile { get; set; }

        public string? LogoUrl { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Nieprawidłowy format koloru (np. #FF5733)")]
        [Display(Name = "Kolor główny")]
        public string? PrimaryColor { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Nieprawidłowy format koloru (np. #FF5733)")]
        [Display(Name = "Kolor dodatkowy")]
        public string? SecondaryColor { get; set; }
    }
}
