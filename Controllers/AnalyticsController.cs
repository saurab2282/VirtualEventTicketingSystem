using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

[Authorize(Roles = "Organizer,Admin")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public AnalyticsController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Events/MyAnalytics
    public IActionResult MyAnalytics()
    {
        return View();
    }

    // JSON endpoint for ticket sales by category
    [HttpGet]
    public async Task<IActionResult> GetTicketSalesByCategory()
    {
        var user = await _userManager.GetUserAsync(User);

        var sales = await _context.Categories
            .Select(c => new 
            {
                Category = c.Name,
                TicketsSold = _context.EventPurchases
                                .Include(ep => ep.Event)
                                .Where(ep => ep.Event.CategoryId == c.Id && 
                                             (user.IsOrganizer ? ep.Event.OrganizerId == user.Id : true))
                                .Count()
            })
            .ToListAsync();

        return Json(sales);
    }

    // JSON endpoint for revenue per month
    [HttpGet]
    public async Task<IActionResult> GetRevenuePerMonth()
    {
        var user = await _userManager.GetUserAsync(User);

        var revenue = await _context.EventPurchases
            .Include(ep => ep.Event)
            .Include(ep => ep.Purchase)
            .Where(ep => user.IsOrganizer ? ep.Event.OrganizerId == user.Id : true)
            .GroupBy(ep => new { ep.Purchase.PurchaseDate.Year, ep.Purchase.PurchaseDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(ep => ep.Purchase.Amount)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return Json(revenue);
    }

    // JSON endpoint for Top 5 Best-Selling Events
    [HttpGet]
    public async Task<IActionResult> GetTopEvents()
    {
        var user = await _userManager.GetUserAsync(User);

        var topEvents = await _context.Events
            .Where(e => user.IsOrganizer ? e.OrganizerId == user.Id : true)
            .Select(e => new
            {
                e.Id,
                e.Title,
                TicketsSold = _context.EventPurchases.Count(ep => ep.EventId == e.Id),
                Revenue = _context.EventPurchases
                            .Where(ep => ep.EventId == e.Id)
                            .Sum(ep => (decimal?)ep.Purchase.Amount) ?? 0
            })
            .OrderByDescending(e => e.TicketsSold)
            .Take(5)
            .ToListAsync();

        return Json(topEvents);
    }
}