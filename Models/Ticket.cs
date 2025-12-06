namespace VirtualEventTicketingSystem.Models;
public class Ticket
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public AppUser User { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; }

    public decimal Price { get; set; }
    public string QRCodeImageUrl { get; set; }
}