using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Services;

public class RoleSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleSeeder(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedRolesUsersAndEventsAsync(ApplicationDbContext db)
    {
        // Apply migrations
        await db.Database.MigrateAsync();

        // 1️⃣ Ensure roles exist
        string[] roles = { "Admin", "Organizer", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2️⃣ Create admin user
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin@123";
        var admin = await _userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
                throw new Exception("Failed to create Admin user: " +
                                    string.Join(", ", createResult.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        // 3️⃣ Seed events (runtime, after admin exists)
        if (!db.Events.Any())
        {
            db.Events.AddRange(
                new Event {
                    Title = "C# Fundamentals",
                    CategoryId = 1,
                    TicketPrice = 15.99M,
                    AvailableTickets = 10,
                    EventDate = DateTime.SpecifyKind(new DateTime(2025, 2, 15, 10, 0, 0), DateTimeKind.Utc),
                    OrganizerId = admin.Id
                },
                new Event {
                    Title = "Rock Night Live",
                    CategoryId = 2,
                    TicketPrice = 30.00M,
                    AvailableTickets = 3,
                    EventDate = DateTime.SpecifyKind(new DateTime(2025, 2, 20, 19, 0, 0), DateTimeKind.Utc),
                    OrganizerId = admin.Id
                },
                new Event {
                    Title = "UI/UX Workshop",
                    CategoryId = 3,
                    TicketPrice = 25.50M,
                    AvailableTickets = 8,
                    EventDate = DateTime.SpecifyKind(new DateTime(2025, 2, 25, 14, 0, 0), DateTimeKind.Utc),
                    OrganizerId = admin.Id
                }
            );
            await db.SaveChangesAsync();
        }
    }
}