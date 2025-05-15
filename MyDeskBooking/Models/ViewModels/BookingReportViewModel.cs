using System;
using System.Collections.Generic;

namespace MyDeskBooking.Models.ViewModels
{
    public class BookingReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public bool IsMonthlyReport { get; set; }
        public int TotalBookings { get; set; }
        public List<LocationStats> LocationStats { get; set; } = new List<LocationStats>();
    }

    public class LocationStats
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int TotalDesks { get; set; }
        public int BookedDesks { get; set; }
        public decimal UtilizationRate => TotalDesks > 0 ? (decimal)BookedDesks / TotalDesks * 100 : 0;
        public List<BuildingStats> BuildingStats { get; set; } = new List<BuildingStats>();
    }

    public class BuildingStats
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public int TotalDesks { get; set; }
        public int BookedDesks { get; set; }
        public decimal UtilizationRate => TotalDesks > 0 ? (decimal)BookedDesks / TotalDesks * 100 : 0;
        public List<FloorStats> FloorStats { get; set; } = new List<FloorStats>();
    }

    public class FloorStats
    {
        public int FloorId { get; set; }
        public int FloorNumber { get; set; }
        public int TotalDesks { get; set; }
        public int BookedDesks { get; set; }
        public decimal UtilizationRate => TotalDesks > 0 ? (decimal)BookedDesks / TotalDesks * 100 : 0;
    }
}
