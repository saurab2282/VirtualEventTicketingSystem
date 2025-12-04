using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Controllers;

[Authorize]
public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public PurchasesController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Purchases/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Events = await _context.Events.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Purchase purchase, int[] eventIds, int[] quantities)
    {
        if (eventIds == null || quantities == null || eventIds.Length != quantities.Length)
        {
            ModelState.AddModelError("", "Invalid ticket selection.");
            ViewBag.Events = await _context.Events.ToListAsync();
            return View(purchase);
        }

        purchase.EventPurchases = new List<EventPurchase>();
        decimal totalCost = 0;

        for (int i = 0; i < eventIds.Length; i++)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventIds[i]);
            if (ev == null) continue;

            var quantity = quantities[i];
            if (quantity <= 0) continue;

            totalCost += ev.TicketPrice * quantity;

            purchase.EventPurchases.Add(new EventPurchase
            {
                EventId = ev.Id,
                Event = ev,
                Quantity = quantity
            });
        }

        purchase.TotalCost = totalCost;

        // Ensure all event details are loaded
        foreach (var ep in purchase.EventPurchases)
        {
            ep.Event = await _context.Events.FirstOrDefaultAsync(e => e.Id == ep.EventId);
        }

        return View("Confirmation", purchase);
    }

    // GET: Purchases
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var purchases = await _context.Purchases
            .Where(p => p.UserId == user.Id)
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return View(purchases);
    }

    // POST: Purchases/Confirm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Purchase purchase, int[] eventIds, int[] quantities)
    {
        if (eventIds == null || quantities == null || eventIds.Length != quantities.Length)
        {
            ModelState.AddModelError("", "Invalid event selection.");
            return View("Confirmation", purchase);
        }

        var user = await _userManager.GetUserAsync(User);

        // Create a new purchase record linked to logged-in user
        var newPurchase = new Purchase
        {
            UserId = user.Id,
            GuestName = user.UserName,
            GuestEmail = user.Email,
            PurchaseDate = DateTime.UtcNow,
            TotalCost = 0
        };

        _context.Purchases.Add(newPurchase);
        await _context.SaveChangesAsync(); // commit to get PurchaseId

        for (int i = 0; i < eventIds.Length; i++)
        {
            int eventId = eventIds[i];
            int qty = quantities[i];

            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
            {
                ModelState.AddModelError("", $"Event with ID {eventId} not found.");
                return View("Confirmation", purchase);
            }

            if (qty < 1)
            {
                ModelState.AddModelError("", $"Invalid ticket quantity for {ev.Title}.");
                return View("Confirmation", purchase);
            }

            if (ev.AvailableTickets < qty)
            {
                ModelState.AddModelError("", $"Not enough tickets left for {ev.Title}. Only {ev.AvailableTickets} remaining.");
                return View("Confirmation", purchase);
            }

            // Deduct tickets
            ev.AvailableTickets -= qty;
            _context.Events.Attach(ev);
            _context.Entry(ev).Property(e => e.AvailableTickets).IsModified = true;

            // Add the EventPurchase
            _context.EventPurchases.Add(new EventPurchase
            {
                PurchaseId = newPurchase.PurchaseId,
                EventId = ev.Id,
                Quantity = qty
            });

            newPurchase.TotalCost += ev.TicketPrice * qty;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Confirmed");
    }

    // GET: Purchases/Confirmed
    public IActionResult Confirmed()
    {
        return View();
    }

    // GET: Purchases/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

        if (purchase == null) return NotFound();

        return View(purchase);
    }

    // POST: Purchases/DeleteConfirmed/5
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

        if (purchase == null) return NotFound();

        // Restore tickets
        foreach (var ep in purchase.EventPurchases)
        {
            if (ep.Event != null)
            {
                ep.Event.AvailableTickets += ep.Quantity;
                _context.Events.Update(ep.Event);
            }
        }

        // Remove related EventPurchases
        _context.EventPurchases.RemoveRange(purchase.EventPurchases);

        // Remove purchase
        _context.Purchases.Remove(purchase);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Purchase deleted and tickets restored successfully.";
        return RedirectToAction(nameof(Index));
    }
}