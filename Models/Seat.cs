using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    // Seat corresponds to a coordinate in a seat map for a specific resource
    public class Seat
    {
        public int Id { get; set; }
        
        [Required]
        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;
        
        [Required]
        [Range(1, 1000)]
        public int X { get; set; }
        
        [Required]
        [Range(1, 1000)]
        public int Y { get; set; }
        
        // Optional label like "A1"
        [StringLength(20)]
        public string? Label { get; set; }
        
        // Row/Column for presentation (optional)
        [StringLength(10)]
        public string? Row { get; set; }
        
        [Range(1, 1000)]
        public int? Column { get; set; }
        
        public bool IsAvailable { get; set; } = true;
    }
}
