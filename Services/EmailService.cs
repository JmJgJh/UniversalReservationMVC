using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace UniversalReservationMVC.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendReservationConfirmationAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string? seatInfo, string companyName)
    {
        var subject = $"Potwierdzenie rezerwacji - {resourceName}";
        var seatDetails = !string.IsNullOrEmpty(seatInfo) ? $"<p><strong>Miejsce:</strong> {seatInfo}</p>" : "";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Potwierdzenie rezerwacji</h1>
        </div>
        <div class='content'>
            <p>Witaj <strong>{recipientName}</strong>,</p>
            <p>Twoja rezerwacja została potwierdzona.</p>
            <h3>Szczegóły rezerwacji:</h3>
            <p><strong>Zasób:</strong> {resourceName}</p>
            <p><strong>Data i godzina:</strong> {reservationDate:dd.MM.yyyy HH:mm}</p>
            {seatDetails}
            <p><strong>Firma:</strong> {companyName}</p>
            <p>Dziękujemy za skorzystanie z naszych usług!</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {companyName}. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendReservationCancellationAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string companyName)
    {
        var subject = $"Anulowanie rezerwacji - {resourceName}";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Anulowanie rezerwacji</h1>
        </div>
        <div class='content'>
            <p>Witaj <strong>{recipientName}</strong>,</p>
            <p>Twoja rezerwacja została anulowana.</p>
            <h3>Szczegóły anulowanej rezerwacji:</h3>
            <p><strong>Zasób:</strong> {resourceName}</p>
            <p><strong>Data i godzina:</strong> {reservationDate:dd.MM.yyyy HH:mm}</p>
            <p><strong>Firma:</strong> {companyName}</p>
            <p>Mamy nadzieję zobaczyć Cię ponownie!</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {companyName}. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendReservationReminderAsync(string recipientEmail, string recipientName, string resourceName, DateTime reservationDate, string? seatInfo, string companyName)
    {
        var subject = $"Przypomnienie o rezerwacji - {resourceName}";
        var seatDetails = !string.IsNullOrEmpty(seatInfo) ? $"<p><strong>Miejsce:</strong> {seatInfo}</p>" : "";
        
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Przypomnienie o rezerwacji</h1>
        </div>
        <div class='content'>
            <p>Witaj <strong>{recipientName}</strong>,</p>
            <p>Przypominamy o Twojej nadchodzącej rezerwacji.</p>
            <h3>Szczegóły rezerwacji:</h3>
            <p><strong>Zasób:</strong> {resourceName}</p>
            <p><strong>Data i godzina:</strong> {reservationDate:dd.MM.yyyy HH:mm}</p>
            {seatDetails}
            <p><strong>Firma:</strong> {companyName}</p>
            <p>Do zobaczenia!</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} {companyName}. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email sending is disabled. Would send to {To} with subject: {Subject}", to, subject);
            return;
        }

        try
        {
            using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

public class EmailSettings
{
    public bool Enabled { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
