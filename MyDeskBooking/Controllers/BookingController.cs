using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Services.BusinessLogic;

namespace BookMyDesk.Controllers
{
    [Authorize]    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingValidationService _validationService;

        public BookingController(
            IBookingService bookingService,
            IBookingValidationService validationService)
        {
            _bookingService = bookingService;
            _validationService = validationService;
        }

        public async Task<ActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        public async Task<ActionResult> Available(DateTime? date, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (!date.HasValue)
                date = DateTime.Today;
            if (!startTime.HasValue)
                startTime = new TimeSpan(9, 0, 0); // 9 AM
            if (!endTime.HasValue)
                endTime = new TimeSpan(17, 0, 0); // 5 PM

            var availableDesks = await _bookingService.GetAvailableDesksAsync(date.Value, startTime.Value, endTime.Value);
            return View(availableDesks);
        }        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var userId = GetCurrentUserId();

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
                var userId = GetCurrentUserId();
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
                var userId = GetCurrentUserId();
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
                var userId = GetCurrentUserId();
                await _bookingService.CheckOutAsync(bookingId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            // In a real application, this would get the user ID from the authenticated user's claims
            // For now, we'll return a dummy user ID
            return 1;
        }
    }
}
