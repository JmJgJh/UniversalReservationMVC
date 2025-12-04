using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    // A generic reservable resource (restaurant, cinema, office, conference room, theatre)
    public class Resource
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ResourceType ResourceType { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }

        // Optional layout metadata (seat map width/height)
        public int? SeatMapWidth { get; set; }
        public int? SeatMapHeight { get; set; }

        // Navigation property
        public ICollection<Seat>? Seats { get; set; }
    }
}
