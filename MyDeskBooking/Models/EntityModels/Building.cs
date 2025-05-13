using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BookMyDesk.Models.EntityModels
{
    public class Building
    {
        public int BuildingId { get; set; }
        public int LocationId { get; set; }
        public string BuildingName { get; set; }
        public virtual Location Location { get; set; }
        public virtual ICollection<Floor> Floors { get; set; } // Added this property to fix the error  
    }
}
