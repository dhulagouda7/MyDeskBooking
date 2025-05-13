using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Models.ViewModels;
using BookMyDesk.Services.DataAccess;
using System.Collections;

namespace BookMyDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Building> _buildingRepository;
        private readonly IRepository<Floor> _floorRepository;
        private readonly IRepository<Desk> _deskRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IBookingRepository _bookingRepository;

        public AdminController(
            IRepository<Location> locationRepository,
            IRepository<Building> buildingRepository,
            IRepository<Floor> floorRepository,
            IRepository<Desk> deskRepository,
            IRepository<User> userRepository,
            IBookingRepository bookingRepository)
        {
            _locationRepository = locationRepository;
            _buildingRepository = buildingRepository;
            _floorRepository = floorRepository;
            _deskRepository = deskRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<ActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                LocationCount = (await _locationRepository.GetAllAsync()).Count(),
                BuildingCount = (await _buildingRepository.GetAllAsync()).Count(),
                FloorCount = (await _floorRepository.GetAllAsync()).Count(),
                DeskCount = (await _deskRepository.GetAllAsync()).Count(),
                UserCount = (await _userRepository.GetAllAsync()).Count(),
                ActiveBookingsCount = (await _bookingRepository.GetAllAsync())
                    .Count(b => b.Status == "Pending" || b.Status == "CheckedIn")
            };
            return View(model);
        }

        #region Building Management
        public async Task<ActionResult> Buildings()
        {
            try
            {
                // Get buildings with eager loading of required relationships
                var buildings = (await _buildingRepository.GetAllAsync())
                    .Select(b => new {
                        Building = b,
                        Floors = (b.Floors ?? Enumerable.Empty<Floor>()).ToList(),
                        DesksCount = b.Floors?.Sum(f => f.Desks?.Count ?? 0) ?? 0
                    })
                    .ToList();

                var model = new BuildingManagementViewModel
                {
                    Buildings = buildings.Select(b => b.Building),
                    AvailableLocations = await _locationRepository.GetAllAsync(),
                    Building = new Building()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error loading buildings: {ex.Message}");
                return View(new BuildingManagementViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBuilding(Building building)
        {
            if (ModelState.IsValid)
            {
                await _buildingRepository.AddAsync(building);
                TempData["SuccessMessage"] = "Building added successfully!";
                return RedirectToAction("Buildings");
            }

            var model = new BuildingManagementViewModel
            {
                Buildings = await _buildingRepository.GetAllAsync(),
                AvailableLocations = await _locationRepository.GetAllAsync(),
                Building = building
            };
            return View("Buildings", model);
        }

        public async Task<ActionResult> GetBuilding(int id)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(id);
                if (building == null)
                {
                    return Json(new { error = "Building not found" }, JsonRequestBehavior.AllowGet);
                }
                
                return Json(new {
                    buildingId = building.BuildingId,
                    locationId = building.LocationId,
                    buildingName = building.BuildingName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error retrieving building: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBuilding(Building building)
        {
            if (ModelState.IsValid)
            {
                await _buildingRepository.UpdateAsync(building);
                TempData["SuccessMessage"] = "Building updated successfully!";
                return RedirectToAction("Buildings");
            }

            var model = new BuildingManagementViewModel
            {
                Buildings = await _buildingRepository.GetAllAsync(),
                AvailableLocations = await _locationRepository.GetAllAsync(),
                Building = building
            };
            return View("Buildings", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBuilding(int buildingId)
        {
            try
            {
                // Check for existing floors and bookings
                var building = await _buildingRepository.GetByIdAsync(buildingId);
                if (building == null)
                {
                    ModelState.AddModelError("", "Building not found");
                    return RedirectToAction("Buildings");
                }

                var floors = (await _floorRepository.GetAllAsync()).Where(f => f.BuildingId == buildingId);
                var desks = (await _deskRepository.GetAllAsync()).Where(d => floors.Any(f => f.FloorId == d.FloorId));
                var activeBookings = (await _bookingRepository.GetAllAsync())
                    .Where(b => desks.Any(d => d.DeskId == b.DeskId) &&
                               (b.Status == "Pending" || b.Status == "CheckedIn"));

                if (activeBookings.Any())
                {
                    ModelState.AddModelError("", "Cannot delete building - there are active bookings for desks in this building");
                    return RedirectToAction("Buildings");
                }

                await _buildingRepository.DeleteAsync(buildingId);
                TempData["SuccessMessage"] = "Building deleted successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting building: {ex.Message}");
            }
            return RedirectToAction("Buildings");
        }
        #endregion

        #region Floor Management
        public async Task<ActionResult> Floors(int? buildingId = null)
        {
            try
            {
                // Get floors with eager loading of related data
                var floors = (await _floorRepository.GetAllAsync())
                    .Where(f => !buildingId.HasValue || f.BuildingId == buildingId)
                    .Select(f => new {
                        Floor = f,
                        DesksCount = f.Desks?.Count ?? 0,
                        AvailableDesksCount = f.Desks?.Count(d => d.Status == "Available" && !d.IsReserved) ?? 0
                    })
                    .ToList();

                var model = new FloorManagementViewModel
                {
                    Floors = floors.Select(f => f.Floor),
                    AvailableBuildings = await _buildingRepository.GetAllAsync(),
                    Floor = new Floor { BuildingId = buildingId ?? 0 }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error loading floors: {ex.Message}");
                return View(new FloorManagementViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddFloor(Floor floor)
        {
            if (ModelState.IsValid)
            {
                // Check if floor number already exists in the building
                var existingFloors = await _floorRepository.GetAllAsync();
                if (existingFloors.Any(f => f.BuildingId == floor.BuildingId && f.FloorNumber == floor.FloorNumber))
                {
                    ModelState.AddModelError("", "This floor number already exists in the selected building");
                }
                else
                {
                    await _floorRepository.AddAsync(floor);
                    TempData["SuccessMessage"] = "Floor added successfully!";
                    return RedirectToAction("Floors", new { buildingId = floor.BuildingId });
                }
            }

            var model = new FloorManagementViewModel
            {
                Floors = await _floorRepository.GetAllAsync(),
                AvailableBuildings = await _buildingRepository.GetAllAsync(),
                Floor = floor
            };
            return View("Floors", model);
        }

        public async Task<ActionResult> GetFloor(int id)
        {
            try
            {
                var floor = await _floorRepository.GetByIdAsync(id);
                if (floor == null)
                {
                    return Json(new { error = "Floor not found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new {
                    floorId = floor.FloorId,
                    buildingId = floor.BuildingId,
                    floorNumber = floor.FloorNumber
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error retrieving floor: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFloor(Floor floor)
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
                    return RedirectToAction("Floors", new { buildingId = floor.BuildingId });
                }
            }

            var model = new FloorManagementViewModel
            {
                Floors = await _floorRepository.GetAllAsync(),
                AvailableBuildings = await _buildingRepository.GetAllAsync(),
                Floor = floor
            };
            return View("Floors", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteFloor(int floorId)
        {
            try
            {
                var floor = await _floorRepository.GetByIdAsync(floorId);
                if (floor == null)
                {
                    ModelState.AddModelError("", "Floor not found");
                    return RedirectToAction("Floors");
                }

                // Check for desks with active bookings
                var desks = (await _deskRepository.GetAllAsync()).Where(d => d.FloorId == floorId);
                var activeBookings = (await _bookingRepository.GetAllAsync())
                    .Where(b => desks.Any(d => d.DeskId == b.DeskId) &&
                               (b.Status == "Pending" || b.Status == "CheckedIn"));

                if (activeBookings.Any())
                {
                    ModelState.AddModelError("", "Cannot delete floor - there are active bookings for desks on this floor");
                    return RedirectToAction("Floors", new { buildingId = floor.BuildingId });
                }

                await _floorRepository.DeleteAsync(floorId);
                TempData["SuccessMessage"] = "Floor deleted successfully!";
                return RedirectToAction("Floors", new { buildingId = floor.BuildingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting floor: {ex.Message}");
                return RedirectToAction("Floors");
            }
        }
        #endregion

        #region Desk Management
        public async Task<ActionResult> Desks(int? floorId = null)
        {
            try
            {
                // Get desks with eager loading and filtering
                var desks = (await _deskRepository.GetAllAsync())
                    .Where(d => !floorId.HasValue || d.FloorId == floorId);

                // Get active bookings for status display
                var activeBookings = (await _bookingRepository.GetAllAsync())
                    .Where(b => b.Status == "Pending" || b.Status == "CheckedIn")
                    .ToDictionary(b => b.DeskId);

                var model = new DeskManagementViewModel
                {
                    Desks = desks,
                    AvailableFloors = await _floorRepository.GetAllAsync(),
                    Desk = new Desk { FloorId = floorId ?? 0 }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error loading desks: {ex.Message}");
                return View(new DeskManagementViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDesk(Desk desk)
        {
            if (ModelState.IsValid)
            {
                // Check if desk number already exists on the floor
                var existingDesks = await _deskRepository.GetAllAsync();
                if (existingDesks.Any(d => d.FloorId == desk.FloorId && d.DeskNumber == desk.DeskNumber))
                {
                    ModelState.AddModelError("", "This desk number already exists on the selected floor");
                }
                else
                {
                    await _deskRepository.AddAsync(desk);
                    TempData["SuccessMessage"] = "Desk added successfully!";
                    return RedirectToAction("Desks", new { floorId = desk.FloorId });
                }
            }

            var model = new DeskManagementViewModel
            {
                Desks = await _deskRepository.GetAllAsync(),
                AvailableFloors = await _floorRepository.GetAllAsync(),
                Desk = desk
            };
            return View("Desks", model);
        }

        public async Task<ActionResult> GetDesk(int id)
        {
            try
            {
                var desk = await _deskRepository.GetByIdAsync(id);
                if (desk == null)
                {
                    return Json(new { error = "Desk not found" }, JsonRequestBehavior.AllowGet);
                }
                
                return Json(new {
                    deskId = desk.DeskId,
                    floorId = desk.FloorId,
                    deskNumber = desk.DeskNumber,
                    status = desk.Status,
                    isReserved = desk.IsReserved
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error retrieving desk: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDesk(Desk desk)
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
                    return RedirectToAction("Desks", new { floorId = desk.FloorId });
                }
            }

            var model = new DeskManagementViewModel
            {
                Desks = await _deskRepository.GetAllAsync(),
                AvailableFloors = await _floorRepository.GetAllAsync(),
                Desk = desk
            };
            return View("Desks", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeDeskStatus(int deskId, string status)
        {
            var desk = await _deskRepository.GetByIdAsync(deskId);
            desk.Status = status;
            await _deskRepository.UpdateAsync(desk);
            TempData["SuccessMessage"] = "Desk status updated successfully!";
            return RedirectToAction("Desks", new { floorId = desk.FloorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDesk(int deskId)
        {
            try
            {
                var desk = await _deskRepository.GetByIdAsync(deskId);
                if (desk == null)
                {
                    ModelState.AddModelError("", "Desk not found");
                    return RedirectToAction("Desks");
                }

                // Check for active bookings
                var activeBookings = (await _bookingRepository.GetAllAsync())
                    .Where(b => b.DeskId == deskId && 
                               (b.Status == "Pending" || b.Status == "CheckedIn"));

                if (activeBookings.Any())
                {
                    ModelState.AddModelError("", "Cannot delete desk - there are active bookings for this desk");
                    return RedirectToAction("Desks", new { floorId = desk.FloorId });
                }

                await _deskRepository.DeleteAsync(deskId);
                TempData["SuccessMessage"] = "Desk deleted successfully!";
                return RedirectToAction("Desks", new { floorId = desk.FloorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting desk: {ex.Message}");
                return RedirectToAction("Desks");
            }
        }
        #endregion
    }
}