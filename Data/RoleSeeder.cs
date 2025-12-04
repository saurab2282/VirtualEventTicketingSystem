using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Services
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(
            RoleManager<IdentityRole> roleManager, 
            UserManager<AppUser> userManager,
            ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedRolesAndAdminAsync()
        {
            string[] roles = { "Admin", "Organizer", "Attendee" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = "admin@example.com";
            var admin = await _userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}