using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Reservation>> GetByResourceIdAsync(int resourceId, DateTime? from = null, DateTime? to = null);
        Task<bool> HasConflictAsync(int resourceId, int seatId, DateTime start, DateTime end, int? excludeReservationId = null);
        Task<IEnumerable<Reservation>> GetActiveReservationsAsync();
    }
}
