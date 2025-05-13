using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Services.DataAccess;
using System.Linq;
    namespace BookMyDesk.Services.BusinessLogic
{
    public interface IBookingValidationService
    {
        Task<(bool IsValid, string[] Errors)> ValidateBookingAsync(int userId, int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<bool> CanUserBookDeskAsync(int userId, DateTime date);
        Task<bool> IsDeskAvailableAsync(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime);
    }

    public class BookingValidationService : IBookingValidationService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Desk> _deskRepository;

        public BookingValidationService(
            IBookingRepository bookingRepository,
            IRepository<User> userRepository,
            IRepository<Desk> deskRepository)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _deskRepository = deskRepository;
        }

        public async Task<(bool IsValid, string[] Errors)> ValidateBookingAsync(
            int userId, int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var errors = new List<string>();

            // Check if date is valid
            if (date.Date < DateTime.Today)
            {
                errors.Add("Cannot book desks for past dates");
            }
            if (date.Date > DateTime.Today.AddMonths(3))
            {
                errors.Add("Cannot book desks more than 3 months in advance");
            }

            // Check business hours
            var minTime = new TimeSpan(8, 0, 0); // 8 AM
            var maxTime = new TimeSpan(18, 0, 0); // 6 PM

            if (startTime < minTime || startTime > maxTime)
            {
                errors.Add("Start time must be between 8 AM and 6 PM");
            }
            if (endTime < minTime || endTime > maxTime)
            {
                errors.Add("End time must be between 8 AM and 6 PM");
            }
            if (endTime <= startTime)
            {
                errors.Add("End time must be after start time");
            }

            // Check user booking limits
            if (!await CanUserBookDeskAsync(userId, date))
            {
                errors.Add("You have reached the maximum number of bookings for this date");
            }

            // Check desk availability
            if (!await IsDeskAvailableAsync(deskId, date, startTime, endTime))
            {
                errors.Add("This desk is not available for the selected time period");
            }

            return (errors.Count == 0, errors.ToArray());
        }

        public async Task<bool> CanUserBookDeskAsync(int userId, DateTime date)
        {
            var userBookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var bookingsOnDate = userBookings.Count(b => 
                b.BookingDate.Date == date.Date && 
                b.Status != "Cancelled" && 
                b.Status != "Completed");

            // Users can only have 1 active booking per day
            return bookingsOnDate == 0;
        }

        public async Task<bool> IsDeskAvailableAsync(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Check if desk exists and is available
            var desk = await _deskRepository.GetByIdAsync(deskId);
            if (desk == null || desk.Status != "Available" || desk.IsReserved)
            {
                return false;
            }

            // Check for conflicting bookings
            return await _bookingRepository.IsDeskAvailableAsync(deskId, date, startTime, endTime);
        }
    }
}
