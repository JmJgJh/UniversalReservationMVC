using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservation(Reservation reservation);
        Task<Reservation> CreateGuestReservation(Reservation reservation);
        Task CancelReservation(int reservationId);
        Task<IEnumerable<Reservation>> GetReservationsForUser(string userId);
        Task<IEnumerable<Reservation>> GetReservationsForResource(int resourceId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Seat>> GetSeatsAsync(int resourceId);
        Task<bool> IsSeatAvailableAsync(int resourceId, int seatId, DateTime start, DateTime end);
    }
}
