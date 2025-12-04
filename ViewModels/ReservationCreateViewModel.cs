using System.ComponentModel.DataAnnotations;

namespace UniversalReservationMVC.ViewModels
{
    public class ReservationCreateViewModel
    {
        public int ResourceId { get; set; }
        public int? SeatId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
