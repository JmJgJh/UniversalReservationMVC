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
        private readonly IEmailService _emailService;

        public ReservationService(
            IUnitOfWork unitOfWork, 
            ILogger<ReservationService> logger,
            IEmailService emailService,
            IHubContext<SeatHub>? hub = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _hub = hub;
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            _logger.LogInformation("Creating reservation for resource {ResourceId}, seat {SeatId}", 
                reservation.ResourceId, reservation.SeatId);

            // Check working hours
            var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
            if (resource != null && !string.IsNullOrEmpty(resource.WorkingHours))
            {
                if (!IsWithinWorkingHours(reservation.StartTime, reservation.EndTime, resource.WorkingHours))
                {
                    throw new InvalidOperationException("Rezerwacja znajduje się poza godzinami otwarcia zasobu.");
                }
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
                    _logger.LogWarning("Seat {SeatId} is not available for the requested time", reservation.SeatId.Value);
                    throw new InvalidOperationException("Miejsce jest już zajęte w wybranym czasie.");
                }
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.CreatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Reservation {ReservationId} created successfully", reservation.Id);

            // Send confirmation email
            if (reservation.User != null)
            {
                var resourceForEmail = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
                var seat = reservation.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(reservation.SeatId.Value) : null;
                var seatInfo = seat != null ? $"Rząd {seat.X}, Miejsce {seat.Y}" : null;
                var companyName = resourceForEmail?.Company?.Name ?? "System Rezerwacji";

                try
                {
                    await _emailService.SendReservationConfirmationAsync(
                        reservation.User.Email ?? string.Empty,
                        reservation.User.FirstName ?? reservation.User.UserName ?? "Użytkownik",
                        resourceForEmail?.Name ?? "Zasób",
                        reservation.StartTime,
                        seatInfo,
                        companyName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email for reservation {ReservationId}", reservation.Id);
                }
            }

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

            // Send confirmation email to guest
            if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
            {
                var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
                var seat = reservation.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(reservation.SeatId.Value) : null;
                var seatInfo = seat != null ? $"Rząd {seat.X}, Miejsce {seat.Y}" : null;
                var companyName = resource?.Company?.Name ?? "System Rezerwacji";

                try
                {
                    await _emailService.SendReservationConfirmationAsync(
                        reservation.GuestEmail,
                        "Gość",
                        resource?.Name ?? "Zasób",
                        reservation.StartTime,
                        seatInfo,
                        companyName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email for guest reservation {ReservationId}", reservation.Id);
                }
            }

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

            // Send update email (user or guest)
            var email = existing.User?.Email ?? existing.GuestEmail;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var resource = await _unitOfWork.Resources.GetByIdAsync(existing.ResourceId);
                var seat = existing.SeatId.HasValue ? await _unitOfWork.Seats.GetByIdAsync(existing.SeatId.Value) : null;
                var seatInfo = seat != null ? $"Rząd {seat.X}, Miejsce {seat.Y}" : null;
                var companyName = resource?.Company?.Name ?? "System Rezerwacji";
                var displayName = existing.User?.FirstName ?? existing.User?.UserName ?? "Gość";

                try
                {
                    await _emailService.SendReservationConfirmationAsync(
                        email,
                        displayName,
                        resource?.Name ?? "Zasób",
                        existing.StartTime,
                        seatInfo,
                        companyName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send update email for reservation {ReservationId}", reservation.Id);
                }
            }

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

            // Send cancellation email
            var email = reservation.User?.Email ?? reservation.GuestEmail;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var resource = await _unitOfWork.Resources.GetByIdAsync(reservation.ResourceId);
                var userName = reservation.User?.FirstName ?? reservation.User?.UserName ?? "Gość";
                var companyName = resource?.Company?.Name ?? "System Rezerwacji";

                try
                {
                    await _emailService.SendReservationCancellationAsync(
                        email,
                        userName,
                        resource?.Name ?? "Zasób",
                        reservation.StartTime,
                        companyName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email for reservation {ReservationId}", reservationId);
                }
            }

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

        private bool IsWithinWorkingHours(DateTime startTime, DateTime endTime, string workingHoursJson)
        {
            try
            {
                var config = System.Text.Json.JsonSerializer.Deserialize<WorkingHoursConfig>(workingHoursJson);
                if (config == null || config.Hours.Count == 0)
                {
                    return true; // No working hours configured, allow all times
                }

                // Check if reservation spans multiple days
                var currentDate = startTime.Date;
                var endDate = endTime.Date;

                while (currentDate <= endDate)
                {
                    var dayName = currentDate.DayOfWeek.ToString().ToLower();
                    
                    if (!config.Hours.TryGetValue(dayName, out var dayHours))
                    {
                        _logger.LogWarning("No working hours configured for {DayName}", dayName);
                        return false; // Day not configured
                    }

                    if (dayHours.IsClosed)
                    {
                        _logger.LogWarning("Resource is closed on {DayName}", dayName);
                        return false; // Resource is closed on this day
                    }

                    if (!string.IsNullOrEmpty(dayHours.Open) && !string.IsNullOrEmpty(dayHours.Close))
                    {
                        var openTime = TimeSpan.Parse(dayHours.Open);
                        var closeTime = TimeSpan.Parse(dayHours.Close);

                        var checkStart = currentDate == startTime.Date ? startTime.TimeOfDay : TimeSpan.Zero;
                        var checkEnd = currentDate == endTime.Date ? endTime.TimeOfDay : new TimeSpan(23, 59, 59);

                        if (checkStart < openTime || checkEnd > closeTime)
                        {
                            _logger.LogWarning("Reservation time {Start}-{End} outside working hours {Open}-{Close} on {Date}",
                                checkStart, checkEnd, openTime, closeTime, currentDate);
                            return false;
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing working hours JSON: {Json}", workingHoursJson);
                return true; // If parsing fails, don't block reservation
            }
        }
    }
}
