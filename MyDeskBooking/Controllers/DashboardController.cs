using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookMyDesk.Models.ViewModels;
using BookMyDesk.Services.BusinessLogic;

namespace BookMyDesk.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBookingService _bookingService;

        public DashboardController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<ActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var userBookings = await _bookingService.GetUserBookingsAsync(userId);

            var viewModel = new DashboardViewModel
            {
                CurrentBookings = userBookings,
                AvailableDesks = await _bookingService.GetAvailableDesksAsync(
                    DateTime.Today,
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(17, 0, 0)
                )
            };

            return View(viewModel);
        }

        private int GetCurrentUserId()
        {
            // In a real application, this would get the user ID from the authenticated user's claims
            // For now, we'll return a dummy user ID
            return 1;
        }
    }
}
