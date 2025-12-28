using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

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
    }
}
