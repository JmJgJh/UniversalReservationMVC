using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class SeatHold
    {
        public int ResourceId { get; set; }
        public int SeatId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string HolderKey { get; set; } = string.Empty; // userId or session key
        public DateTime ExpiresAt { get; set; }
    }

    public interface ISeatHoldService
    {
        bool TryHold(int resourceId, int seatId, DateTime start, DateTime end, string holderKey, TimeSpan ttl);
        bool Release(int resourceId, int seatId, string holderKey);
        IEnumerable<SeatHold> GetHolds(int resourceId);
        IEnumerable<int> GetOccupiedByHold(int resourceId, DateTime start, DateTime end);
        void CleanupExpired();
    }
}
