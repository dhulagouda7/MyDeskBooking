using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookMyDesk.Models.EntityModels
{
    public class Floor
    {
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public int FloorNumber { get; set; }
        public virtual Building Building { get; set; }
        public virtual ICollection<Desk> Desks { get; set; } // Added property to fix CS1061  
    }
}
