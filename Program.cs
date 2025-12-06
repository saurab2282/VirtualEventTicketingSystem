using VirtualEventTicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using VirtualEventTicketingSystem.Models;
using VirtualEventTicketingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
// 2. Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;   // ⭐ REQUIRED
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// 3. Add Authorization POLICIES
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrganizerOrAdmin", policy =>
        policy.RequireRole("Organizer", "Admin"));
});

// 4. Add Seeder
builder.Services.AddScoped<RoleSeeder>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 5. MIDDLEWARE (Authentication + Authorization)
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 6. Inline role/user seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Roles
    string[] roles = new[] { "Admin", "Organizer", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Admin user
    var adminEmail = "admin@example.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new AppUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "System Admin",
            IsOrganizer = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Organizer user
    var organizerEmail = "organizer@example.com";
    if (await userManager.FindByEmailAsync(organizerEmail) == null)
    {
        var organizerUser = new AppUser
        {
            UserName = "organizer",
            Email = organizerEmail,
            FullName = "Event Organizer",
            IsOrganizer = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(organizerUser, "Organizer123!");
        await userManager.AddToRoleAsync(organizerUser, "Organizer");
    }

    // Regular user
    var userEmail = "user@example.com";
    if (await userManager.FindByEmailAsync(userEmail) == null)
    {
        var normalUser = new AppUser
        {
            UserName = "user",
            Email = userEmail,
            FullName = "Demo User",
            IsOrganizer = false,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(normalUser, "User123!");
        await userManager.AddToRoleAsync(normalUser, "User");
    }

    await db.SaveChangesAsync();
}

// 7. Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();