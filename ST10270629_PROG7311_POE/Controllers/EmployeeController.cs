using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10270629_PROG7311_POE.Data;
using ST10270629_PROG7311_POE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System;
using ST10270629_PROG7311_POE.ViewModels;
using System.Linq; 
using System.Collections.Generic; 

namespace ST10270629_PROG7311_POE.Controllers
{
    [Authorize(Roles = "Employee")] // Only logged-in Employees
    public class EmployeeController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public EmployeeController(AppDBContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager; 
        }

        // GET: /Employee/FarmerList
        public async Task<IActionResult> FarmerList()
        {
            var farmers = await _context.Farmers.Include(f => f.User).OrderBy(f => f.Name).ToListAsync();
            return View(farmers);
        }          

        // GET: /Employee/FindUserToMakeFarmer
        // Shows users who are NOT already Employees or Farmers and don't have a Farmer profile yet
        public async Task<IActionResult> FindUserToMakeFarmer()
        {
            // Get IDs of users already in Employee or Farmer roles
            var employeeUsers = await _userManager.GetUsersInRoleAsync("Employee");
            var farmerUsers = await _userManager.GetUsersInRoleAsync("Farmer");
            var existingRoleUserIds = employeeUsers.Select(u => u.Id)
                                                 .Union(farmerUsers.Select(u => u.Id))
                                                 .ToList();

            // Get IDs of users already linked to a FarmerModel profile
            var linkedUserIds = await _context.Farmers
                                              .Select(f => f.UserId)
                                              .Distinct()
                                              .ToListAsync();

            // Combine the exclusion lists
            var excludedUserIds = existingRoleUserIds.Union(linkedUserIds).Distinct().ToList();

            // Find users who are NOT in the excluded lists
            var potentialFarmers = await _userManager.Users
                                                     .Where(u => !excludedUserIds.Contains(u.Id))
                                                     .OrderBy(u => u.Email)
                                                     .ToListAsync();

            return View(potentialFarmers); // Pass this list to the new View
        }

        // GET: /Employee/AssignFarmerProfile/{userId}
        // Shows the form to add Farmer profile details for a selected user
        public async Task<IActionResult> AssignFarmerProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to find user with ID '{userId}'.");
            }

            // Double-check if user somehow already became a farmer since the last view
            var isFarmerRole = await _userManager.IsInRoleAsync(user, "Farmer");
            var hasProfile = await _context.Farmers.AnyAsync(f => f.UserId == userId);

            if (isFarmerRole || hasProfile)
            {
                TempData["ErrorMessage"] = $"User '{user.Email}' is already designated as a Farmer.";
                return RedirectToAction(nameof(FindUserToMakeFarmer));
            }

            var viewModel = new AssignFarmerProfileViewModel
            {
                UserId = user.Id,
                Email = user.Email // Pre-fill email for display
            };

            return View(viewModel); // Pass VM to the new View
        }

        // POST: /Employee/AssignFarmerProfile
        // Processes the form submission to assign role and create profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFarmerProfile(AssignFarmerProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    // This shouldn't happen if the form submitted correctly, but check anyway
                    ModelState.AddModelError("", $"Unable to find user.");
                    return View(model); // Return view with error
                }

                // Check again before making changes
                var isFarmerRole = await _userManager.IsInRoleAsync(user, "Farmer");
                var hasProfile = await _context.Farmers.AnyAsync(f => f.UserId == model.UserId);

                if (isFarmerRole || hasProfile)
                {
                    ModelState.AddModelError("", $"User '{user.Email}' is already designated as a Farmer.");
                    model.Email = user.Email; // Re-populate email for display
                    return View(model);
                }

                // 1. Assign the "Farmer" role
                var roleResult = await _userManager.AddToRoleAsync(user, "Farmer");
                if (!roleResult.Succeeded)
                {
                    // Add role assignment errors to ModelState
                    foreach (var error in roleResult.Errors) { ModelState.AddModelError("", error.Description); }
                    model.Email = user.Email; // Re-populate email
                    return View(model);
                }

                // 2. Create the FarmerModel profile
                var farmerProfile = new FarmerModel
                {
                    UserId = model.UserId, // Link to the user account
                    Name = model.Name,
                    Address = model.Address,
                    ContactNumber = model.ContactNumber
                };
                _context.Farmers.Add(farmerProfile);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User '{user.Email}' successfully designated as Farmer '{model.Name}'.";
                    return RedirectToAction(nameof(FarmerList)); // Redirect to the list of confirmed farmers
                }
                catch (DbUpdateException ex)
                {
                    // If saving FarmerModel fails, attempt to roll back role assignment (best effort)
                    await _userManager.RemoveFromRoleAsync(user, "Farmer");
                    // Log the inner exception: ex.InnerException?.Message
                    ModelState.AddModelError("", "Failed to save farmer profile after assigning role. Role assignment rolled back. Error: " + ex.InnerException?.Message);
                    model.Email = user.Email; // Re-populate email
                    return View(model);
                }
            }

            // If ModelState is invalid, return the view with the model to show validation errors
            if (string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.UserId))
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null) model.Email = user.Email;
            }
            return View(model);
        }

        // GET: /Employee/ViewFarmerProducts/{farmerId} (Stays the same)
        public async Task<IActionResult> ViewFarmerProducts(
            int farmerId,
            string? productType,
            DateTime? startDate,
            DateTime? endDate)
        {
            
            var farmer = await _context.Farmers
                                       .Include(f => f.User) // Include user details if needed
                                       .FirstOrDefaultAsync(f => f.FarmerId == farmerId);

            if (farmer == null)
            {
                return NotFound(); // Farmer not found
            }

            // Base query for the specific farmer's products
            var productsQuery = _context.Products
                                       .Where(p => p.FarmerId == farmerId)
                                       .AsQueryable(); // Start as IQueryable to build filters

            // --- Apply Filters ---
            if (!string.IsNullOrEmpty(productType))
            {
                productsQuery = productsQuery.Where(p => p.Category == productType);
            }
            if (startDate.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductionDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Add 1 day and check for less than, to include the whole end date
                productsQuery = productsQuery.Where(p => p.ProductionDate < endDate.Value.AddDays(1));
            }

            // Execute the query and order results
            var filteredProducts = await productsQuery.OrderByDescending(p => p.ProductionDate).ToListAsync();

            // --- Prepare data for the view ---
            ViewBag.FarmerName = farmer.Name;
            ViewBag.FarmerId = farmer.FarmerId; // Pass FarmerId back for filter form action

            // Pass filter values back to the view to keep them selected
            ViewBag.CurrentProductType = productType;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd"); // Format for date input
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            // Get distinct product types for the dropdown filter
            ViewBag.ProductTypes = await _context.Products
                                                .Where(p => p.FarmerId == farmerId) // Only types this farmer has
                                                .Select(p => p.Category)
                                                .Distinct()
                                                .OrderBy(c => c)
                                                .ToListAsync();

            return View(filteredProducts); // Pass the filtered list to the view
        }
    }
}