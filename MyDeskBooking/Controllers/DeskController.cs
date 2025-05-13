using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Models.ViewModels;
using BookMyDesk.Services.DataAccess;

namespace BookMyDesk.Controllers
{
    [Authorize]
    public class DeskController : Controller
    {
        private readonly IRepository<Desk> _deskRepository;
        private readonly IRepository<Floor> _floorRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IBookingRepository _bookingRepository;

        public DeskController(
            IRepository<Desk> deskRepository,
            IRepository<Floor> floorRepository,
            IRepository<Building> buildingRepository,
            IBookingRepository bookingRepository)
        {
            _deskRepository = deskRepository;
            _floorRepository = floorRepository;
            _buildingRepository = buildingRepository;
            _bookingRepository = bookingRepository;
        }

        // GET: Desk
        public async Task<ActionResult> Index(int? floorId = null, int? buildingId = null)
        {
            var desks = (await _deskRepository.GetAllAsync())
                .Where(d => !floorId.HasValue || d.FloorId == floorId)
                .Where(d => !buildingId.HasValue || d.Floor.BuildingId == buildingId)
                .OrderBy(d => d.Floor.Building.BuildingName)
                .ThenBy(d => d.Floor.FloorNumber)
                .ThenBy(d => d.DeskNumber);

            var floors = await _floorRepository.GetAllAsync();
            var buildings = await _buildingRepository.GetAllAsync();

            // Filter floors based on selected building
            if (buildingId.HasValue)
            {
                floors = floors.Where(f => f.BuildingId == buildingId);
            }

            ViewBag.Buildings = new SelectList(buildings, "BuildingId", "BuildingName", buildingId);
            ViewBag.Floors = new SelectList(floors, "FloorId", "FloorNumber", floorId);

            if (buildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId.Value);
                ViewBag.BuildingName = building?.BuildingName;
            }

            if (floorId.HasValue)
            {
                var floor = await _floorRepository.GetByIdAsync(floorId.Value);
                ViewBag.FloorNumber = floor?.FloorNumber;
            }

            return View(desks);
        }

        // GET: Desk/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(int? floorId = null, int? buildingId = null)
        {
            var buildings = await _buildingRepository.GetAllAsync();
            ViewBag.Buildings = new SelectList(buildings, "BuildingId", "BuildingName", buildingId);

            var floors = await _floorRepository.GetAllAsync();
            if (buildingId.HasValue)
            {
                floors = floors.Where(f => f.BuildingId == buildingId);
            }
            ViewBag.Floors = new SelectList(floors, "FloorId", "FloorNumber", floorId);

            return View(new Desk { FloorId = floorId ?? 0, Status = "Available" });
        }

        // POST: Desk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Desk desk)
        {
            if (ModelState.IsValid)
            {
                // Check if desk number already exists on the floor
                var existingDesks = await _deskRepository.GetAllAsync();
                if (existingDesks.Any(d => d.FloorId == desk.FloorId && 
                                         d.DeskNumber == desk.DeskNumber))
                {
                    ModelState.AddModelError("", "This desk number already exists on the selected floor");
                }
                else
                {
                    await _deskRepository.AddAsync(desk);
                    TempData["SuccessMessage"] = "Desk added successfully!";
                    return RedirectToAction("Index", new { floorId = desk.FloorId });
                }
            }

            var buildings = await _buildingRepository.GetAllAsync();
            ViewBag.Buildings = new SelectList(buildings, "BuildingId", "BuildingName");

            var floors = await _floorRepository.GetAllAsync();
            ViewBag.Floors = new SelectList(floors, "FloorId", "FloorNumber", desk.FloorId);

            return View(desk);
        }

        // GET: Desk/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int id)
        {
            var desk = await _deskRepository.GetByIdAsync(id);
            if (desk == null)
            {
                return HttpNotFound();
            }

            var buildings = await _buildingRepository.GetAllAsync();
            ViewBag.Buildings = new SelectList(buildings, "BuildingId", "BuildingName", desk.Floor.BuildingId);

            var floors = await _floorRepository.GetAllAsync();
            ViewBag.Floors = new SelectList(floors, "FloorId", "FloorNumber", desk.FloorId);

            return View(desk);
        }

        // POST: Desk/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Desk desk)
        {
            if (ModelState.IsValid)
            {
                // Check if desk number already exists on the floor
                var existingDesks = await _deskRepository.GetAllAsync();
                if (existingDesks.Any(d => d.FloorId == desk.FloorId && 
                                         d.DeskNumber == desk.DeskNumber &&
                                         d.DeskId != desk.DeskId))
                {
                    ModelState.AddModelError("", "This desk number already exists on the selected floor");
                }
                else
                {
                    await _deskRepository.UpdateAsync(desk);
                    TempData["SuccessMessage"] = "Desk updated successfully!";
                    return RedirectToAction("Index", new { floorId = desk.FloorId });
                }
            }

            var buildings = await _buildingRepository.GetAllAsync();
            ViewBag.Buildings = new SelectList(buildings, "BuildingId", "BuildingName");

            var floors = await _floorRepository.GetAllAsync();
            ViewBag.Floors = new SelectList(floors, "FloorId", "FloorNumber", desk.FloorId);

            return View(desk);
        }

        // POST: Desk/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var desk = await _deskRepository.GetByIdAsync(id);
            if (desk == null)
            {
                return HttpNotFound();
            }

            // Check for active bookings
            var activeBookings = (await _bookingRepository.GetAllAsync())
                .Where(b => b.DeskId == id && 
                           (b.Status == "Pending" || b.Status == "CheckedIn"));

            if (activeBookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete desk - there are active bookings for this desk.";
                return RedirectToAction("Index", new { floorId = desk.FloorId });
            }

            await _deskRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Desk deleted successfully!";
            return RedirectToAction("Index", new { floorId = desk.FloorId });
        }

        // GET: Desk/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var desk = await _deskRepository.GetByIdAsync(id);
            if (desk == null)
            {
                return HttpNotFound();
            }

            var activeBookings = (await _bookingRepository.GetAllAsync())
                .Where(b => b.DeskId == id &&
                           b.BookingDate.Date >= DateTime.Today &&
                           (b.Status == "Pending" || b.Status == "CheckedIn"))
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime);

            ViewBag.ActiveBookings = activeBookings;

            return View(desk);
        }

        // POST: Desk/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeStatus(int id, string status)
        {
            var desk = await _deskRepository.GetByIdAsync(id);
            if (desk == null)
            {
                return HttpNotFound();
            }

            if (status != "Available" && status != "Maintenance" && status != "Reserved")
            {
                ModelState.AddModelError("", "Invalid status value");
                return RedirectToAction("Index", new { floorId = desk.FloorId });
            }

            desk.Status = status;
            await _deskRepository.UpdateAsync(desk);
            TempData["SuccessMessage"] = "Desk status updated successfully!";
            return RedirectToAction("Index", new { floorId = desk.FloorId });
        }
    }
}
