using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services;

public interface IReportService
{
    Task<byte[]> GenerateReservationsPdfAsync(IEnumerable<Reservation> reservations, string title);
    Task<byte[]> GenerateReservationsExcelAsync(IEnumerable<Reservation> reservations, string sheetName);
    Task<byte[]> GenerateCompanySummaryPdfAsync(Company company, DateTime startDate, DateTime endDate);
    Task<byte[]> GenerateRevenueReportExcelAsync(Company company, DateTime startDate, DateTime endDate);
}
