using Microsoft.Extensions.Logging;
using Moq;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using UniversalReservationMVC.Services;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class SeatMapServiceTests
    {
        [Fact]
        public async Task SaveSeatMapAsync_ThrowsOnDuplicateCoordinates()
        {
            var unit = new InMemoryUnitOfWork();
            var logger = Mock.Of<ILogger<SeatMapService>>();
            var service = new SeatMapService(unit, logger);

            var seats = new List<Seat>
            {
                new Seat { X = 1, Y = 1, IsAvailable = true },
                new Seat { X = 1, Y = 1, IsAvailable = true }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveSeatMapAsync(5, seats));
        }

        [Fact]
        public async Task SaveSeatMapAsync_RemovesMissingAndAddsNew()
        {
            var unit = new InMemoryUnitOfWork();
            // existing seat at (1,1)
            await unit.Seats.AddAsync(new Seat { ResourceId = 9, X = 1, Y = 1, Label = "A1", IsAvailable = true });

            var logger = Mock.Of<ILogger<SeatMapService>>();
            var service = new SeatMapService(unit, logger);

            var incoming = new List<Seat>
            {
                new Seat { X = 2, Y = 1, IsAvailable = true }, // new
                new Seat { X = 3, Y = 1, IsAvailable = false } // new unavailable
            };

            await service.SaveSeatMapAsync(9, incoming);

            var seats = (await unit.Seats.GetByResourceIdAsync(9)).OrderBy(s => s.X).ToList();
            Assert.Equal(2, seats.Count); // old (1,1) removed
            Assert.Equal(2, seats[0].X);
            Assert.True(seats[0].IsAvailable);
            Assert.Equal(3, seats[1].X);
            Assert.False(seats[1].IsAvailable);
        }

        private sealed class InMemoryUnitOfWork : IUnitOfWork
        {
            public InMemoryUnitOfWork()
            {
                Seats = new InMemorySeatRepository();
            }

            public IReservationRepository Reservations => throw new NotImplementedException();
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
                entity.Id = _nextId++;
                _seats.Add(entity);
                return Task.CompletedTask;
            }

            public Task AddRangeAsync(IEnumerable<Seat> entities)
            {
                foreach (var seat in entities)
                {
                    seat.Id = _nextId++;
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
                if (idx >= 0)
                {
                    _seats[idx] = entity;
                }
            }
        }
    }
}
