using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Controllers
{
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

        // GET: /Analytics/MyAnalytics
        public IActionResult MyAnalytics()
        {
            return View();
        }

        // JSON endpoint for ticket sales by category
        [HttpGet]
        public async Task<IActionResult> GetTicketSalesByCategory()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            bool isOrganizer = roles.Contains("Organizer");

            var categories = await _context.Categories.ToListAsync();
            var sales = new List<object>();

            foreach (var c in categories)
            {
                var ticketsQuery = _context.EventPurchases
                    .Include(ep => ep.Event)
                    .Where(ep => ep.Event.CategoryId == c.Id);

                if (isOrganizer)
                {
                    ticketsQuery = ticketsQuery.Where(ep => ep.Event.OrganizerId == user.Id);
                }

                var ticketsSold = await ticketsQuery.CountAsync();

                sales.Add(new
                {
                    Category = c.Name,
                    TicketsSold = ticketsSold
                });
            }

            return Json(sales);
        }

        // JSON endpoint for revenue per month
        [HttpGet]
        public async Task<IActionResult> GetRevenuePerMonth()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            bool isOrganizer = roles.Contains("Organizer");

            var query = _context.EventPurchases
                .Include(ep => ep.Event)
                .Include(ep => ep.Purchase)
                .AsQueryable();

            if (isOrganizer)
            {
                query = query.Where(ep => ep.Event.OrganizerId == user.Id);
            }

            var revenue = await query
                .GroupBy(ep => new { ep.Purchase.PurchaseDate.Year, ep.Purchase.PurchaseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(ep => ep.Purchase.Amount)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            return Json(revenue);
        }

        // JSON endpoint for Top 5 Best-Selling Events
        [HttpGet]
        public async Task<IActionResult> GetTopEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            bool isOrganizer = roles.Contains("Organizer");

            var eventsQuery = _context.Events.AsQueryable();

            if (isOrganizer)
            {
                eventsQuery = eventsQuery.Where(e => e.OrganizerId == user.Id);
            }

            var topEvents = await eventsQuery
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
}