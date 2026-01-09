namespace UniversalReservationMVC.ViewModels
{
    public class AnalyticsDashboardViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Summary stats
        public int TotalReservations { get; set; }
        public int ConfirmedReservations { get; set; }
        public int CancelledReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidRevenue { get; set; }
        public double AverageOccupancy { get; set; }
        
        // Chart data
        public OccupancyChartData OccupancyData { get; set; } = new();
        public RevenueChartData RevenueData { get; set; } = new();
        public List<ResourcePopularityData> PopularResources { get; set; } = new();
        public BookingPatternsData BookingPatterns { get; set; } = new();
        public StatusDistributionData StatusDistribution { get; set; } = new();
    }

    public class OccupancyChartData
    {
        public List<string> Labels { get; set; } = new(); // Dates
        public List<double> Values { get; set; } = new(); // Occupancy percentages
    }

    public class RevenueChartData
    {
        public List<string> Labels { get; set; } = new(); // Dates
        public List<decimal> TotalRevenue { get; set; } = new();
        public List<decimal> PaidRevenue { get; set; } = new();
    }

    public class ResourcePopularityData
    {
        public string ResourceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class BookingPatternsData
    {
        public List<int> ByDayOfWeek { get; set; } = new(); // Mon-Sun (7 values)
        public List<int> ByHourOfDay { get; set; } = new(); // 0-23 (24 values)
    }

    public class StatusDistributionData
    {
        public int Confirmed { get; set; }
        public int Pending { get; set; }
        public int Cancelled { get; set; }
        public int Completed { get; set; }
    }
}
