using System.Collections.Generic;
using MyDeskBooking.Models.EntityModels;

namespace MyDeskBooking.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int LocationCount { get; set; }
        public int BuildingCount { get; set; }
        public int FloorCount { get; set; }
        public int DeskCount { get; set; }
        public int UserCount { get; set; }
        public int ActiveBookingsCount { get; set; }
    }

    public class LocationManagementViewModel
    {
        public Location Location { get; set; }
        public IEnumerable<Location> Locations { get; set; }
    }

    public class BuildingManagementViewModel
    {
        public Building Building { get; set; }
        public IEnumerable<Building> Buildings { get; set; }
        public IEnumerable<Location> AvailableLocations { get; set; }
    }

    public class FloorManagementViewModel
    {
        public Floor Floor { get; set; }
        public IEnumerable<Floor> Floors { get; set; }
        public IEnumerable<Building> AvailableBuildings { get; set; }
    }

    public class DeskManagementViewModel
    {
        public Desk Desk { get; set; }
        public IEnumerable<Desk> Desks { get; set; }
        public IEnumerable<Floor> AvailableFloors { get; set; }
    }
}
