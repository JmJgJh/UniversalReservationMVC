using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _db;
        public TicketService(ApplicationDbContext db) => _db = db;

        public async Task<Ticket> BuyTicket(int reservationId, decimal price, string? purchaserId = null)
        {
            var reservation = await _db.Reservations.FindAsync(reservationId);
            if (reservation == null) throw new KeyNotFoundException("Reservation not found.");

            var ticket = new Ticket
            {
                ReservationId = reservationId,
                Price = price,
                Status = TicketStatus.Purchased,
                PurchaseReference = Guid.NewGuid().ToString()
            };
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return ticket;
        }

        public async Task CancelTicket(int ticketId)
        {
            var t = await _db.Tickets.FindAsync(ticketId);
            if (t == null) throw new KeyNotFoundException("Ticket not found.");
            t.Status = TicketStatus.Cancelled;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsForUser(string userId)
        {
            return await _db.Tickets
                .Include(t => t.Reservation)
                .ThenInclude(r => r.Resource)
                .Where(t => t.Reservation != null && t.Reservation.UserId == userId)
                .ToListAsync();
        }
    }
}
