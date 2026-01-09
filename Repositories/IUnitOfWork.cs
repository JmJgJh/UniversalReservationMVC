namespace UniversalReservationMVC.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IReservationRepository Reservations { get; }
        IResourceRepository Resources { get; }
        IEventRepository Events { get; }
        ITicketRepository Tickets { get; }
        ISeatRepository Seats { get; }
            ICompanyRepository Companies { get; }
            ICompanyMemberRepository CompanyMembers { get; }
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
