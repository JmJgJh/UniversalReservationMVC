using UniversalReservationMVC.Models;
using System.Collections.Generic;

namespace UniversalReservationMVC.ViewModels
{
    public class UserDashboardViewModel
    {
        // Statystyki
        public int TotalReservationsCount { get; set; }
        public int ActiveReservationsCount { get; set; }
        public int CompletedReservationsCount { get; set; }
        public decimal TotalSpent { get; set; }

        // Nadchodz¹ce i przesz³e rezerwacje
        public List<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
        public List<Reservation> PastReservations { get; set; } = new List<Reservation>();

        // Lista firm, jeœli u¿ytkownik jest w³aœcicielem
        public List<Company> MyCompanies { get; set; } = new List<Company>();

        // Polecane wydarzenia
        public List<Event> UpcomingEvents { get; set; } = new List<Event>();

        // Wybrane miejsca
        public List<SelectedSeatViewModel> SelectedSeats { get; set; } = new List<SelectedSeatViewModel>();
    }
}