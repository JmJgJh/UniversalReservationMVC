using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(Event ev);
        Task<Event?> GetEventByIdAsync(int id);
        Task<Event?> GetCurrentEventAsync(int resourceId, DateTime? at = null);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> GetUpcomingEventsAsync(int? resourceId = null);
        Task UpdateEventAsync(Event ev);
        Task DeleteEventAsync(int id);
    }
}
