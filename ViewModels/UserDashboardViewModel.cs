using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class UserDashboardViewModel
    {
        public List<Reservation> UpcomingReservations { get; set; } = new();
        public List<Reservation> PastReservations { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();
        public int TotalReservationsCount { get; set; }
        public int ActiveReservationsCount { get; set; }
        public int CompletedReservationsCount { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Company> MyCompanies { get; set; } = new();
    }
}
