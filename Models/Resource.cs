using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalReservationMVC.Models
{
    // A generic reservable resource (restaurant, cinema, office, conference room, theatre)
    public class Resource
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Nazwa zasobu jest wymagana")]
        [StringLength(200, ErrorMessage = "Nazwa nie może przekraczać 200 znaków")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public ResourceType ResourceType { get; set; }
        
        [StringLength(500, ErrorMessage = "Lokalizacja nie może przekraczać 500 znaków")]
        public string? Location { get; set; }
        
        [StringLength(2000, ErrorMessage = "Opis nie może przekraczać 2000 znaków")]
        public string? Description { get; set; }

        // Optional layout metadata (seat map width/height)
        [Range(1, 100, ErrorMessage = "Szerokość musi być między 1 a 100")]
        public int? SeatMapWidth { get; set; }
        
        [Range(1, 100, ErrorMessage = "Wysokość musi być między 1 a 100")]
        public int? SeatMapHeight { get; set; }

        // Total capacity of the resource (if not seat-based)
        [Range(1, 10000, ErrorMessage = "Pojemność musi być między 1 a 10000")]
        public int? Capacity { get; set; }

        // Pricing
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 999999.99, ErrorMessage = "Cena musi być między 0 a 999999.99")]
        public decimal Price { get; set; } = 0;

        // Working hours (JSON: {"monday": {"open": "09:00", "close": "17:00"}, ...})
        public string? WorkingHours { get; set; }

        // Company association (resource belongs to a company owner)
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
