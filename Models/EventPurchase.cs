using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketingSystem.Models
{
    public class EventPurchase
    {
        [Key]
        public int EventPurchaseId { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; }
    }
}