using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ICompanyService
    {
        Task<Company?> GetCompanyByOwnerAsync(string userId);
        Task<Company?> GetCompanyByIdAsync(int companyId);
        Task<IEnumerable<Company>> GetAllActiveCompaniesAsync();
        Task<Company> CreateCompanyAsync(Company company);
        Task<Company> UpdateCompanyAsync(Company company);
        Task<bool> DeleteCompanyAsync(int companyId);
        Task<bool> UserIsCompanyOwnerAsync(string userId, int companyId);
    }
}
