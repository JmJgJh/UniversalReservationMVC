using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tytuł wydarzenia jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł nie może przekraczać 200 znaków")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000, ErrorMessage = "Opis nie może przekraczać 2000 znaków")]
        public string? Description { get; set; }
        
        [Required]
        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
    }
}
