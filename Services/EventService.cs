using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EventService> _logger;

        public EventService(IUnitOfWork unitOfWork, ILogger<EventService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Event> CreateEventAsync(Event ev)
        {
            _logger.LogInformation("Creating event {Title} for resource {ResourceId}", ev.Title, ev.ResourceId);

            if (ev.StartTime >= ev.EndTime)
            {
                throw new ArgumentException("Data zakończenia musi być późniejsza niż data rozpoczęcia.");
            }

            await _unitOfWork.Events.AddAsync(ev);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Event {EventId} created successfully", ev.Id);
            return ev;
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _unitOfWork.Events.GetByIdWithResourceAsync(id);
        }

        public async Task<Event?> GetCurrentEventAsync(int resourceId, DateTime? at = null)
        {
            return await _unitOfWork.Events.GetCurrentEventAsync(resourceId, at);
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _unitOfWork.Events.GetAllAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int? resourceId = null)
        {
            return await _unitOfWork.Events.GetUpcomingEventsAsync(resourceId);
        }

        public async Task UpdateEventAsync(Event ev)
        {
            _logger.LogInformation("Updating event {EventId}", ev.Id);

            if (ev.StartTime >= ev.EndTime)
            {
                throw new ArgumentException("Data zakończenia musi być późniejsza niż data rozpoczęcia.");
            }

            _unitOfWork.Events.Update(ev);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Event {EventId} updated successfully", ev.Id);
        }

        public async Task DeleteEventAsync(int id)
        {
            _logger.LogInformation("Deleting event {EventId}", id);

            var ev = await _unitOfWork.Events.GetByIdAsync(id);
            if (ev == null)
            {
                throw new KeyNotFoundException("Wydarzenie nie zostało znalezione.");
            }

            _unitOfWork.Events.Remove(ev);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Event {EventId} deleted successfully", id);
        }
    }
}
