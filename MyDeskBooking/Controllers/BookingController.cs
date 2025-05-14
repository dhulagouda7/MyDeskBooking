using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Services.BusinessLogic;
using BookMyDesk.Services.DataAccess;

namespace BookMyDesk.Controllers
{
    [Authorize]    
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingValidationService _validationService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IRepository<Floor> _floorRepository;

        public BookingController(
            IBookingService bookingService,
            IBookingValidationService validationService,
            IRepository<User> userRepository,
            IRepository<Location> locationRepository,
            IRepository<Building> buildingRepository,
            IRepository<Floor> floorRepository)
        {
            _bookingService = bookingService;
            _validationService = validationService;
            _userRepository = userRepository;
            _locationRepository = locationRepository;
            _buildingRepository = buildingRepository;
            _floorRepository = floorRepository;
        }

        public async Task<ActionResult> Index()
        {
            var userId = await GetCurrentUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        public async Task<ActionResult> Available(DateTime? date, TimeSpan? startTime, TimeSpan? endTime, 
            int? locationId = null, int? buildingId = null, int? floorId = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;
            if (!startTime.HasValue)
                startTime = new TimeSpan(9, 0, 0); // 9 AM
            if (!endTime.HasValue)
                endTime = new TimeSpan(17, 0, 0); // 5 PM

            var availableDesks = await _bookingService.GetAvailableDesksAsync(date.Value, startTime.Value, endTime.Value);

            // Apply filters if provided
            if (floorId.HasValue)
            {
                availableDesks = availableDesks.Where(d => d.FloorId == floorId);
            }
            else if (buildingId.HasValue)
            {
                availableDesks = availableDesks.Where(d => d.Floor.BuildingId == buildingId);
            }
            else if (locationId.HasValue)
            {
                availableDesks = availableDesks.Where(d => d.Floor.Building.LocationId == locationId);
            }

            ViewBag.Locations = await _locationRepository.GetAllAsync();
            ViewBag.Buildings = await _buildingRepository.GetAllAsync();
            ViewBag.Floors = await _floorRepository.GetAllAsync();

            return View(availableDesks);
        }        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var userId = await GetCurrentUserId();

                // Validate the booking request
                var (isValid, errors) = await _validationService.ValidateBookingAsync(userId, deskId, date, startTime, endTime);
                if (!isValid)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return RedirectToAction("Available", new { date, startTime, endTime });
                }

                var booking = await _bookingService.CreateBookingAsync(userId, deskId, date, startTime, endTime);
                TempData["SuccessMessage"] = "Booking created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Available");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancel(int bookingId)
        {
            try
            {
                var userId = await GetCurrentUserId();
                await _bookingService.CancelBookingAsync(bookingId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckIn(int bookingId)
        {
            try
            {
                var userId = await GetCurrentUserId();
                await _bookingService.CheckInAsync(bookingId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckOut(int bookingId)
        {
            try
            {
                var userId = await GetCurrentUserId();
                await _bookingService.CheckOutAsync(bookingId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index");
            }
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
