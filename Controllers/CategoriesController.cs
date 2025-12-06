using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Controllers;


public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.ToListAsync());
    }

    // GET: Categories/Create
    [Authorize(Policy = "OrganizerOrAdmin")]
    public IActionResult Create() => View();

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Delete/5
    [Authorize(Policy = "OrganizerOrAdmin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    // POST: Categories/Delete
    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "OrganizerOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}