using System;
using System.ComponentModel.DataAnnotations;

namespace MyDeskBooking.Models.EntityModels
{
    public class Desk
    {
        [Key]
        public int DeskId { get; set; }

        [Required]
        public int FloorId { get; set; }

        [Required]
        [StringLength(20)]
        public string DeskNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public bool IsReserved { get; set; }

        public virtual Floor Floor { get; set; }
    }
}
