using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VirtualEventTicketingSystem.Models;
using VirtualEventTicketingSystem.Sessions;

namespace VirtualEventTicketingSystem.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Cart GetCart()
    {
        var cart = HttpContext.Session.GetObject<Cart>("CART");
        if (cart == null)
        {
            cart = new Cart();
            HttpContext.Session.SetObject("CART", cart);
        }
        return cart;
    }

    private void SaveCart(Cart cart)
    {
        HttpContext.Session.SetObject("CART", cart);
    }

    // -----------------------------
    // ADD TO CART (AJAX)
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> AddToCart(int eventId)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt == null) return NotFound();

        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.EventId == eventId);

        if (item == null)
        {
            cart.Items.Add(new CartItem
            {
                EventId = evt.Id,
                Title = evt.Title,
                TicketPrice = evt.TicketPrice,
                Quantity = 1,
                AvailableTickets = evt.AvailableTickets
            });
        }
        else
        {
            if (item.Quantity < evt.AvailableTickets)
                item.Quantity++;
        }

        SaveCart(cart);

        return Json(new { totalQuantity = cart.TotalQuantity });
    }

    // -----------------------------
    // CART PAGE
    // -----------------------------
    public IActionResult Index()
    {
        return View(GetCart());
    }

    // -----------------------------
    // UPDATE QUANTITY (AJAX)
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int eventId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.EventId == eventId);

        if (item == null)
            return Json(new { success = false, message = "Item not found" });

        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt == null)
            return Json(new { success = false, message = "Event not found" });

        if (quantity < 1)
            quantity = 1;

        if (quantity > evt.AvailableTickets)
            quantity = evt.AvailableTickets;

        item.Quantity = quantity;
        item.AvailableTickets = evt.AvailableTickets;

        SaveCart(cart);

        return Json(new
        {
            success = true,
            lineTotal = item.LineTotal,
            totalQuantity = cart.TotalQuantity,
            totalPrice = cart.TotalPrice,
            available = evt.AvailableTickets
        });
    }

    // -----------------------------
    // REMOVE ITEM (AJAX)
    // -----------------------------
    [HttpPost]
    public IActionResult RemoveItem(int eventId)
    {
        var cart = GetCart();

        var item = cart.Items.FirstOrDefault(i => i.EventId == eventId);
        if (item != null)
            cart.Items.Remove(item);

        SaveCart(cart);

        return Json(new
        {
            success = true,
            totalQuantity = cart.TotalQuantity,
            totalPrice = cart.TotalPrice
        });
    }

    // -----------------------------
    // CHECKOUT (AJAX)
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();
        if (cart == null || !cart.Items.Any())
            return Json(new { success = false, message = "Cart is empty." });

        // start transaction
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Re-load all involved events to check availability and update stock
            var eventIds = cart.Items.Select(i => i.EventId).ToList();
            var events = await _context.Events
                .Where(e => eventIds.Contains(e.Id))
                .ToListAsync();

            // validate availability
            foreach (var ci in cart.Items)
            {
                var ev = events.FirstOrDefault(e => e.Id == ci.EventId);
                if (ev == null)
                    return Json(new { success = false, message = $"Event not found (id={ci.EventId})." });

                if (ci.Quantity > ev.AvailableTickets)
                    return Json(new { success = false, message = $"Not enough tickets for {ev.Title}. Only {ev.AvailableTickets} left." });
            }

            // create purchase
            var purchase = new Purchase
            {
                PurchaseDate = DateTime.UtcNow,
                // Try to set TotalCost or TotalAmount depending on your model name:
                // If your Purchase model has TotalCost use that, otherwise adjust to TotalAmount etc.
                TotalCost = cart.TotalPrice
            };

            // If your Purchase has a UserId and you want to set it for logged-in users:
            // var user = await _userManager.GetUserAsync(User);
            // if (user != null) purchase.UserId = user.Id;

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync(); // so purchase.PurchaseId is populated

            // Add event-purchase (line items) and decrement stock
            foreach (var ci in cart.Items)
            {
                var ev = events.First(e => e.Id == ci.EventId);

                // reduce available tickets
                ev.AvailableTickets -= ci.Quantity;
                _context.Events.Update(ev);

                // Use EventPurchase (your existing join entity)
                var ep = new EventPurchase
                {
                    EventId = ev.Id,
                    PurchaseId = purchase.PurchaseId, // your PK name
                    Quantity = ci.Quantity
                };
                _context.EventPurchases.Add(ep);
            }

            await _context.SaveChangesAsync();
            Log.Information("User {UserId} purchased {Count} items. PurchaseId={PurchaseId}",
                purchase.UserId,
                cart.Items.Count,
                purchase.PurchaseId);
            await tx.CommitAsync();

            // clear cart session
            HttpContext.Session.Remove("CART");

            return Json(new { success = true, purchaseId = purchase.PurchaseId });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            // optionally log ex
            return Json(new { success = false, message = ex.Message });
        }
    }
}
