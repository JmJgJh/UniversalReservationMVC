using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace UniversalReservationMVC.Services
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TicketService> _logger;

        public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Ticket> BuyTicketAsync(int reservationId, decimal price, string? purchaserId = null)
        {
            _logger.LogInformation("Processing ticket purchase for reservation {ReservationId}", reservationId);

            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation {ReservationId} not found for ticket purchase", reservationId);
                throw new KeyNotFoundException("Rezerwacja nie została znaleziona.");
            }

            // Check if ticket already purchased
            var alreadyPurchased = await _unitOfWork.Tickets.HasPurchasedTicketAsync(reservationId);
            if (alreadyPurchased)
            {
                _logger.LogWarning("Ticket already purchased for reservation {ReservationId}", reservationId);
                throw new InvalidOperationException("Bilet dla tej rezerwacji został już zakupiony.");
            }

            if (price <= 0)
            {
                throw new ArgumentException("Cena biletu musi być większa od zera.");
            }

            var ticket = new Ticket
            {
                ReservationId = reservationId,
                Price = price,
                Status = TicketStatus.Purchased,
                PurchaseReference = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                PurchasedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Ticket {TicketId} purchased successfully for reservation {ReservationId}", 
                ticket.Id, reservationId);

            return ticket;
        }

        public async Task CancelTicketAsync(int ticketId)
        {
            _logger.LogInformation("Cancelling ticket {TicketId}", ticketId);

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} not found for cancellation", ticketId);
                throw new KeyNotFoundException("Bilet nie został znaleziony.");
            }

            ticket.Status = TicketStatus.Cancelled;
            _unitOfWork.Tickets.Update(ticket);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Ticket {TicketId} cancelled successfully", ticketId);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsForUserAsync(string userId)
        {
            return await _unitOfWork.Tickets.GetByUserIdAsync(userId);
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _unitOfWork.Tickets.GetByIdAsync(id);
        }

        public string GenerateQrCodeForReservation(int reservationId)
        {
            try
            {
                // Generate ticket verification URL or code
                var ticketCode = GenerateTicketCode(reservationId, DateTime.UtcNow);
                var qrContent = $"RESERVATION:{reservationId}|CODE:{ticketCode}|VERIFY:https://localhost/Reservation/Verify/{reservationId}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                // Convert to Base64 string for embedding in HTML
                return Convert.ToBase64String(qrCodeImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for reservation {ReservationId}", reservationId);
                return string.Empty;
            }
        }

        public string GenerateTicketCode(int reservationId, DateTime createdAt)
        {
            // Generate a unique, verifiable ticket code
            var input = $"{reservationId}-{createdAt:yyyyMMddHHmmss}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            // Take first 8 bytes and convert to alphanumeric code
            var code = Convert.ToBase64String(hashBytes.Take(8).ToArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .ToUpper();
            
            return $"TKT-{code}";
        }
    }
}
