using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketingSystem.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Event>? Events { get; set; }
}