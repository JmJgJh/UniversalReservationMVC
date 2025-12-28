using Microsoft.Extensions.Logging;
using Moq;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.Tests.Fakes;
using UniversalReservationMVC.Hubs;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class ReservationServiceTests
    {
        [Fact]
        public async Task IsSeatAvailable_ReturnsFalse_WhenSeatDoesNotBelongToResource()
        {
            var unit = new InMemoryUnitOfWork();
            await unit.Seats.AddAsync(new Seat { Id = 1, ResourceId = 2, X = 1, Y = 1 });

            var service = new ReservationService(unit, Mock.Of<ILogger<ReservationService>>());
            var available = await service.IsSeatAvailableAsync(resourceId: 9, seatId: 1, start: DateTime.UtcNow, end: DateTime.UtcNow.AddHours(1));

            Assert.False(available);
        }

        [Fact]
        public async Task CreateGuestReservation_Throws_WhenMissingContact()
        {
            var unit = new InMemoryUnitOfWork();
            var service = new ReservationService(unit, Mock.Of<ILogger<ReservationService>>());

            var reservation = new Reservation
            {
                ResourceId = 5,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                SeatId = null,
                GuestEmail = null,
                GuestPhone = null
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateGuestReservationAsync(reservation));
        }

        [Fact]
        public async Task CreateReservation_Throws_WhenSeatHasConflict()
        {
            var unit = new InMemoryUnitOfWork();
            // seat belongs to the resource
            await unit.Seats.AddAsync(new Seat { Id = 1, ResourceId = 5, X = 1, Y = 1 });
            // existing reservation overlapping
            await unit.Reservations.AddAsync(new Reservation
            {
                ResourceId = 5,
                SeatId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            });

            var service = new ReservationService(unit, Mock.Of<ILogger<ReservationService>>());

            var newReservation = new Reservation
            {
                ResourceId = 5,
                SeatId = 1,
                StartTime = DateTime.UtcNow.AddMinutes(30),
                EndTime = DateTime.UtcNow.AddHours(3)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(newReservation));
        }

        [Fact]
        public async Task CreateReservation_SetsConfirmedAndPersists_WhenAvailable()
        {
            var unit = new InMemoryUnitOfWork();
            await unit.Seats.AddAsync(new Seat { Id = 1, ResourceId = 7, X = 1, Y = 1 });

            var service = new ReservationService(unit, Mock.Of<ILogger<ReservationService>>());

            var reservation = new Reservation
            {
                ResourceId = 7,
                SeatId = 1,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var created = await service.CreateReservationAsync(reservation);

            Assert.Equal(ReservationStatus.Confirmed, created.Status);
            Assert.True(created.Id > 0);
            Assert.NotEqual(default, created.CreatedAt);

            var all = (await unit.Reservations.GetAllAsync()).ToList();
            Assert.Single(all);
            Assert.Equal(created.Id, all[0].Id);
        }

        [Fact]
        public async Task CreateReservation_PublishesSeatReserved_ToHubGroup()
        {
            var unit = new InMemoryUnitOfWork();
            await unit.Seats.AddAsync(new Seat { Id = 2, ResourceId = 11, X = 1, Y = 1 });

            var fakeHub = new FakeHubContext<SeatHub>();
            var logger = Mock.Of<ILogger<ReservationService>>();
            var service = new ReservationService(unit, logger, fakeHub);

            var reservation = new Reservation
            {
                ResourceId = 11,
                SeatId = 2,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            await service.CreateReservationAsync(reservation);

            Assert.Contains(fakeHub.GroupProxy.Sent, x => x.method == "SeatReserved");
        }

        private sealed class InMemoryUnitOfWork : IUnitOfWork
        {
            public InMemoryUnitOfWork()
            {
                Reservations = new InMemoryReservationRepository();
                Seats = new InMemorySeatRepository();
            }

            public IReservationRepository Reservations { get; }
            public IResourceRepository Resources => throw new NotImplementedException();
            public IEventRepository Events => throw new NotImplementedException();
            public ITicketRepository Tickets => throw new NotImplementedException();
            public ISeatRepository Seats { get; }
            public ICompanyRepository Companies => throw new NotImplementedException();
            public ICompanyMemberRepository CompanyMembers => throw new NotImplementedException();

            public Task<int> SaveAsync() => Task.FromResult(0);
            public Task BeginTransactionAsync() => Task.CompletedTask;
            public Task CommitTransactionAsync() => Task.CompletedTask;
            public Task RollbackTransactionAsync() => Task.CompletedTask;
            public void Dispose() { }
        }

        private sealed class InMemorySeatRepository : ISeatRepository
        {
            private readonly List<Seat> _seats = new();
            private int _nextId = 1;

            public Task AddAsync(Seat entity)
            {
                if (entity.Id == 0) entity.Id = _nextId++;
                _seats.Add(entity);
                return Task.CompletedTask;
            }

            public Task AddRangeAsync(IEnumerable<Seat> entities)
            {
                foreach (var seat in entities)
                {
                    if (seat.Id == 0) seat.Id = _nextId++;
                    _seats.Add(seat);
                }
                return Task.CompletedTask;
            }

            public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Seat, bool>> predicate)
                => Task.FromResult(_seats.AsQueryable().Any(predicate));

            public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Seat, bool>> predicate)
                => Task.FromResult(_seats.AsQueryable().Count(predicate));

            public Task<IEnumerable<Seat>> FindAsync(System.Linq.Expressions.Expression<Func<Seat, bool>> predicate)
                => Task.FromResult<IEnumerable<Seat>>(_seats.AsQueryable().Where(predicate).ToList());

            public Task<IEnumerable<Seat>> GetAllAsync() => Task.FromResult<IEnumerable<Seat>>(_seats);

            public Task<Seat?> GetByIdAsync(int id) => Task.FromResult(_seats.FirstOrDefault(s => s.Id == id));

            public Task<IEnumerable<Seat>> GetByResourceIdAsync(int resourceId)
                => Task.FromResult<IEnumerable<Seat>>(_seats.Where(s => s.ResourceId == resourceId).ToList());

            public Task<Seat?> GetSeatWithResourceAsync(int seatId) => GetByIdAsync(seatId);

            public Task<Seat?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Seat, bool>> predicate)
                => Task.FromResult(_seats.AsQueryable().FirstOrDefault(predicate));

            public Task RemoveAsync(Seat entity)
            {
                _seats.Remove(entity);
                return Task.CompletedTask;
            }

            public void Remove(Seat entity) => _seats.Remove(entity);

            public void RemoveRange(IEnumerable<Seat> entities)
            {
                foreach (var seat in entities.ToList())
                {
                    _seats.Remove(seat);
                }
            }

            public Task<int> SaveAsync() => Task.FromResult(0);

            public void Update(Seat entity)
            {
                var idx = _seats.FindIndex(s => s.Id == entity.Id);
                if (idx >= 0) _seats[idx] = entity;
            }
        }

        private sealed class InMemoryReservationRepository : IReservationRepository
        {
            private readonly List<Reservation> _reservations = new();
            private int _nextId = 1;

            public Task AddAsync(Reservation entity)
            {
                if (entity.Id == 0) entity.Id = _nextId++;
                _reservations.Add(entity);
                return Task.CompletedTask;
            }

            public Task AddRangeAsync(IEnumerable<Reservation> entities)
            {
                foreach (var r in entities)
                {
                    if (r.Id == 0) r.Id = _nextId++;
                    _reservations.Add(r);
                }
                return Task.CompletedTask;
            }

            public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Reservation, bool>> predicate)
                => Task.FromResult(_reservations.AsQueryable().Any(predicate));

            public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Reservation, bool>> predicate)
                => Task.FromResult(_reservations.AsQueryable().Count(predicate));

            public Task<IEnumerable<Reservation>> FindAsync(System.Linq.Expressions.Expression<Func<Reservation, bool>> predicate)
                => Task.FromResult<IEnumerable<Reservation>>(_reservations.AsQueryable().Where(predicate).ToList());

            public Task<IEnumerable<Reservation>> GetAllAsync() => Task.FromResult<IEnumerable<Reservation>>(_reservations);

            public Task<Reservation?> GetByIdAsync(int id) => Task.FromResult(_reservations.FirstOrDefault(r => r.Id == id));

            public Task<IEnumerable<Reservation>> GetByResourceIdAsync(int resourceId, DateTime? from = null, DateTime? to = null)
                => Task.FromResult<IEnumerable<Reservation>>(_reservations.Where(r => r.ResourceId == resourceId).ToList());

            public Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId)
                => Task.FromResult<IEnumerable<Reservation>>(_reservations.Where(r => r.UserId == userId).ToList());

            public Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
                => Task.FromResult<IEnumerable<Reservation>>(_reservations.Where(r => r.Status != ReservationStatus.Cancelled));

            public Task<Reservation?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Reservation, bool>> predicate)
                => Task.FromResult(_reservations.AsQueryable().FirstOrDefault(predicate));

            public Task<bool> HasConflictAsync(int resourceId, int seatId, DateTime start, DateTime end, int? excludeReservationId = null)
            {
                var conflict = _reservations.Any(r =>
                    r.ResourceId == resourceId &&
                    r.SeatId == seatId &&
                    r.Id != excludeReservationId &&
                    r.StartTime < end && r.EndTime > start);
                return Task.FromResult(conflict);
            }

            public void Remove(Reservation entity) => _reservations.Remove(entity);

            public void RemoveRange(IEnumerable<Reservation> entities)
            {
                foreach (var r in entities.ToList()) _reservations.Remove(r);
            }

            public Task<int> SaveAsync() => Task.FromResult(0);

            public void Update(Reservation entity)
            {
                var idx = _reservations.FindIndex(r => r.Id == entity.Id);
                if (idx >= 0) _reservations[idx] = entity;
            }
        }
    }
}
