using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class SeatMapViewModel
    {
        public Resource? Resource { get; set; }
        public IEnumerable<Seat>? Seats { get; set; }
    }
}
