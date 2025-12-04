using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class ResourceDetailsViewModel
    {
        public Resource? Resource { get; set; }
        public IEnumerable<Seat>? Seats { get; set; }
        public Event? CurrentEvent { get; set; }
    }
}
