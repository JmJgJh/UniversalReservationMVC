using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyMember> CompanyMembers { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Company configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Description).HasMaxLength(1000);
                entity.Property(c => c.Address).HasMaxLength(200);
                entity.Property(c => c.PhoneNumber).HasMaxLength(20);
                entity.Property(c => c.Email).HasMaxLength(100);
                entity.Property(c => c.Website).HasMaxLength(100);
                entity.Property(c => c.LogoUrl).HasMaxLength(500);
                
                entity.HasOne(c => c.Owner)
                    .WithMany()
                    .HasForeignKey(c => c.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Resources)
                    .WithOne(r => r.Company)
                    .HasForeignKey(r => r.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.OwnerId);

                entity.HasMany(c => c.Members)
                    .WithOne(m => m.Company)
                    .HasForeignKey(m => m.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Resource configuration
            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(r => r.ResourceType);
                entity.HasIndex(r => r.CompanyId);
                
                entity.HasOne(r => r.Company)
                    .WithMany(c => c.Resources)
                    .HasForeignKey(r => r.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(r => r.Seats)
                    .WithOne(s => s.Resource)
                    .HasForeignKey(s => s.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seat configuration
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => new { s.ResourceId, s.X, s.Y });
                entity.Property(s => s.Label).HasMaxLength(20);
            });

            // Reservation configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => new { r.ResourceId, r.StartTime, r.EndTime });
                entity.HasIndex(r => r.Status);
                
                entity.HasOne(r => r.Resource)
                    .WithMany()
                    .HasForeignKey(r => r.ResourceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Seat)
                    .WithMany()
                    .HasForeignKey(r => r.SeatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Event)
                    .WithMany()
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(r => r.GuestEmail).HasMaxLength(255);
                entity.Property(r => r.GuestPhone).HasMaxLength(50);
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => new { e.ResourceId, e.StartTime });
                
                entity.HasOne(e => e.Resource)
                    .WithMany()
                    .HasForeignKey(e => e.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.ReservationId);
                entity.HasIndex(t => t.Status);
                
                entity.HasOne(t => t.Reservation)
                    .WithMany()
                    .HasForeignKey(t => t.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(t => t.Price).HasColumnType("decimal(18,2)");
                entity.Property(t => t.PurchaseReference).HasMaxLength(100);
            });

            // CompanyMember configuration (many-to-many zwischen User und Company)
            modelBuilder.Entity<CompanyMember>(entity =>
            {
                entity.HasKey(cm => cm.Id);
                entity.HasIndex(cm => new { cm.CompanyId, cm.UserId }).IsUnique();
                entity.HasIndex(cm => cm.UserId);
                entity.HasIndex(cm => cm.IsActive);

                entity.HasOne(cm => cm.Company)
                    .WithMany(c => c.Members)
                    .HasForeignKey(cm => cm.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cm => cm.User)
                    .WithMany(u => u.CompanyMemberships)
                    .HasForeignKey(cm => cm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(cm => cm.Role).HasMaxLength(50);
            });
        }
    }
}
