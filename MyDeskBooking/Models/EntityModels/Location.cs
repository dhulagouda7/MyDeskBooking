using System;
using System.ComponentModel.DataAnnotations;

namespace MyDeskBooking.Models.EntityModels
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100)]
        public string LocationName { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }
    }
}
