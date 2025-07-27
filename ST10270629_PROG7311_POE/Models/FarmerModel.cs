using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10270629_PROG7311_POE.Models
{
    public class FarmerModel
    {
        [Key] // Primary Key
        public int FarmerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; } // Optional address

        [StringLength(50)]
        public string? ContactNumber { get; set; } // Optional contact number

        // --- Relationship to Application User ---
        [Required]
        public string UserId { get; set; } = string.Empty; // Foreign Key to ApplicationUser (IdentityUser)

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; } // Navigation property

        // --- Relationship to Products ---
        public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>(); // One-to-many
    }
}
