using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BookMyDesk.DataAccess;
using BookMyDesk.Models.EntityModels;

namespace BookMyDesk.Services.DataAccess
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<Desk>> GetAvailableDesksAsync(DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<bool> IsDeskAvailableAsync(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime);
    }

    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            var bookings = _dbSet
                .Where(b => b.UserId == userId)
                .Include(b => b.Desk)
                .OrderByDescending(b => b.BookingDate)
                .ToList();
                
            return Task.FromResult<IEnumerable<Booking>>(bookings);
        }

        public Task<IEnumerable<Desk>> GetAvailableDesksAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var bookedDeskIds = _dbSet
                .Where(b => b.BookingDate == date &&
                           ((b.StartTime <= startTime && b.EndTime > startTime) ||
                            (b.StartTime < endTime && b.EndTime >= endTime) ||
                            (b.StartTime >= startTime && b.EndTime <= endTime)))
                .Select(b => b.DeskId)
                .ToList();

            var availableDesks = _context.Desks
                .Where(d => !bookedDeskIds.Contains(d.DeskId) && 
                           d.Status == "Available" && 
                           !d.IsReserved)
                .Include(d => d.Floor)
                .Include(d => d.Floor.Building)
                .Include(d => d.Floor.Building.Location)
                .ToList();
                
            return Task.FromResult<IEnumerable<Desk>>(availableDesks);
        }

        public Task<bool> IsDeskAvailableAsync(int deskId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var conflictingBookings = _dbSet
                .Any(b => b.DeskId == deskId &&
                              b.BookingDate == date &&
                              ((b.StartTime <= startTime && b.EndTime > startTime) ||
                               (b.StartTime < endTime && b.EndTime >= endTime) ||
                               (b.StartTime >= startTime && b.EndTime <= endTime)));

            if (conflictingBookings)
                return Task.FromResult(false);

            var desk = _context.Desks
                .FirstOrDefault(d => d.DeskId == deskId);            return Task.FromResult(desk != null && 
                   desk.Status == "Available" && 
                   !desk.IsReserved);
        }
    }
}
