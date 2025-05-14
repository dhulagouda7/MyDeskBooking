using System.Collections.Generic;
using MyDeskBooking.Models.EntityModels;

namespace MyDeskBooking.Models.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Booking> CurrentBookings { get; set; }
        public IEnumerable<Desk> AvailableDesks { get; set; }
    }
}
