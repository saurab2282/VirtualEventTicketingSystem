namespace VirtualEventTicketingSystem.Models;

public class Cart
{
    public List<CartItem> Items { get; set; } = new();

    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal TotalPrice => Items.Sum(i => i.LineTotal);
}

