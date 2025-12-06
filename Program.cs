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

// 6. ⭐ NOW seed database — AFTER middleware is ready!
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();

    await seeder.SeedRolesUsersAndEventsAsync(db);
}

// 7. Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();