using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Services.DataAccess;

namespace BookMyDesk.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IRepository<Floor> _floorRepository;

        public LocationController(
            IRepository<Location> locationRepository,
            IRepository<Building> buildingRepository,
            IRepository<Floor> floorRepository)
        {
            _locationRepository = locationRepository;
            _buildingRepository = buildingRepository;
            _floorRepository = floorRepository;
        }

        public async Task<ActionResult> Index()
        {
            var locations = await _locationRepository.GetAllAsync();
            return View(locations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Location location)
        {
            if (ModelState.IsValid)
            {
                await _locationRepository.AddAsync(location);
                TempData["SuccessMessage"] = "Location added successfully!";
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                await _locationRepository.UpdateAsync(location);
                TempData["SuccessMessage"] = "Location updated successfully!";
                return RedirectToAction("Index");
            }
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return HttpNotFound();
            }

            // Check for existing buildings
            var buildings = (await _buildingRepository.GetAllAsync())
                .Where(b => b.LocationId == id);

            if (buildings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete location - there are buildings associated with this location.";
                return RedirectToAction("Index");
            }

            await _locationRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Location deleted successfully!";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Buildings(int locationId)
        {
            var buildings = (await _buildingRepository.GetAllAsync())
                .Where(b => b.LocationId == locationId)
                .OrderBy(b => b.BuildingName);
            var location = await _locationRepository.GetByIdAsync(locationId);
            
            ViewBag.LocationName = location.LocationName;
            ViewBag.LocationId = locationId;
            return View(buildings);
        }

        public async Task<ActionResult> Floors(int buildingId)
        {
            var floors = (await _floorRepository.GetAllAsync())
                .Where(f => f.BuildingId == buildingId)
                .OrderBy(f => f.FloorNumber);
            var building = await _buildingRepository.GetByIdAsync(buildingId);
            
            ViewBag.BuildingName = building.BuildingName;
            ViewBag.BuildingId = buildingId;
            ViewBag.LocationId = building.LocationId;
            return View(floors);
        }
    }
}
