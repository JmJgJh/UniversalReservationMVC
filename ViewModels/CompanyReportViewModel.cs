using System;
using System.Collections.Generic;

namespace UniversalReservationMVC.ViewModels
{
    public class CompanyReportViewModel
    {
        public int TotalResources { get; set; }
        public int TotalSeats { get; set; }
        public int TotalReservations { get; set; }
        public int UpcomingReservations { get; set; }
        public double OccupancyRate { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<ResourceReportRow> Resources { get; set; } = new();
    }

    public class ResourceReportRow
    {
        public int ResourceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public int ReservationCount { get; set; }
        public double Occupancy { get; set; }
        public decimal Revenue { get; set; }
    }
}
