using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Models.ViewModels;
using MyDeskBooking.Services.DataAccess;

namespace MyDeskBooking.Controllers
{
    [Authorize]
    public class FloorController : Controller
    {
        private readonly IRepository<Floor> _floorRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IRepository<Desk> _deskRepository;
        private readonly IRepository<Location> _locationRepository;

        public FloorController(
            IRepository<Floor> floorRepository,
            IRepository<Building> buildingRepository,
            IRepository<Desk> deskRepository,
            IRepository<Location> locationRepository)
        {
            _floorRepository = floorRepository;
            _buildingRepository = buildingRepository;
            _deskRepository = deskRepository;
            _locationRepository = locationRepository;
        }

        private SelectList GetBuildingsSelectList(IEnumerable<Building> buildings, int? selectedId = null)
        {
            var items = buildings.Select(b => new
            {
                BuildingId = b.BuildingId,
                BuildingName = b.BuildingName,
                locationid = b.LocationId
            });

            return new SelectList(items, "BuildingId", "BuildingName", selectedId, "locationid");
        }

        // GET: Floor
        public async Task<ActionResult> Index(int? buildingId = null)
        {
            if (buildingId == 0)
                buildingId = null;
            var floors = (await _floorRepository.GetAllAsync())
                .Where(f => !buildingId.HasValue || f.BuildingId == buildingId)
                .OrderBy(f => f.Building.BuildingName)
                .ThenBy(f => f.FloorNumber);

            ViewBag.Buildings = new SelectList(
                await _buildingRepository.GetAllAsync(), 
                "BuildingId", 
                "BuildingName",
                buildingId);

            if (buildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId.Value);
                ViewBag.BuildingName = building?.BuildingName;
            }

            return View(floors);
        }

        // GET: Floor/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(int? locationId = null, int? buildingId = null)
        {
            var buildings = await _buildingRepository.GetAllAsync();
            if (locationId.HasValue)
            {
                buildings = buildings.Where(b => b.LocationId == locationId);
                var location = await _locationRepository.GetByIdAsync(locationId.Value);
                ViewBag.LocationName = location?.LocationName;
            }

            ViewBag.Locations = new SelectList(
                await _locationRepository.GetAllAsync(),
                "LocationId",
                "LocationName",
                locationId);

            ViewBag.Buildings = GetBuildingsSelectList(buildings, buildingId);

            return View(new Floor { BuildingId = buildingId ?? 0 });
        }

        // POST: Floor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Floor floor)
        {
            if (ModelState.IsValid)
            {
                // Check if floor number already exists in the building
                var existingFloors = await _floorRepository.GetAllAsync();
                if (existingFloors.Any(f => f.BuildingId == floor.BuildingId && 
                                          f.FloorNumber == floor.FloorNumber))
                {
                    ModelState.AddModelError("", "This floor number already exists in the selected building");
                }
                else
                {
                    await _floorRepository.AddAsync(floor);
                    TempData["SuccessMessage"] = "Floor added successfully!";
                    return RedirectToAction("Index", new { buildingId = floor.BuildingId });
                }
            }

            ViewBag.Buildings = new SelectList(
                await _buildingRepository.GetAllAsync(), 
                "BuildingId", 
                "BuildingName",
                floor.BuildingId);

            return View(floor);
        }

        // GET: Floor/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int id)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
            {
                return HttpNotFound();
            }

            var buildings = await _buildingRepository.GetAllAsync();
            
            // Get the building's location for pre-selecting
            var building = buildings.FirstOrDefault(b => b.BuildingId == floor.BuildingId);
            var locationId = building?.LocationId;

            ViewBag.Locations = new SelectList(
                await _locationRepository.GetAllAsync(),
                "LocationId",
                "LocationName",
                locationId);

            ViewBag.Buildings = GetBuildingsSelectList(buildings, floor.BuildingId);

            return View(floor);
        }

        // POST: Floor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Floor floor)
        {
            if (ModelState.IsValid)
            {
                // Check if floor number already exists in the building
                var existingFloors = await _floorRepository.GetAllAsync();
                if (existingFloors.Any(f => f.BuildingId == floor.BuildingId && 
                                          f.FloorNumber == floor.FloorNumber &&
                                          f.FloorId != floor.FloorId))
                {
                    ModelState.AddModelError("", "This floor number already exists in the selected building");
                }
                else
                {
                    await _floorRepository.UpdateAsync(floor);
                    TempData["SuccessMessage"] = "Floor updated successfully!";
                    return RedirectToAction("Index", new { buildingId = floor.BuildingId });
                }
            }

            ViewBag.Buildings = new SelectList(
                await _buildingRepository.GetAllAsync(), 
                "BuildingId", 
                "BuildingName",
                floor.BuildingId);

            return View(floor);
        }

        // POST: Floor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
            {
                return HttpNotFound();
            }

            // Check for existing desks
            var desks = (await _deskRepository.GetAllAsync())
                .Where(d => d.FloorId == id);

            if (desks.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete floor - there are desks associated with this floor.";
                return RedirectToAction("Index", new { buildingId = floor.BuildingId });
            }

            await _floorRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Floor deleted successfully!";
            return RedirectToAction("Index", new { buildingId = floor.BuildingId });
        }

        // GET: Floor/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
            {
                return HttpNotFound();
            }

            return View(floor);
        }
    }
}
