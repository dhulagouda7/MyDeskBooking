using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Models.ViewModels;
using MyDeskBooking.Services.DataAccess;
using MyDeskBooking.Services.BusinessLogic;

namespace MyDeskBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IRepository<Floor> _floorRepository;
        private readonly IRepository<Desk> _deskRepository;

        public ReportController(
            IRepository<Booking> bookingRepository,
            IRepository<Location> locationRepository,
            IRepository<Building> buildingRepository,
            IRepository<Floor> floorRepository,
            IRepository<Desk> deskRepository)
        {
            _bookingRepository = bookingRepository;
            _locationRepository = locationRepository;
            _buildingRepository = buildingRepository;
            _floorRepository = floorRepository;
            _deskRepository = deskRepository;
        }

        // GET: Report
        public async Task<ActionResult> Daily(DateTime? date)
        {
            if (!date.HasValue)
            {
                date = DateTime.Today;
            }

            var bookings = await _bookingRepository.GetAllAsync();
            var locations = await _locationRepository.GetAllAsync();
            var buildings = await _buildingRepository.GetAllAsync();
            var floors = await _floorRepository.GetAllAsync();
            var desks = await _deskRepository.GetAllAsync();

            var dailyBookings = bookings.Where(b => b.BookingDate.Date == date.Value.Date);


            var report = new BookingReportViewModel
            {
                ReportDate = date.Value,
                IsMonthlyReport = false,
                //TotalBookings = dailyBookings.Count()
                TotalBookings = dailyBookings.Select(f => f.DeskId).Distinct().Count()
            };

            foreach (var location in locations)
            {
                var locationStats = new LocationStats
                {
                    LocationId = location.LocationId,
                    LocationName = location.LocationName
                };

                foreach (var building in buildings.Where(b => b.LocationId == location.LocationId))
                {
                    var buildingStats = new BuildingStats
                    {
                        BuildingId = building.BuildingId,
                        BuildingName = building.BuildingName
                    };

                    foreach (var floor in floors.Where(f => f.BuildingId == building.BuildingId))
                    {
                        var floorDesks = desks.Where(d => d.FloorId == floor.FloorId).ToList();
                        var floorBookings = dailyBookings.Where(b => floorDesks.Any(d => d.DeskId == b.DeskId));

                        var floorStats = new FloorStats
                        {
                            FloorId = floor.FloorId,
                            FloorNumber = floor.FloorNumber,
                            TotalDesks = floorDesks.Count,
                            BookedDesks = floorBookings.Select(f => f.DeskId).Distinct().Count()
                        };

                        buildingStats.FloorStats.Add(floorStats);
                        buildingStats.TotalDesks += floorStats.TotalDesks;
                        buildingStats.BookedDesks += floorStats.BookedDesks;
                    }

                    locationStats.BuildingStats.Add(buildingStats);
                    locationStats.TotalDesks += buildingStats.TotalDesks;
                    locationStats.BookedDesks += buildingStats.BookedDesks;
                }

                report.LocationStats.Add(locationStats);
            }

            return View(report);
        }

        // GET: Report/Monthly
        public async Task<ActionResult> Monthly(DateTime? date)
        {
            if (!date.HasValue)
            {
                date = DateTime.Today;
            }
            var startDate = new DateTime(date.Value.Year, date.Value.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bookings = await _bookingRepository.GetAllAsync();
            var locations = await _locationRepository.GetAllAsync();
            var buildings = await _buildingRepository.GetAllAsync();
            var floors = await _floorRepository.GetAllAsync();
            var desks = await _deskRepository.GetAllAsync();

            var monthlyBookings = bookings.Where(b => b.BookingDate.Date >= startDate && b.BookingDate.Date <= endDate);

            var report = new BookingReportViewModel
            {
                ReportDate = date.Value,
                IsMonthlyReport = true,
                TotalBookings = monthlyBookings.Select(f => f.DeskId).Distinct().Count()
            };

            foreach (var location in locations)
            {
                var locationStats = new LocationStats
                {
                    LocationId = location.LocationId,
                    LocationName = location.LocationName
                };

                foreach (var building in buildings.Where(b => b.LocationId == location.LocationId))
                {
                    var buildingStats = new BuildingStats
                    {
                        BuildingId = building.BuildingId,
                        BuildingName = building.BuildingName
                    };

                    foreach (var floor in floors.Where(f => f.BuildingId == building.BuildingId))
                    {
                        var floorDesks = desks.Where(d => d.FloorId == floor.FloorId).ToList();
                        var floorBookings = monthlyBookings.Where(b => floorDesks.Any(d => d.DeskId == b.DeskId));

                        var floorStats = new FloorStats
                        {
                            FloorId = floor.FloorId,
                            FloorNumber = floor.FloorNumber,
                            TotalDesks = floorDesks.Count,
                            BookedDesks = floorBookings.Select(f => f.DeskId).Distinct().Count()
                        };

                        buildingStats.FloorStats.Add(floorStats);
                        buildingStats.TotalDesks += floorStats.TotalDesks;
                        buildingStats.BookedDesks += floorStats.BookedDesks;
                    }

                    locationStats.BuildingStats.Add(buildingStats);
                    locationStats.TotalDesks += buildingStats.TotalDesks;
                    locationStats.BookedDesks += buildingStats.BookedDesks;
                }

                report.LocationStats.Add(locationStats);
            }

            return View(report);
        }
    }
}
