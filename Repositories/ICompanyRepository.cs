using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetByOwnerIdAsync(string ownerId);
        Task<IEnumerable<Company>> GetAllActiveAsync();
        Task<IEnumerable<Company>> GetByOwnerIdWithResourcesAsync(string ownerId);
    }
}
