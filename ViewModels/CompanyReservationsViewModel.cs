using System;
using System.Collections.Generic;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.ViewModels
{
    public class CompanyReservationsViewModel
    {
        public List<Reservation> Reservations { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CancelledCount { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Status { get; set; }
    }
}
