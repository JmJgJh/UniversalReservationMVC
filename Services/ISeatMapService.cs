using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface ISeatMapService
    {
        Task<IEnumerable<Seat>> GetSeatMap(int resourceId);
        Task<IEnumerable<Seat>> GenerateSeatGrid(int resourceId, int rows, int columns);
    }
}
