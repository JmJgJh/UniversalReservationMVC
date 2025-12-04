using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    // Seat corresponds to a coordinate in a seat map for a specific resource
    public class Seat
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        // Optional label like "A1"
        public string? Label { get; set; }
        // Row/Column for presentation (optional)
        public string? Row { get; set; }
        public int? Column { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
