using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Models.ViewModels;
using MyDeskBooking.Services.DataAccess;
using MyDeskBooking.Services.BusinessLogic;
using ClosedXML.Excel;
using System.IO;

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

        private MemoryStream GenerateExcelReport(BookingReportViewModel report)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Booking Report");

                // Add header
                worksheet.Cell(1, 1).Value = report.IsMonthlyReport ? "Monthly Booking Report" : "Daily Booking Report";
                worksheet.Cell(2, 1).Value = "Report Date:";
                worksheet.Cell(2, 2).Value = report.ReportDate.ToString("dd-MMM-yyyy");
                worksheet.Cell(3, 1).Value = "Total Bookings:";
                worksheet.Cell(3, 2).Value = report.TotalBookings;

                // Add column headers
                worksheet.Cell(5, 1).Value = "Location";
                worksheet.Cell(5, 2).Value = "Building";
                worksheet.Cell(5, 3).Value = "Floor";
                worksheet.Cell(5, 4).Value = "Total Desks";
                worksheet.Cell(5, 5).Value = "Booked Desks";
                worksheet.Cell(5, 6).Value = "Occupancy Rate";

                // Add data
                int row = 6;
                foreach (var location in report.LocationStats)
                {
                    foreach (var building in location.BuildingStats)
                    {
                        foreach (var floor in building.FloorStats)
                        {
                            worksheet.Cell(row, 1).Value = location.LocationName;
                            worksheet.Cell(row, 2).Value = building.BuildingName;
                            worksheet.Cell(row, 3).Value = $"Floor {floor.FloorNumber}";
                            worksheet.Cell(row, 4).Value = floor.TotalDesks;
                            worksheet.Cell(row, 5).Value = floor.BookedDesks;
                            worksheet.Cell(row, 6).Value = floor.TotalDesks > 0
                                ? (double)floor.BookedDesks / floor.TotalDesks
                                : 0;
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "0.00%";
                            row++;
                        }
                    }
                }

                // Format header
                var headerRange = worksheet.Range(1, 1, 3, 2);
                headerRange.Style.Font.Bold = true;

                // Format column headers
                var columnHeaderRange = worksheet.Range(5, 1, 5, 6);
                columnHeaderRange.Style.Font.Bold = true;
                columnHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                // Save to MemoryStream
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        // Add these action methods to generate Excel reports
        public async Task<ActionResult> ExportDailyExcel(DateTime? date)
        {
            var report = await Daily(date) as ViewResult;
            var model = report.Model as BookingReportViewModel;

            using (var stream = GenerateExcelReport(model))
            {
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DailyBookingReport_{model.ReportDate:yyyy-MM-dd}.xlsx"
                );
            }
        }

    }
}
