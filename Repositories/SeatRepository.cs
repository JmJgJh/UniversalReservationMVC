using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Seat>> GetByResourceIdAsync(int resourceId)
        {
            return await _dbSet
                .Where(s => s.ResourceId == resourceId)
                .OrderBy(s => s.Y)
                .ThenBy(s => s.X)
                .ToListAsync();
        }

        public async Task<Seat?> GetSeatWithResourceAsync(int seatId)
        {
            return await _dbSet
                .Include(s => s.Resource)
                .FirstOrDefaultAsync(s => s.Id == seatId);
        }
    }
}
