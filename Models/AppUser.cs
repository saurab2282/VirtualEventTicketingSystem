using Microsoft.AspNetCore.Identity;

namespace VirtualEventTicketingSystem.Models;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
    public string? ProfilePictureUrl { get; set; } // store filename path in wwwroot/uploads
   
   
}