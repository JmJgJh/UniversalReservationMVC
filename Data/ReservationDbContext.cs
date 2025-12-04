using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Data
{
    // ReservationDbContext used by reservation-related services (separate from Identity ApplicationDbContext).
    public class ReservationDbContext : DbContext
    {
        public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options)
        {
        }

        public DbSet<Resource> Resources { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}
