namespace UniversalReservationMVC.Services;

public interface IEmailService
{
    Task SendReservationConfirmationAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string? seatInfo, string companyName);
    Task SendReservationCancellationAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string companyName);
    Task SendReservationReminderAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string? seatInfo, string companyName);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
