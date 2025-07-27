using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST10270629_PROG7311_POE.Models; 

namespace ST10270629_PROG7311_POE.Data
{
    // Inherit from IdentityDbContext<ApplicationUser> to include Identity tables
    public class AppDBContext : IdentityDbContext<ApplicationUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        // Add DbSets for your custom models
        public DbSet<FarmerModel> Farmers { get; set; }
        public DbSet<ProductModel> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // IMPORTANT: Call base method first for Identity configuration
                        
            builder.Entity<FarmerModel>()
                .HasOne(f => f.User)          // FarmerModel has one User
                .WithOne(u => u.FarmerProfile) // ApplicationUser has one FarmerProfile
                .HasForeignKey<FarmerModel>(f => f.UserId); // Foreign key is UserId in FarmerModel
                        
            builder.Entity<FarmerModel>()
                .HasIndex(f => f.UserId)
                .IsUnique();

            // Configure the relationship between FarmerModel and ProductModel (One-to-Many)
            builder.Entity<FarmerModel>()
                .HasMany(f => f.Products)       // FarmerModel has many Products
                .WithOne(p => p.Farmer)         // ProductModel has one Farmer
                .HasForeignKey(p => p.FarmerId) // Foreign key is FarmerId in ProductModel
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}