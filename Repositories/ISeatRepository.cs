using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Repositories
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<IEnumerable<Seat>> GetByResourceIdAsync(int resourceId);
        Task<Seat?> GetSeatWithResourceAsync(int seatId);
    }
}
