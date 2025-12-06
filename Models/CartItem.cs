namespace VirtualEventTicketingSystem.Models;

public class CartItem
{
    public int EventId { get; set; }
    public Event Event { get; set; }

    public string Title { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableTickets { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => Quantity * TicketPrice;
}