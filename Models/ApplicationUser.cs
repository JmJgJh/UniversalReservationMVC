using Microsoft.AspNetCore.Identity;

namespace UniversalReservationMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Firmy, do których użytkownik jest przypisany (many-to-many relationship)
        /// </summary>
        public virtual ICollection<CompanyMember>? CompanyMemberships { get; set; } = new List<CompanyMember>();
    }
}
