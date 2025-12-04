using Microsoft.AspNetCore.Identity;

namespace UniversalReservationMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }
}
