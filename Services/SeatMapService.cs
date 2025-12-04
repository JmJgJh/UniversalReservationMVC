using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class SeatMapService : ISeatMapService
    {
        private readonly ApplicationDbContext _db;
        public SeatMapService(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Seat>> GetSeatMap(int resourceId)
        {
            return await _db.Seats.Where(s => s.ResourceId == resourceId).ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GenerateSeatGrid(int resourceId, int rows, int columns)
        {
            var seats = new List<Seat>();
            for (int r = 1; r <= rows; r++)
            {
                for (int c = 1; c <= columns; c++)
                {
                    seats.Add(new Seat
                    {
                        ResourceId = resourceId,
                        X = c,
                        Y = r,
                        Row = ((char)('A' + (r - 1))).ToString(),
                        Column = c,
                        Label = $"{(char)('A' + (r - 1))}{c}",
                        IsAvailable = true
                    });
                }
            }
            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();
            return seats;
        }
    }
}
