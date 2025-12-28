using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<Reservation> CreateGuestReservationAsync(Reservation reservation);
        Task<Reservation> UpdateReservationAsync(Reservation reservation);
        Task CancelReservationAsync(int reservationId);
        Task<Reservation?> GetReservationByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string userId);
        Task<IEnumerable<Reservation>> GetReservationsForResourceAsync(int resourceId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Seat>> GetSeatsAsync(int resourceId);
        Task<bool> IsSeatAvailableAsync(int resourceId, int seatId, DateTime start, DateTime end, int? excludeReservationId = null);
    }
}
