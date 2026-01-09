using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ISeatMapService
    {
        Task<IEnumerable<Seat>> GetSeatMapAsync(int resourceId);
        Task<IEnumerable<Seat>> GenerateSeatGridAsync(int resourceId, int rows, int columns);
        Task SaveSeatMapAsync(int resourceId, IEnumerable<Seat> seats);
    }
}
