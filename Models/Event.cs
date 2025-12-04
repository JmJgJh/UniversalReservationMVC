using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.Models
{
    public class Event
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
