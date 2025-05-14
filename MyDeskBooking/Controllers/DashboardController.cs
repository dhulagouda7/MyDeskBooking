using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MyDeskBooking.Models.ViewModels;
using MyDeskBooking.Services.BusinessLogic;
using MyDeskBooking.Services.DataAccess;
using MyDeskBooking.Models.EntityModels;

namespace MyDeskBooking.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IRepository<User> _userRepository;

        public DashboardController(IBookingService bookingService, IRepository<User> userRepository)
        {
            _bookingService = bookingService;
            _userRepository = userRepository;
        }

        public async Task<ActionResult> Index()
        {
            var userId = await GetCurrentUserId();
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

        private async Task<int> GetCurrentUserId()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return user.UserId;
        }
    }
}
