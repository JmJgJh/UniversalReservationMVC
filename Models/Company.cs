using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa firmy jest wymagana")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Nazwa firmy musi zawierać od 2 do 200 znaków")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Website { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser? Owner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(7)]
        public string? PrimaryColor { get; set; }

        [StringLength(7)]
        public string? SecondaryColor { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Resource>? Resources { get; set; } = new List<Resource>();

        /// <summary>
        /// Członkowie firmy (many-to-many relationship z ApplicationUser)
        /// </summary>
        public virtual ICollection<CompanyMember>? Members { get; set; } = new List<CompanyMember>();
    }
}
