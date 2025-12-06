using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace VirtualEventTicketingSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<EventPurchase> EventPurchases { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for Identity

            // --------------------------
            // Global DateTime UTC converter
            // --------------------------
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // store as UTC
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // read as UTC
                );
            

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProps = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime));

                foreach (var prop in dateTimeProps)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(prop.Name)
                        .HasConversion(dateTimeConverter);
                }
            }

            // --------------------------
            // EventPurchase composite key
            // --------------------------
            modelBuilder.Entity<EventPurchase>()
                .HasKey(ep => new { ep.EventId, ep.PurchaseId });

            // --------------------------
            // Category → Events relationship
            // --------------------------
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Events)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            // --------------------------
            // Seed categories
            // --------------------------
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Webinar", Description = "Online educational sessions" },
                new Category { Id = 2, Name = "Concert", Description = "Live musical performances" },
                new Category { Id = 3, Name = "Workshop", Description = "Interactive training sessions" },
                new Category { Id = 4, Name = "Conference", Description = "Professional Meetings" }
            );

            // Do NOT seed events here — we'll seed them at runtime
        }
    }
}