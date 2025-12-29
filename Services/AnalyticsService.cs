using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.ViewModels;

namespace UniversalReservationMVC.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AnalyticsDashboardViewModel> GetCompanyAnalyticsAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservations = await _context.Reservations
                    .Include(r => r.Resource)
                    .Where(r => r.Resource != null && r.Resource.CompanyId == companyId
                        && r.StartTime >= startDate && r.StartTime <= endDate)
                    .ToListAsync();

                var model = new AnalyticsDashboardViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalReservations = reservations.Count,
                    ConfirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
                    CancelledReservations = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                    TotalRevenue = reservations.Where(r => r.Resource != null).Sum(r => r.Resource!.Price),
                    PaidRevenue = reservations.Where(r => r.IsPaid && r.Resource != null).Sum(r => r.Resource!.Price)
                };

                // Calculate average occupancy
                var resources = await _context.Resources
                    .Where(r => r.CompanyId == companyId)
                    .ToListAsync();

                if (resources.Any())
                {
                    var totalHours = resources.Count * (endDate - startDate).TotalHours;
                    var bookedHours = reservations
                        .Where(r => r.Status != ReservationStatus.Cancelled)
                        .Sum(r => (r.EndTime - r.StartTime).TotalHours);
                    model.AverageOccupancy = totalHours > 0 ? (bookedHours / totalHours) * 100 : 0;
                }

                // Load chart data
                model.OccupancyData = await GetOccupancyDataAsync(companyId, startDate, endDate);
                model.RevenueData = await GetRevenueDataAsync(companyId, startDate, endDate);
                model.PopularResources = await GetPopularResourcesAsync(companyId, startDate, endDate);
                model.BookingPatterns = await GetBookingPatternsAsync(companyId, startDate, endDate);
                
                model.StatusDistribution = new StatusDistributionData
                {
                    Confirmed = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
                    Pending = reservations.Count(r => r.Status == ReservationStatus.Pending),
                    Cancelled = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                    Completed = reservations.Count(r => r.Status == ReservationStatus.Completed)
                };

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<OccupancyChartData> GetOccupancyDataAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var data = new OccupancyChartData();
            var resources = await _context.Resources
                .Where(r => r.CompanyId == companyId)
                .CountAsync();

            if (resources == 0) return data;

            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);
                var dayReservations = await _context.Reservations
                    .Include(r => r.Resource)
                    .Where(r => r.Resource != null && r.Resource.CompanyId == companyId
                        && r.Status != ReservationStatus.Cancelled
                        && r.StartTime < nextDate && r.EndTime > currentDate)
                    .ToListAsync();

                var totalHours = resources * 24.0; // Assume 24h availability
                var bookedHours = dayReservations.Sum(r =>
                {
                    var start = r.StartTime < currentDate ? currentDate : r.StartTime;
                    var end = r.EndTime > nextDate ? nextDate : r.EndTime;
                    return (end - start).TotalHours;
                });

                var occupancy = totalHours > 0 ? (bookedHours / totalHours) * 100 : 0;

                data.Labels.Add(currentDate.ToString("dd MMM"));
                data.Values.Add(Math.Round(occupancy, 2));

                currentDate = nextDate;
            }

            return data;
        }

        public async Task<RevenueChartData> GetRevenueDataAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var data = new RevenueChartData();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);
                var dayReservations = await _context.Reservations
                    .Include(r => r.Resource)
                    .Where(r => r.Resource != null && r.Resource.CompanyId == companyId
                        && r.StartTime >= currentDate && r.StartTime < nextDate)
                    .ToListAsync();

                var totalRevenue = dayReservations.Where(r => r.Resource != null).Sum(r => r.Resource!.Price);
                var paidRevenue = dayReservations.Where(r => r.IsPaid && r.Resource != null).Sum(r => r.Resource!.Price);

                data.Labels.Add(currentDate.ToString("dd MMM"));
                data.TotalRevenue.Add(totalRevenue);
                data.PaidRevenue.Add(paidRevenue);

                currentDate = nextDate;
            }

            return data;
        }

        public async Task<List<ResourcePopularityData>> GetPopularResourcesAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var resourceStats = await _context.Reservations
                .Include(r => r.Resource)
                .Where(r => r.Resource != null && r.Resource.CompanyId == companyId
                    && r.StartTime >= startDate && r.StartTime <= endDate
                    && r.Status != ReservationStatus.Cancelled)
                .GroupBy(r => new { r.ResourceId, r.Resource!.Name, r.Resource.Price })
                .Select(g => new ResourcePopularityData
                {
                    ResourceName = g.Key.Name,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(r => g.Key.Price)
                })
                .OrderByDescending(r => r.BookingCount)
                .Take(10)
                .ToListAsync();

            return resourceStats;
        }

        public async Task<BookingPatternsData> GetBookingPatternsAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Resource)
                .Where(r => r.Resource != null && r.Resource.CompanyId == companyId
                    && r.StartTime >= startDate && r.StartTime <= endDate
                    && r.Status != ReservationStatus.Cancelled)
                .ToListAsync();

            var data = new BookingPatternsData();

            // Initialize arrays
            data.ByDayOfWeek = new List<int> { 0, 0, 0, 0, 0, 0, 0 }; // Mon-Sun
            data.ByHourOfDay = Enumerable.Repeat(0, 24).ToList(); // 0-23

            foreach (var reservation in reservations)
            {
                // Day of week (Monday = 0)
                var dayIndex = ((int)reservation.StartTime.DayOfWeek + 6) % 7;
                data.ByDayOfWeek[dayIndex]++;

                // Hour of day
                data.ByHourOfDay[reservation.StartTime.Hour]++;
            }

            return data;
        }
    }
}
