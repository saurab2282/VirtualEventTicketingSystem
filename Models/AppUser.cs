using Microsoft.AspNetCore.Identity;

namespace VirtualEventTicketingSystem.Models;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ProfilePicture { get; set; } // store filename path in wwwroot/uploads
}