using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketingSystem.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    private DateTime _dateTime;

    [Required]
    public DateTime DateTime 
    { get => _dateTime; set => _dateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); }

    [Range(0, double.MaxValue)]
    public decimal TicketPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableTickets { get; set; }

    // Foreign Key
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<EventPurchase> EventPurchases { get; set; } = new List<EventPurchase>();
}