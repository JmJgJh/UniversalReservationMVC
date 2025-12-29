using UniversalReservationMVC.ViewModels;

namespace UniversalReservationMVC.Services
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDashboardViewModel> GetCompanyAnalyticsAsync(int companyId, DateTime startDate, DateTime endDate);
        Task<OccupancyChartData> GetOccupancyDataAsync(int companyId, DateTime startDate, DateTime endDate);
        Task<RevenueChartData> GetRevenueDataAsync(int companyId, DateTime startDate, DateTime endDate);
        Task<List<ResourcePopularityData>> GetPopularResourcesAsync(int companyId, DateTime startDate, DateTime endDate);
        Task<BookingPatternsData> GetBookingPatternsAsync(int companyId, DateTime startDate, DateTime endDate);
    }
}
