using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public class CompanyMemberRepository : Repository<CompanyMember>, ICompanyMemberRepository
    {
        public CompanyMemberRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<CompanyMember>> GetCompanyMembersAsync(int companyId)
        {
            return await _context.CompanyMembers
                .Where(cm => cm.CompanyId == companyId && cm.IsActive)
                .Include(cm => cm.User)
                .OrderByDescending(cm => cm.JoinedAt)
                .ToListAsync();
        }

        public async Task<CompanyMember?> GetMemberAsync(int companyId, string userId)
        {
            return await _context.CompanyMembers
                .FirstOrDefaultAsync(cm => cm.CompanyId == companyId && cm.UserId == userId);
        }

        public async Task<List<CompanyMember>> GetUserCompaniesAsync(string userId)
        {
            return await _context.CompanyMembers
                .Where(cm => cm.UserId == userId && cm.IsActive)
                .Include(cm => cm.Company)
                .OrderByDescending(cm => cm.JoinedAt)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(int companyId, string userId)
        {
            return await _context.CompanyMembers
                .AnyAsync(cm => cm.CompanyId == companyId && cm.UserId == userId && cm.IsActive);
        }

        public async Task<bool> CanUserManageResourcesAsync(int companyId, string userId)
        {
            var member = await GetMemberAsync(companyId, userId);
            return member?.IsActive == true && member.CanManageResources;
        }

        public async Task<bool> CanUserManageReservationsAsync(int companyId, string userId)
        {
            var member = await GetMemberAsync(companyId, userId);
            return member?.IsActive == true && member.CanManageReservations;
        }

        public async Task RemoveMemberAsync(int companyId, string userId)
        {
            var member = await GetMemberAsync(companyId, userId);
            if (member != null)
            {
                _context.CompanyMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}
