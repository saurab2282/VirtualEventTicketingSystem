using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VirtualEventTicketingSystem.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }

        // Link to logged-in user
        public string UserId { get; set; }
        public AppUser User { get; set; }
        // Guest details
        [Required]
        public string GuestName { get; set; }

        [Required, EmailAddress]
        public string GuestEmail { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public decimal TotalCost { get; set; }

        // List of events purchased
        public List<EventPurchase> EventPurchases { get; set; } = new List<EventPurchase>();
        public int Rating { get; set; } = 0; // default 0
        public decimal? Amount
        { get; set; }
    }
}