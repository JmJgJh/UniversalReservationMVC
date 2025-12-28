using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class ResourceRepository : Repository<Resource>, IResourceRepository
    {
        public ResourceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Resource?> GetByIdWithSeatsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type)
        {
            return await _dbSet
                .Where(r => r.ResourceType == type)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
    }
}
