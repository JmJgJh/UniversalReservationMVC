using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetByUserIdAsync(string userId);
        Task<bool> HasPurchasedTicketAsync(int reservationId);
    }
}
