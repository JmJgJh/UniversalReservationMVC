using Microsoft.AspNetCore.SignalR;
using UniversalReservationMVC.Hubs;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<SeatHub>? _hub;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IUnitOfWork unitOfWork, 
            ILogger<ReservationService> logger,
            IHubContext<SeatHub>? hub = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _hub = hub;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            _logger.LogInformation("Creating reservation for resource {ResourceId}, seat {SeatId}", 
                reservation.ResourceId, reservation.SeatId);

            if (reservation.SeatId.HasValue)
            {
                bool available = await IsSeatAvailableAsync(
                    reservation.ResourceId, 
                    reservation.SeatId.Value, 
                    reservation.StartTime, 
                    reservation.EndTime);
                    
                if (!available)
                {
                    _logger.LogWarning("Seat {SeatId} is not available for the requested time", reservation.SeatId.Value);
                    throw new InvalidOperationException("Miejsce jest już zajęte w wybranym czasie.");
                }
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.CreatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Reservation {ReservationId} created successfully", reservation.Id);

            if (_hub != null && reservation.SeatId.HasValue)
            {
                await _hub.Clients.Group(reservation.ResourceId.ToString())
                    .SendAsync("SeatReserved", new 
                    { 
                        resourceId = reservation.ResourceId, 
                        seatId = reservation.SeatId.Value, 
                        start = reservation.StartTime, 
                        end = reservation.EndTime 
                    });
            }

            return reservation;
        }

        public async Task<Reservation> CreateGuestReservationAsync(Reservation reservation)
        {
            _logger.LogInformation("Creating guest reservation for resource {ResourceId}", reservation.ResourceId);

            if (string.IsNullOrWhiteSpace(reservation.GuestEmail) && string.IsNullOrWhiteSpace(reservation.GuestPhone))
            {
                _logger.LogWarning("Guest reservation missing contact information");
                throw new ArgumentException("Wymagany jest e-mail lub telefon dla rezerwacji gościa.");
            }

            if (reservation.SeatId.HasValue)
            {
                bool available = await IsSeatAvailableAsync(
                    reservation.ResourceId, 
                    reservation.SeatId.Value, 
                    reservation.StartTime, 
                    reservation.EndTime);
                    
                if (!available)
                {
                    _logger.LogWarning("Seat {SeatId} is not available for guest reservation", reservation.SeatId.Value);
                    throw new InvalidOperationException("Miejsce jest już zajęte w wybranym czasie.");
                }
            }

            reservation.Status = ReservationStatus.Pending;
            reservation.CreatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Guest reservation {ReservationId} created successfully", reservation.Id);

            return reservation;
        }

        public async Task<Reservation> UpdateReservationAsync(Reservation reservation)
        {
            _logger.LogInformation("Updating reservation {ReservationId}", reservation.Id);

            var existing = await _unitOfWork.Reservations.GetByIdAsync(reservation.Id);
            if (existing == null)
            {
                _logger.LogWarning("Reservation {ReservationId} not found for update", reservation.Id);
                throw new KeyNotFoundException("Rezerwacja nie została znaleziona.");
            }

            // Check availability if seat or time changed
            if (reservation.SeatId.HasValue &&
                (existing.SeatId != reservation.SeatId || 
                 existing.StartTime != reservation.StartTime || 
                 existing.EndTime != reservation.EndTime))
            {
                bool available = await IsSeatAvailableAsync(
                    reservation.ResourceId,
                    reservation.SeatId.Value,
                    reservation.StartTime,
                    reservation.EndTime,
                    reservation.Id);

                if (!available)
                {
                    _logger.LogWarning("Updated seat/time conflict for reservation {ReservationId}", reservation.Id);
                    throw new InvalidOperationException("Miejsce jest już zajęte w wybranym czasie.");
                }
            }

            existing.StartTime = reservation.StartTime;
            existing.EndTime = reservation.EndTime;
            existing.SeatId = reservation.SeatId;
            existing.EventId = reservation.EventId;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Reservations.Update(existing);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Reservation {ReservationId} updated successfully", reservation.Id);

            return existing;
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            _logger.LogInformation("Cancelling reservation {ReservationId}", reservationId);

            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation {ReservationId} not found for cancellation", reservationId);
                throw new KeyNotFoundException("Rezerwacja nie została znaleziona.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Reservation {ReservationId} cancelled successfully", reservationId);

            if (_hub != null && reservation.SeatId.HasValue)
            {
                await _hub.Clients.Group(reservation.ResourceId.ToString())
                    .SendAsync("SeatReservationCancelled", new 
                    { 
                        resourceId = reservation.ResourceId, 
                        seatId = reservation.SeatId.Value, 
                        start = reservation.StartTime, 
                        end = reservation.EndTime 
                    });
            }
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            return await _unitOfWork.Reservations.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string userId)
        {
            return await _unitOfWork.Reservations.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsForResourceAsync(int resourceId, DateTime? from = null, DateTime? to = null)
        {
            return await _unitOfWork.Reservations.GetByResourceIdAsync(resourceId, from, to);
        }

        public async Task<IEnumerable<Seat>> GetSeatsAsync(int resourceId)
        {
            return await _unitOfWork.Seats.GetByResourceIdAsync(resourceId);
        }

        public async Task<bool> IsSeatAvailableAsync(int resourceId, int seatId, DateTime start, DateTime end, int? excludeReservationId = null)
        {
            var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
            if (seat == null || seat.ResourceId != resourceId)
            {
                _logger.LogWarning("Seat {SeatId} not found or doesn't belong to resource {ResourceId}", seatId, resourceId);
                return false;
            }

            var hasConflict = await _unitOfWork.Reservations.HasConflictAsync(resourceId, seatId, start, end, excludeReservationId);
            return !hasConflict;
        }
    }
}
