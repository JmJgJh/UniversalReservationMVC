using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface IEventService
    {
        Task<Event> CreateEvent(Event ev);
        Task<Event?> GetCurrentEvent(int resourceId, DateTime? at = null);
    }
}
