using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    static ReportService()
    {
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
        
        // Set EPPlus license (version 8+)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> GenerateReservationsPdfAsync(IEnumerable<Reservation> reservations, string title)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(5);

                        column.Item().Text(title)
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // ID
                                columns.RelativeColumn(2);    // Zasób
                                columns.RelativeColumn(2);    // Użytkownik
                                columns.RelativeColumn(1.5f); // Data od
                                columns.RelativeColumn(1.5f); // Data do
                                columns.RelativeColumn(1);    // Status
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("ID").Bold();
                                header.Cell().Element(CellStyle).Text("Zasób").Bold();
                                header.Cell().Element(CellStyle).Text("Użytkownik").Bold();
                                header.Cell().Element(CellStyle).Text("Od").Bold();
                                header.Cell().Element(CellStyle).Text("Do").Bold();
                                header.Cell().Element(CellStyle).Text("Status").Bold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Medium)
                                        .PaddingVertical(5);
                                }
                            });

                            // Content
                            foreach (var reservation in reservations)
                            {
                                var userDisplay = reservation.User != null
                                    ? $"{reservation.User.FirstName} {reservation.User.LastName}"
                                    : reservation.GuestEmail ?? "Gość";

                                table.Cell().Element(CellStyle).Text(reservation.Id.ToString());
                                table.Cell().Element(CellStyle).Text(reservation.Resource?.Name ?? "N/A");
                                table.Cell().Element(CellStyle).Text(userDisplay);
                                table.Cell().Element(CellStyle).Text(reservation.StartTime.ToString("dd.MM HH:mm"));
                                table.Cell().Element(CellStyle).Text(reservation.EndTime.ToString("dd.MM HH:mm"));
                                table.Cell().Element(CellStyle).Text(GetStatusText(reservation.Status));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Strona ");
                        text.CurrentPageNumber();
                        text.Span(" z ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });

        static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Universal Reservation System")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().Text("Raport rezerwacji")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }
    }

    public async Task<byte[]> GenerateReservationsExcelAsync(IEnumerable<Reservation> reservations, string sheetName)
    {
        return await Task.Run(() =>
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Header
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Zasób";
            worksheet.Cells[1, 3].Value = "Użytkownik";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "Telefon";
            worksheet.Cells[1, 6].Value = "Data od";
            worksheet.Cells[1, 7].Value = "Data do";
            worksheet.Cells[1, 8].Value = "Status";
            worksheet.Cells[1, 9].Value = "Opłacone";
            worksheet.Cells[1, 10].Value = "Data utworzenia";

            using (var range = worksheet.Cells[1, 1, 1, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            // Data
            int row = 2;
            foreach (var reservation in reservations)
            {
                var userDisplay = reservation.User != null
                    ? $"{reservation.User.FirstName} {reservation.User.LastName}"
                    : "Gość";

                worksheet.Cells[row, 1].Value = reservation.Id;
                worksheet.Cells[row, 2].Value = reservation.Resource?.Name ?? "N/A";
                worksheet.Cells[row, 3].Value = userDisplay;
                worksheet.Cells[row, 4].Value = reservation.User?.Email ?? reservation.GuestEmail ?? "";
                worksheet.Cells[row, 5].Value = reservation.GuestPhone ?? "";
                worksheet.Cells[row, 6].Value = reservation.StartTime;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd.mm.yyyy hh:mm";
                worksheet.Cells[row, 7].Value = reservation.EndTime;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "dd.mm.yyyy hh:mm";
                worksheet.Cells[row, 8].Value = GetStatusText(reservation.Status);
                worksheet.Cells[row, 9].Value = reservation.IsPaid ? "Tak" : "Nie";
                worksheet.Cells[row, 10].Value = reservation.CreatedAt;
                worksheet.Cells[row, 10].Style.Numberformat.Format = "dd.mm.yyyy hh:mm";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        });
    }

    public async Task<byte[]> GenerateCompanySummaryPdfAsync(Company company, DateTime startDate, DateTime endDate)
    {
        var reservations = await _context.Reservations            .AsNoTracking()            .Include(r => r.Resource)
            .Include(r => r.User)
            .Where(r => r.Resource!.CompanyId == company.Id
                && r.StartTime >= startDate
                && r.StartTime <= endDate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var payments = await _context.Payments            .AsNoTracking()            .Include(p => p.Reservation)
                .ThenInclude(r => r!.Resource)
            .Where(p => p.Reservation!.Resource!.CompanyId == company.Id
                && p.Status == PaymentStatus.Succeeded
                && p.PaidAt >= startDate
                && p.PaidAt <= endDate)
            .ToListAsync();

        var totalRevenue = payments.Sum(p => p.Amount);
        var totalReservations = reservations.Count;
        var confirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed);

        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(company.Name)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text($"Raport finansowy: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(10);

                        // Summary stats
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(StatBox).Column(col =>
                            {
                                col.Item().Text("Przychód całkowity").Bold().FontSize(12);
                                col.Item().Text($"{totalRevenue:C2}").FontSize(20).FontColor(Colors.Green.Darken2);
                            });

                            row.RelativeItem().Element(StatBox).Column(col =>
                            {
                                col.Item().Text("Rezerwacje").Bold().FontSize(12);
                                col.Item().Text(totalReservations.ToString()).FontSize(20).FontColor(Colors.Blue.Medium);
                            });

                            row.RelativeItem().Element(StatBox).Column(col =>
                            {
                                col.Item().Text("Potwierdzone").Bold().FontSize(12);
                                col.Item().Text(confirmedReservations.ToString()).FontSize(20).FontColor(Colors.Orange.Medium);
                            });

                            static IContainer StatBox(IContainer container)
                            {
                                return container
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(10);
                            }
                        });

                        column.Item().PaddingTop(20).Text("Szczegóły płatności")
                            .FontSize(14)
                            .Bold();

                        if (payments.Any())
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);  // Zasób
                                    columns.RelativeColumn(1);  // Data płatności
                                    columns.RelativeColumn(1);  // Kwota
                                    columns.RelativeColumn(1);  // Waluta
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Zasób").Bold();
                                    header.Cell().Element(CellStyle).Text("Data płatności").Bold();
                                    header.Cell().Element(CellStyle).Text("Kwota").Bold();
                                    header.Cell().Element(CellStyle).Text("Waluta").Bold();
                                });

                                foreach (var payment in payments)
                                {
                                    table.Cell().Element(CellStyle).Text(payment.Reservation?.Resource?.Name ?? "N/A");
                                    table.Cell().Element(CellStyle).Text(payment.PaidAt?.ToString("dd.MM.yyyy HH:mm") ?? "-");
                                    table.Cell().Element(CellStyle).Text($"{payment.Amount:F2}");
                                    table.Cell().Element(CellStyle).Text(payment.Currency);
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            });
                        }
                        else
                        {
                            column.Item().Text("Brak płatności w wybranym okresie")
                                .FontColor(Colors.Grey.Medium)
                                .Italic();
                        }
                    });

                    page.Footer().AlignCenter().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<byte[]> GenerateRevenueReportExcelAsync(Company company, DateTime startDate, DateTime endDate)
    {
        var payments = await _context.Payments
            .Include(p => p.Reservation)
                .ThenInclude(r => r!.Resource)
            .Include(p => p.Reservation)
                .ThenInclude(r => r!.User)
            .Where(p => p.Reservation!.Resource!.CompanyId == company.Id
                && p.PaidAt >= startDate
                && p.PaidAt <= endDate)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        return await Task.Run(() =>
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Raport przychodów");

            // Title
            worksheet.Cells[1, 1].Value = $"{company.Name} - Raport przychodów";
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            worksheet.Cells[2, 1].Value = $"Okres: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            worksheet.Cells[2, 1].Style.Font.Size = 12;

            // Summary
            var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Succeeded).Sum(p => p.Amount);
            var refundedAmount = payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.Amount);

            worksheet.Cells[4, 1].Value = "Przychód całkowity:";
            worksheet.Cells[4, 1].Style.Font.Bold = true;
            worksheet.Cells[4, 2].Value = totalRevenue;
            worksheet.Cells[4, 2].Style.Numberformat.Format = "#,##0.00 [$zł-415]";
            worksheet.Cells[4, 2].Style.Font.Bold = true;
            worksheet.Cells[4, 2].Style.Font.Color.SetColor(System.Drawing.Color.Green);

            worksheet.Cells[5, 1].Value = "Zwroty:";
            worksheet.Cells[5, 1].Style.Font.Bold = true;
            worksheet.Cells[5, 2].Value = refundedAmount;
            worksheet.Cells[5, 2].Style.Numberformat.Format = "#,##0.00 [$zł-415]";
            worksheet.Cells[5, 2].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Data table header
            int startRow = 7;
            worksheet.Cells[startRow, 1].Value = "ID płatności";
            worksheet.Cells[startRow, 2].Value = "Data płatności";
            worksheet.Cells[startRow, 3].Value = "Zasób";
            worksheet.Cells[startRow, 4].Value = "Użytkownik";
            worksheet.Cells[startRow, 5].Value = "Kwota";
            worksheet.Cells[startRow, 6].Value = "Waluta";
            worksheet.Cells[startRow, 7].Value = "Status";
            worksheet.Cells[startRow, 8].Value = "Stripe Payment ID";

            using (var range = worksheet.Cells[startRow, 1, startRow, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            // Data
            int row = startRow + 1;
            foreach (var payment in payments)
            {
                var userDisplay = payment.Reservation?.User != null
                    ? $"{payment.Reservation.User.FirstName} {payment.Reservation.User.LastName}"
                    : payment.Reservation?.GuestEmail ?? "Gość";

                worksheet.Cells[row, 1].Value = payment.Id;
                worksheet.Cells[row, 2].Value = payment.PaidAt ?? payment.CreatedAt;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd.mm.yyyy hh:mm";
                worksheet.Cells[row, 3].Value = payment.Reservation?.Resource?.Name ?? "N/A";
                worksheet.Cells[row, 4].Value = userDisplay;
                worksheet.Cells[row, 5].Value = payment.Amount;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00 [$zł-415]";
                worksheet.Cells[row, 6].Value = payment.Currency;
                worksheet.Cells[row, 7].Value = GetPaymentStatusText(payment.Status);
                worksheet.Cells[row, 8].Value = payment.StripePaymentIntentId;

                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        });
    }

    private static string GetStatusText(ReservationStatus status)
    {
        return status switch
        {
            ReservationStatus.Pending => "Oczekująca",
            ReservationStatus.Confirmed => "Potwierdzona",
            ReservationStatus.Cancelled => "Anulowana",
            ReservationStatus.Completed => "Zakończona",
            _ => status.ToString()
        };
    }

    private static string GetPaymentStatusText(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Oczekująca",
            PaymentStatus.Processing => "Przetwarzana",
            PaymentStatus.Succeeded => "Opłacona",
            PaymentStatus.Failed => "Niepowodzenie",
            PaymentStatus.Refunded => "Zwrócona",
            PaymentStatus.Cancelled => "Anulowana",
            _ => status.ToString()
        };
    }
}
