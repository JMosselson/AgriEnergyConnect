using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10270629_PROG7311_POE.Models; 

namespace ST10270629_PROG7311_POE.Data
{
    // --- Seed Data Helper Class ---
    public static class SeedData
    {
        public static async Task Initialize(AppDBContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            // Define Role Names
            string farmerRole = "Farmer";
            string employeeRole = "Employee";

            // --- 1. Create Roles ---
            if (!await roleManager.RoleExistsAsync(farmerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(farmerRole));
            }
            if (!await roleManager.RoleExistsAsync(employeeRole))
            {
                await roleManager.CreateAsync(new IdentityRole(employeeRole));
            }

            // --- 2. Seed Employee ---
            string employeeEmail = "employee@farm.com"; // Use a consistent sample email
            if (await userManager.FindByEmailAsync(employeeEmail) == null)
            {
                var employeeUser = new ApplicationUser
                {
                    UserName = employeeEmail, // Required by Identity
                    Email = employeeEmail,    // Used for login and communication
                    EmailConfirmed = true     // Auto-confirm for seeded users

                };
                var result = await userManager.CreateAsync(employeeUser, "Pass123!");
                if (result.Succeeded)
                {
                    // Assign the 'Employee' role to this user
                    await userManager.AddToRoleAsync(employeeUser, employeeRole);
                }
                // Handle potential errors during user creation if needed (e.g., logging)
            }

            // --- 3. Seed Farmers and their Products ---

            // Farmer 1 + Products
            string farmer1Email = "farmer1@farm.com";
            if (await userManager.FindByEmailAsync(farmer1Email) == null)
            {
                var farmer1User = new ApplicationUser { UserName = farmer1Email, Email = farmer1Email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(farmer1User, "Pass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(farmer1User, farmerRole);

                    if (!context.Farmers.Any(f => f.UserId == farmer1User.Id))
                    {
                        // Create the linked Farmer Profile
                        var farmer1Profile = new FarmerModel
                        {
                            UserId = farmer1User.Id, // Link to the ApplicationUser account
                            Name = "Alice Green",
                            Address = "1 Willow Lane, Farmtown",
                            ContactNumber = "555-1111"
                        };
                        context.Farmers.Add(farmer1Profile);
                        // Save changes here to ensure FarmerId is generated before adding products
                        await context.SaveChangesAsync();

                        // Seed Products for Farmer 1 (only if farmer was newly added)
                        if (!context.Products.Any(p => p.FarmerId == farmer1Profile.FarmerId))
                        {
                            context.Products.AddRange(
                                new ProductModel { Name = "Organic Apples", Category = "Fruit", ProductionDate = DateTime.Parse("2025-04-01"), FarmerId = farmer1Profile.FarmerId },
                                new ProductModel { Name = "Heirloom Carrots", Category = "Vegetable", ProductionDate = DateTime.Parse("2025-03-20"), FarmerId = farmer1Profile.FarmerId },
                                new ProductModel { Name = "Fresh Eggs", Category = "Poultry", ProductionDate = DateTime.Parse("2025-04-05"), FarmerId = farmer1Profile.FarmerId }
                            );
                            await context.SaveChangesAsync(); // Save products
                        }
                    }
                }
            }

            // Farmer 2 + Products
            string farmer2Email = "farmer2@farm.com";
            if (await userManager.FindByEmailAsync(farmer2Email) == null)
            {
                var farmer2User = new ApplicationUser { UserName = farmer2Email, Email = farmer2Email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(farmer2User, "Pass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(farmer2User, farmerRole);

                    if (!context.Farmers.Any(f => f.UserId == farmer2User.Id))
                    {
                        var farmer2Profile = new FarmerModel
                        {
                            UserId = farmer2User.Id,
                            Name = "Bob White",
                            Address = "2 Oak Street, Fieldsville",
                            ContactNumber = "555-2222"
                        };
                        context.Farmers.Add(farmer2Profile);
                        await context.SaveChangesAsync();

                        if (!context.Products.Any(p => p.FarmerId == farmer2Profile.FarmerId))
                        {
                            context.Products.AddRange(
                               new ProductModel { Name = "Whole Milk", Category = "Dairy", ProductionDate = DateTime.Parse("2025-04-06"), FarmerId = farmer2Profile.FarmerId },
                               new ProductModel { Name = "Russet Potatoes", Category = "Vegetable", ProductionDate = DateTime.Parse("2025-03-15"), FarmerId = farmer2Profile.FarmerId },
                               new ProductModel { Name = "Wheat Flour", Category = "Grain", ProductionDate = DateTime.Parse("2025-02-28"), FarmerId = farmer2Profile.FarmerId }
                            );
                            await context.SaveChangesAsync(); // Save products
                        }
                    }
                }
            }
        }
    }
}