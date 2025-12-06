using System.Collections.Generic;

namespace VirtualEventTicketingSystem.Models
{
    public class DashboardViewModel
    {
        public List<EventPurchase> MyTickets { get; set; }
        public List<Purchase> PurchaseHistory { get; set; }
        public List<Event> MyEvents { get; set; }
        public AppUser Profile { get; set; }
    }
}