using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ITicketService
    {
        Task<Ticket> BuyTicketAsync(int reservationId, decimal price, string? purchaserId = null);
        Task CancelTicketAsync(int ticketId);
        Task<IEnumerable<Ticket>> GetTicketsForUserAsync(string userId);
        Task<Ticket?> GetTicketByIdAsync(int id);
    }
}
