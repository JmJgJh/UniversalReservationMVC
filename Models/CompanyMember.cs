using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalReservationMVC.Models
{
    /// <summary>
    /// Model łączenia użytkownika z firmą (many-to-many)
    /// Użytkownik może być przypisany do wielu firm
    /// </summary>
    public class CompanyMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Rola użytkownika w firmie (Manager, Employee, Viewer itp.)
        /// </summary>
        [StringLength(50)]
        public string Role { get; set; } = "Employee";

        /// <summary>
        /// Czy użytkownik ma dostęp do zarządzania zasobami
        /// </summary>
        public bool CanManageResources { get; set; } = false;

        /// <summary>
        /// Czy użytkownik ma dostęp do przeglądania rezerwacji
        /// </summary>
        public bool CanViewReservations { get; set; } = true;

        /// <summary>
        /// Czy użytkownik ma dostęp do zarządzania rezerwacjami
        /// </summary>
        public bool CanManageReservations { get; set; } = false;

        /// <summary>
        /// Czy użytkownik ma dostęp do zarządzania wydarzeniami
        /// </summary>
        public bool CanManageEvents { get; set; } = false;

        /// <summary>
        /// Czy użytkownik ma dostęp do analityki
        /// </summary>
        public bool CanViewAnalytics { get; set; } = false;

        /// <summary>
        /// Czy użytkownik ma dostęp do eksportu raportów
        /// </summary>
        public bool CanExportReports { get; set; } = false;

        /// <summary>
        /// Czy użytkownik ma dostęp do zarządzania członkami
        /// </summary>
        public bool CanManageMembers { get; set; } = false;

        /// <summary>
        /// Data dołączenia do firmy
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ostatniej aktywności
        /// </summary>
        public DateTime? LastActivityAt { get; set; }

        /// <summary>
        /// Czy członek jest aktywny
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
