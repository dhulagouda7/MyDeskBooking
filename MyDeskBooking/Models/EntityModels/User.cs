using System;
using System.ComponentModel.DataAnnotations;

namespace MyDeskBooking.Models.EntityModels
{    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Team")]
        public int TeamId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }
    }
}
