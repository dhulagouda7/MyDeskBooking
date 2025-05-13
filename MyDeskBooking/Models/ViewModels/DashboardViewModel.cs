using System.Collections.Generic;
using BookMyDesk.Models.EntityModels;

namespace BookMyDesk.Models.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Booking> CurrentBookings { get; set; }
        public IEnumerable<Desk> AvailableDesks { get; set; }
    }
}
