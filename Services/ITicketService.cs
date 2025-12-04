using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ITicketService
    {
        Task<Ticket> BuyTicket(int reservationId, decimal price, string? purchaserId = null);
        Task CancelTicket(int ticketId);
        Task<IEnumerable<Ticket>> GetTicketsForUser(string userId);
    }
}
