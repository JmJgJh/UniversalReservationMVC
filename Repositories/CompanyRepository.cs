using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Company?> GetByOwnerIdAsync(string ownerId)
        {
            return await _context.Companies
                .Include(c => c.Owner)
                .Include(c => c.Resources)
                .FirstOrDefaultAsync(c => c.OwnerId == ownerId && c.IsActive);
        }

        public async Task<IEnumerable<Company>> GetAllActiveAsync()
        {
            return await _context.Companies
                .Include(c => c.Owner)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetByOwnerIdWithResourcesAsync(string ownerId)
        {
            return await _context.Companies
                .Include(c => c.Resources!)
                    .ThenInclude(r => r.Seats)
                .Include(c => c.Owner)
                .Where(c => c.OwnerId == ownerId && c.IsActive)
                .ToListAsync();
        }
    }
}
