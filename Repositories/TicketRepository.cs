using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Ticket>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(t => t.Reservation)
                    .ThenInclude(r => r!.Resource)
                .Include(t => t.Reservation)
                    .ThenInclude(r => r!.Seat)
                .Where(t => t.Reservation != null && t.Reservation.UserId == userId)
                .OrderByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<bool> HasPurchasedTicketAsync(int reservationId)
        {
            return await _dbSet
                .AnyAsync(t => t.ReservationId == reservationId 
                    && t.Status == TicketStatus.Purchased);
        }
    }
}
