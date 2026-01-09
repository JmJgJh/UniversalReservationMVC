using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<Event?> GetCurrentEventAsync(int resourceId, DateTime? at = null);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync(int? resourceId = null);
        Task<Event?> GetByIdWithResourceAsync(int id);
    }
}
