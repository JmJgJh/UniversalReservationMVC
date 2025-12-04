using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class TicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public Reservation? Reservation { get; set; }
    }
}
