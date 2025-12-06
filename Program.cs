using VirtualEventTicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using VirtualEventTicketingSystem.Models;


var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"ENVIRONMENT = {builder.Environment.EnvironmentName}");

// 1. Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
// 2. Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
       // options.SignIn.RequireConfirmedAccount = true;   // ⭐ REQUIRED
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
// ❗ REQUIRED: Add memory cache FIRST
builder.Services.AddDistributedMemoryCache();

// ❗ REQUIRED: Add session BEFORE app.Build()
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 5. MIDDLEWARE (Authentication + Authorization)
app.UseHttpsRedirection();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
// GLOBAL ERROR HANDLING
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePages(async context =>
{
    var code = context.HttpContext.Response.StatusCode;

    switch (code)
    {
        case 404:
            context.HttpContext.Response.Redirect("/Home/Error404");
            break;

        case 500:
            context.HttpContext.Response.Redirect("/Home/Error500");
            break;
    }

    await Task.CompletedTask;
});

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

    // 2. Create Admin user if not exists
    string adminEmail = "admin@example.com";
    string adminPassword = "Admin123!"; // Change for production

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // 3. Optional: Create Organizer user
    string organizerEmail = "organizer@example.com";
    string organizerPassword = "Organizer123!";

    var organizerUser = await userManager.FindByEmailAsync(organizerEmail);
    if (organizerUser == null)
    {
        organizerUser = new AppUser
        {
            UserName = organizerEmail,
            Email = organizerEmail,
            FullName = "Organizer User",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(organizerUser, organizerPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(organizerUser, "Organizer");
        }
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
