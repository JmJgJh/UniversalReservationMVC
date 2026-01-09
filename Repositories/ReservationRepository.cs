using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .Include(r => r.Seat)
                .Include(r => r.Resource)
                .Include(r => r.Event)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByResourceIdAsync(int resourceId, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbSet
                .Where(r => r.ResourceId == resourceId)
                .Include(r => r.Seat)
                .Include(r => r.Resource)
                .Include(r => r.User)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.EndTime >= from.Value);
            
            if (to.HasValue)
                query = query.Where(r => r.StartTime <= to.Value);

            return await query.OrderBy(r => r.StartTime).ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int resourceId, int seatId, DateTime start, DateTime end, int? excludeReservationId = null)
        {
            var query = _dbSet
                .Where(r => r.ResourceId == resourceId
                    && r.SeatId == seatId
                    && r.StartTime < end 
                    && r.EndTime > start
                    && r.Status != ReservationStatus.Cancelled);

            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(r => r.Status == ReservationStatus.Confirmed 
                    && r.StartTime <= now 
                    && r.EndTime >= now)
                .Include(r => r.Resource)
                .Include(r => r.User)
                .ToListAsync();
        }
    }
}
