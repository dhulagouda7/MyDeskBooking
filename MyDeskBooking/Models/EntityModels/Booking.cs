using System;
using System.ComponentModel.DataAnnotations;

namespace BookMyDesk.Models.EntityModels
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int DeskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public virtual User User { get; set; }
        public virtual Desk Desk { get; set; }
    }
}
