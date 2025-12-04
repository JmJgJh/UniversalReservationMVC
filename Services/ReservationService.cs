using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _db;
        public ReservationService(ApplicationDbContext db) => _db = db;

        public async Task<Reservation> CreateReservation(Reservation reservation)
        {
            if (reservation.SeatId.HasValue)
            {
                bool available = await IsSeatAvailableAsync(reservation.ResourceId, reservation.SeatId.Value, reservation.StartTime, reservation.EndTime);
                if (!available) throw new InvalidOperationException("Seat is already taken for the requested time range.");
            }
            reservation.Status = ReservationStatus.Confirmed;
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation> CreateGuestReservation(Reservation reservation)
        {
            // guest reservation: ensure GuestEmail or GuestPhone provided
            if (string.IsNullOrWhiteSpace(reservation.GuestEmail) && string.IsNullOrWhiteSpace(reservation.GuestPhone))
                throw new ArgumentException("Guest contact required for guest reservation.");

            if (reservation.SeatId.HasValue)
            {
                bool available = await IsSeatAvailableAsync(reservation.ResourceId, reservation.SeatId.Value, reservation.StartTime, reservation.EndTime);
                if (!available) throw new InvalidOperationException("Seat is already taken for the requested time range.");
            }
            reservation.Status = ReservationStatus.Pending;
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        public async Task CancelReservation(int reservationId)
        {
            var res = await _db.Reservations.FindAsync(reservationId);
            if (res == null) throw new KeyNotFoundException("Reservation not found.");
            res.Status = ReservationStatus.Cancelled;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsForUser(string userId)
        {
            return await _db.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Seat)
                .Include(r => r.Resource)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsForResource(int resourceId, DateTime? from = null, DateTime? to = null)
        {
            var q = _db.Reservations.Where(r => r.ResourceId == resourceId).AsQueryable();
            if (from.HasValue) q = q.Where(r => r.EndTime >= from.Value);
            if (to.HasValue) q = q.Where(r => r.StartTime <= to.Value);
            return await q.Include(r => r.Seat).Include(r => r.User).ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetSeatsAsync(int resourceId)
        {
            return await _db.Seats.Where(s => s.ResourceId == resourceId).ToListAsync();
        }

        public async Task<bool> IsSeatAvailableAsync(int resourceId, int seatId, DateTime start, DateTime end)
        {
            var seat = await _db.Seats.FindAsync(seatId);
            if (seat == null) return false;
            var conflict = await _db.Reservations.AnyAsync(r => r.ResourceId == resourceId
                && r.SeatId == seatId
                && r.StartTime < end && r.EndTime > start
                && r.Status != ReservationStatus.Cancelled);
            return !conflict;
        }
    }
}
