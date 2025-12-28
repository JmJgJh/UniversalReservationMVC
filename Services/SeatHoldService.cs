using System.Collections.Concurrent;

namespace UniversalReservationMVC.Services
{
    public class SeatHoldService : ISeatHoldService
    {
        private readonly ConcurrentDictionary<(int resourceId, int seatId), SeatHold> _holds = new();

        public bool TryHold(int resourceId, int seatId, DateTime start, DateTime end, string holderKey, TimeSpan ttl)
        {
            CleanupExpired();
            var key = (resourceId, seatId);
            var now = DateTime.UtcNow;
            var expires = now.Add(ttl);

            // Reject invalid range
            if (start >= end) return false;

            // If a current non-expired hold conflicts, reject
            if (_holds.TryGetValue(key, out var existing))
            {
                if (existing.ExpiresAt > now && existing.StartTime < end && existing.EndTime > start)
                {
                    // Existing overlapping hold
                    return false;
                }
            }

            var hold = new SeatHold
            {
                ResourceId = resourceId,
                SeatId = seatId,
                StartTime = start,
                EndTime = end,
                HolderKey = holderKey,
                ExpiresAt = expires
            };
            _holds[key] = hold;
            return true;
        }

        public bool Release(int resourceId, int seatId, string holderKey)
        {
            var key = (resourceId, seatId);
            if (_holds.TryGetValue(key, out var existing))
            {
                if (existing.HolderKey == holderKey)
                {
                    return _holds.TryRemove(key, out _);
                }
            }
            return false;
        }

        public IEnumerable<SeatHold> GetHolds(int resourceId)
        {
            CleanupExpired();
            return _holds.Values.Where(h => h.ResourceId == resourceId);
        }

        public IEnumerable<int> GetOccupiedByHold(int resourceId, DateTime start, DateTime end)
        {
            CleanupExpired();
            return _holds.Values
                .Where(h => h.ResourceId == resourceId && h.StartTime < end && h.EndTime > start && h.ExpiresAt > DateTime.UtcNow)
                .Select(h => h.SeatId)
                .Distinct()
                .ToList();
        }

        public void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _holds.ToArray())
            {
                if (kv.Value.ExpiresAt <= now)
                {
                    _holds.TryRemove(kv.Key, out _);
                }
            }
        }
    }
}
