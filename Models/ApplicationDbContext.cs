using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VirtualEventTicketingSystem.Models;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<EventPurchase> EventPurchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // important for Identity

        modelBuilder.Entity<EventPurchase>()
            .HasKey(ep => new { ep.EventId, ep.PurchaseId });

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Events)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);

        // seed categories (static)
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Webinar", Description = "Online educational sessions" },
            new Category { Id = 2, Name = "Concert", Description = "Live musical performances" },
            new Category { Id = 3, Name = "Workshop", Description = "Interactive training sessions" },
            new Category { Id = 4, Name = "Conference", Description = "Professional Meetings" }
        );

        // seed events (static dates)
        modelBuilder.Entity<Event>().HasData(
            new Event {
                Id = 1, Title = "C# Fundamentals", CategoryId = 1,
                DateTime = new DateTime(2025, 02, 15, 10, 0, 0),
                TicketPrice = 15.99M, AvailableTickets = 10
            },
            new Event {
                Id = 2, Title = "Rock Night Live", CategoryId = 2,
                DateTime = new DateTime(2025, 02, 20, 19, 0, 0),
                TicketPrice = 30.00M, AvailableTickets = 3
            },
            new Event {
                Id = 3, Title = "UI/UX Workshop", CategoryId = 3,
                DateTime = new DateTime(2025, 02, 25, 14, 0, 0),
                TicketPrice = 25.50M, AvailableTickets = 8
            }
        );
    }
}