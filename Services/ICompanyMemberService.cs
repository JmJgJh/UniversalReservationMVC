using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ICompanyMemberService
    {
        Task<List<CompanyMember>> GetCompanyMembersAsync(int companyId);
        Task<(bool Success, string Message)> AddMemberByEmailAsync(int companyId, string email, string role, bool canManageResources, bool canManageReservations);
        Task<bool> RemoveMemberAsync(int companyId, string userId);
        Task<bool> IsMemberAsync(int companyId, string userId);
    }
}
