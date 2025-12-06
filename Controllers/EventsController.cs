
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Events
    [AllowAnonymous]
    public async Task<IActionResult> Index(string search, int? categoryId, string sortOrder)
    {
        var events = _context.Events.Include(e => e.Category).AsQueryable();

        // Filtering by category
        if (categoryId.HasValue)
        {
            events = events.Where(e => e.CategoryId == categoryId);
        }

        // Searching by title
        if (!string.IsNullOrEmpty(search))
        {
            events = events.Where(e => e.Title.Contains(search));
        }

        // Sorting options
        switch (sortOrder)
        {
            case "title":
                events = events.OrderBy(e => e.Title);
                break;
            case "date":
                events = events.OrderBy(e => e.EventDate);
                break;
            case "price":
                events = events.OrderBy(e => e.TicketPrice);
                break;
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(await events.ToListAsync());
    }

    // GET: Events/Create
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }

    // POST: Events/Create
    [HttpPost]
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event e)
    {
        if (ModelState.IsValid)
        {
            _context.Add(e);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(e);
    }

    // GET: Events/Edit/5
    [HttpGet]
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var e = await _context.Events.FindAsync(id);
        if (e == null) return NotFound();

        ViewBag.Categories = _context.Categories.ToList();
        return View(e);
    }

    // POST: Events/Edit/5
    [HttpPost]
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event e)
    {
        if (id != e.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(e);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(e);
    }

    // GET: Events/Delete/5
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var e = await _context.Events
            .Include(x => x.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (e == null) return NotFound();

        return View(e);
    }

    // POST: Events/Delete/5
    [HttpPost, ActionName("Delete")]
    
    [Authorize(Policy = "OrganizerOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var e = await _context.Events.FindAsync(id);
        _context.Events.Remove(e);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
