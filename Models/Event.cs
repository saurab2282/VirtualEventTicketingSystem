namespace VirtualEventTicketingSystem.Models;



public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime EventDate { get; set; }= DateTime.UtcNow;
    public decimal TicketPrice { get; set; }
    public int AvailableTickets { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public string OrganizerId { get; set; } = null!;
    public AppUser Organizer { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public DateTime DateTime { get; set; } = DateTime.Now;
    public decimal TotalRevenue { get; set; }
}
