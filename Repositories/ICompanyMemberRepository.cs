using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface ICompanyMemberRepository : IRepository<CompanyMember>
    {
        Task<List<CompanyMember>> GetCompanyMembersAsync(int companyId);
        Task<CompanyMember?> GetMemberAsync(int companyId, string userId);
        Task<CompanyMember?> GetByIdWithIncludesAsync(int id);
        Task<List<CompanyMember>> GetUserCompaniesAsync(string userId);
        Task<bool> IsMemberAsync(int companyId, string userId);
        Task<bool> CanUserManageResourcesAsync(int companyId, string userId);
        Task<bool> CanUserManageReservationsAsync(int companyId, string userId);
        Task RemoveMemberAsync(int companyId, string userId);
    }
}
