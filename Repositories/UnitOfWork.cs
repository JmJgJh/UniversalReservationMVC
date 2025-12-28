using Microsoft.EntityFrameworkCore.Storage;
using UniversalReservationMVC.Data;

namespace UniversalReservationMVC.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IReservationRepository Reservations { get; }
        public IResourceRepository Resources { get; }
        public IEventRepository Events { get; }
        public ITicketRepository Tickets { get; }
        public ISeatRepository Seats { get; }
        public ICompanyRepository Companies { get; }
        public ICompanyMemberRepository CompanyMembers { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IReservationRepository reservations,
            IResourceRepository resources,
            IEventRepository events,
            ITicketRepository tickets,
            ISeatRepository seats,
            ICompanyRepository companies,
            ICompanyMemberRepository companyMembers)
        {
            _context = context;
            Reservations = reservations;
            Resources = resources;
            Events = events;
            Tickets = tickets;
            Seats = seats;
            Companies = companies;
            CompanyMembers = companyMembers;
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
