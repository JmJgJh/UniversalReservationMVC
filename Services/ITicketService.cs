using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ITicketService
    {
        Task<Ticket> BuyTicketAsync(int reservationId, decimal price, string? purchaserId = null);
        Task CancelTicketAsync(int ticketId);
        Task<IEnumerable<Ticket>> GetTicketsForUserAsync(string userId);
        Task<Ticket?> GetTicketByIdAsync(int id);
        
        /// <summary>
        /// Generuje kod QR dla rezerwacji jako obraz Base64
        /// </summary>
        string GenerateQrCodeForReservation(int reservationId);
        
        /// <summary>
        /// Generuje unikalny kod biletu dla rezerwacji
        /// </summary>
        string GenerateTicketCode(int reservationId, DateTime createdAt);
    }
}
