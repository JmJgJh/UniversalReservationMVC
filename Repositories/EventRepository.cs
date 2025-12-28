using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Event?> GetCurrentEventAsync(int resourceId, DateTime? at = null)
        {
            var now = at ?? DateTime.UtcNow;
            return await _dbSet
                .Where(e => e.ResourceId == resourceId 
                    && e.StartTime <= now 
                    && e.EndTime >= now)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int? resourceId = null)
        {
            var now = DateTime.UtcNow;
            var query = _dbSet
                .Where(e => e.StartTime >= now)
                .Include(e => e.Resource)
                .AsQueryable();

            if (resourceId.HasValue)
                query = query.Where(e => e.ResourceId == resourceId.Value);

            return await query
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<Event?> GetByIdWithResourceAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Resource)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
