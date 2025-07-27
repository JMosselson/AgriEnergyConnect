using System.ComponentModel.DataAnnotations;

namespace ST10270629_PROG7311_POE.ViewModels
{
    public class AssignFarmerProfileViewModel
    {
        // Hidden field to store the User's ID
        public string UserId { get; set; } = string.Empty;

        // Display field
        [Display(Name = "User Email")]
        public string Email { get; set; } = string.Empty; // To show who we are assigning

        // Fields for FarmerModel
        [Required]
        [StringLength(100)]
        [Display(Name = "Farmer's Full Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Number")]
        [Phone]
        public string? ContactNumber { get; set; }
    }
}