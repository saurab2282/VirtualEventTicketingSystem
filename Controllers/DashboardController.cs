using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;
using VirtualEventTicketingSystem.ViewModels;
using QRCoder; // For QR codes
using System.Drawing;
using System.IO;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        var myTickets = await _context.EventPurchases
            .Include(ep => ep.Event)
            .Include(ep => ep.Purchase)
            .Where(ep => ep.Purchase.UserId == user.Id)
            .ToListAsync();

        var purchaseHistory = await _context.EventPurchases
            .Include(ep => ep.Event)
            .Include(ep => ep.Purchase)
            .Where(ep => ep.Purchase.UserId == user.Id)
            .ToListAsync();

        var myEvents = roles.Contains("Organizer")
            ? await _context.Events.Where(e => e.OrganizerId == user.Id).ToListAsync()
            : new List<Event>();

        var model = new DashboardViewModel
        {
            Profile = user,                    // ⚠ Assign the user here
            IsOrganizer = roles.Contains("Organizer"),
            MyTickets = myTickets,
            PurchaseHistory = purchaseHistory,
            MyEvents = myEvents
        };

        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateProfile(IFormFile ProfilePicture, string FullName, string PhoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        user.FullName = FullName;
        user.PhoneNumber = PhoneNumber;

        if (ProfilePicture != null && ProfilePicture.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await ProfilePicture.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/uploads/{fileName}";
        }

        await _userManager.UpdateAsync(user);
        return RedirectToAction("Index");
    }

    // Generate QR code for ticket
    public IActionResult GenerateQr(string code)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        using var bitmap = qrCode.GetGraphic(20);

        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return File(stream.ToArray(), "image/png");
    }

    // Download ticket as PDF (basic version)
    public async Task<IActionResult> DownloadPdf(int eventPurchaseId)
    {
        var ep = await _context.EventPurchases
            .Include(e => e.Event)
            .Include(e => e.Purchase)
            .FirstOrDefaultAsync(e => e.PurchaseId == eventPurchaseId);

        if (ep == null) return NotFound();

        // Replace with a proper PDF generation library if needed
        var pdfBytes = System.Text.Encoding.UTF8.GetBytes($"Ticket for {ep.Event.Title}");
        return File(pdfBytes, "application/pdf", $"Ticket_{ep.PurchaseId}.pdf");
    }
}