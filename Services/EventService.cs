using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _db;
        public EventService(ApplicationDbContext db) => _db = db;

        public async Task<Event> CreateEvent(Event ev)
        {
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return ev;
        }

        public async Task<Event?> GetCurrentEvent(int resourceId, DateTime? at = null)
        {
            var now = at ?? DateTime.UtcNow;
            return await _db.Events.Where(e => e.ResourceId == resourceId && e.StartTime <= now && e.EndTime >= now).FirstOrDefaultAsync();
        }
    }
}
