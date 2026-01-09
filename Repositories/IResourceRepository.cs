using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface IResourceRepository : IRepository<Resource>
    {
        Task<Resource?> GetByIdWithSeatsAsync(int id);
        Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type);
    }
}
