using System.Collections.Generic;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.ViewModels
{
    public class DashboardViewModel
    {
        public AppUser Profile { get; set; } = null!;
        public string UserEmail { get; set; }
        public bool IsOrganizer { get; set; }
        public List<EventPurchase> MyTickets { get; set; } = new();
        public List<EventPurchase> PurchaseHistory { get; set; } = new();
        public List<Event> MyEvents { get; set; } = new();
    }
}