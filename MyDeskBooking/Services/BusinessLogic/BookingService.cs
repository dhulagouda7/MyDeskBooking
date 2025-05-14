using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Services.DataAccess;

namespace MyDeskBooking.Services.BusinessLogic
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<Desk>> GetAvailableDesksAsync(DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<Booking> CreateBookingAsync(int userId, int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task CancelBookingAsync(int bookingId, int userId);
        Task CheckInAsync(int bookingId, int userId);
        Task CheckOutAsync(int bookingId, int userId);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            return await _bookingRepository.GetUserBookingsAsync(userId);
        }

        public async Task<IEnumerable<Desk>> GetAvailableDesksAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (date.Date < DateTime.Today)
                throw new ArgumentException("Cannot book desks for past dates");

            return await _bookingRepository.GetAvailableDesksAsync(date, startTime, endTime);
        }

        public async Task<Booking> CreateBookingAsync(int userId, int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (date.Date < DateTime.Today)
                throw new ArgumentException("Cannot book desks for past dates");

            if (startTime >= endTime)
                throw new ArgumentException("End time must be after start time");

            var isAvailable = await _bookingRepository.IsDeskAvailableAsync(deskId, date, startTime, endTime);
            if (!isAvailable)
                throw new InvalidOperationException("Desk is not available for the selected time period");

            var booking = new Booking
            {
                UserId = userId,
                DeskId = deskId,
                BookingDate = date,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Pending"
            };

            return await _bookingRepository.AddAsync(booking);
        }

        public async Task CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own bookings");

            if (booking.BookingDate.Date < DateTime.Today)
                throw new InvalidOperationException("Cannot cancel past bookings");

            booking.Status = "Cancelled";
            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task CheckInAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only check in to your own bookings");

            if (booking.Status != "Pending")
                throw new InvalidOperationException("Booking is not in a valid state for check-in");

            booking.Status = "CheckedIn";
            booking.CheckInTime = DateTime.Now;
            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task CheckOutAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only check out from your own bookings");

            if (booking.Status != "CheckedIn")
                throw new InvalidOperationException("Booking is not checked in");

            booking.Status = "Completed";
            booking.CheckOutTime = DateTime.Now;
            await _bookingRepository.UpdateAsync(booking);
        }
    }
}
