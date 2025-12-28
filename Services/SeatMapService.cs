using System.Linq;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;

namespace UniversalReservationMVC.Services
{
    public class SeatMapService : ISeatMapService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SeatMapService> _logger;

        public SeatMapService(IUnitOfWork unitOfWork, ILogger<SeatMapService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Seat>> GetSeatMapAsync(int resourceId)
        {
            return await _unitOfWork.Seats.GetByResourceIdAsync(resourceId);
        }

        public async Task<IEnumerable<Seat>> GenerateSeatGridAsync(int resourceId, int rows, int columns)
        {
            _logger.LogInformation("Generating seat grid for resource {ResourceId}: {Rows}x{Columns}", 
                resourceId, rows, columns);

            if (rows <= 0 || columns <= 0)
            {
                throw new ArgumentException("Liczba rzędów i kolumn musi być większa od zera.");
            }

            if (rows > 100 || columns > 100)
            {
                throw new ArgumentException("Maksymalna liczba rzędów i kolumn to 100.");
            }

            var seats = new List<Seat>();
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 1; c <= columns; c++)
                {
                    char rowChar = (char)('A' + (r - 1));
                    seats.Add(new Seat
                    {
                        ResourceId = resourceId,
                        X = c,
                        Y = r,
                        Row = rowChar.ToString(),
                        Column = c,
                        Label = $"{rowChar}{c}",
                        IsAvailable = true
                    });
                }
            }

            await _unitOfWork.Seats.AddRangeAsync(seats);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Generated {Count} seats for resource {ResourceId}", 
                seats.Count, resourceId);

            return seats;
        }

        public async Task SaveSeatMapAsync(int resourceId, IEnumerable<Seat> seats)
        {
            if (seats == null)
            {
                throw new ArgumentException("Lista miejsc nie może być pusta.");
            }

            var seatList = seats.ToList();
            if (!seatList.Any())
            {
                throw new ArgumentException("Mapa miejsc jest pusta.");
            }
            _logger.LogInformation("Saving seat map for resource {ResourceId} with {Count} seats", resourceId, seatList.Count);

            // Validate basics
            var invalid = seatList.FirstOrDefault(s => s.X <= 0 || s.Y <= 0);
            if (invalid != null)
            {
                throw new ArgumentException("Współrzędne muszą być większe od zera.");
            }

            var duplicate = seatList.GroupBy(s => new { s.X, s.Y }).FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                throw new ArgumentException("Duplikaty miejsc (X,Y) w mapie.");
            }

            var existing = (await _unitOfWork.Seats.GetByResourceIdAsync(resourceId)).ToList();

            // Remove seats no longer present
            var toRemove = existing.Where(e => seatList.All(s => s.X != e.X || s.Y != e.Y)).ToList();
            if (toRemove.Any())
            {
                _unitOfWork.Seats.RemoveRange(toRemove);
            }

            foreach (var incoming in seatList)
            {
                incoming.ResourceId = resourceId;
                incoming.Row ??= ComputeRow(incoming.Y);
                incoming.Column ??= incoming.X;
                incoming.Label = string.IsNullOrWhiteSpace(incoming.Label)
                    ? $"{incoming.Row}{incoming.Column}"
                    : incoming.Label;

                var match = existing.FirstOrDefault(e => e.X == incoming.X && e.Y == incoming.Y);
                if (match != null)
                {
                    match.Label = incoming.Label;
                    match.Row = incoming.Row;
                    match.Column = incoming.Column;
                    match.IsAvailable = incoming.IsAvailable;
                    _unitOfWork.Seats.Update(match);
                }
                else
                {
                    await _unitOfWork.Seats.AddAsync(new Seat
                    {
                        ResourceId = resourceId,
                        X = incoming.X,
                        Y = incoming.Y,
                        Label = incoming.Label,
                        Row = incoming.Row,
                        Column = incoming.Column,
                        IsAvailable = incoming.IsAvailable
                    });
                }
            }

            await _unitOfWork.SaveAsync();
        }

        private static string ComputeRow(int y)
        {
            if (y >= 1 && y <= 26)
            {
                return ((char)('A' + (y - 1))).ToString();
            }

            return $"R{y}";
        }
    }
}
