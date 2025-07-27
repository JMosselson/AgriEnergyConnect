using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ST10270629_PROG7311_POE.Models
{
    public class ProductModel
    {
        [Key] // Primary Key
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., "Fruit", "Vegetable", "Dairy"

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Production Date")]
        public DateTime ProductionDate { get; set; }

        // --- Relationship to Farmer ---
        [Required]
        public int FarmerId { get; set; } // Foreign Key

        [ForeignKey("FarmerId")]
        public virtual FarmerModel? Farmer { get; set; } // Navigation property
    }
}
