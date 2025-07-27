using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10270629_PROG7311_POE.Data;
using ST10270629_PROG7311_POE.Models;
using System;
using System.Linq; 

namespace ST10270629_PROG7311_POE.Controllers
{
    [Authorize(Roles = "Farmer")] // Only logged-in Farmers can access
    public class FarmerController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FarmerController(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper method to get the current farmer's profile
        private async Task<FarmerModel?> GetCurrentFarmerProfileAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return null; // Should not happen if Authorize works, but handle defensively
            }
            return await _context.Farmers
                                 .FirstOrDefaultAsync(f => f.UserId == currentUser.Id);
        }

        // Helper method to check if a product belongs to the current farmer (used internally)
        private async Task<bool> IsProductOwnedByCurrentFarmerAsync(int productId)
        {
            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null) return false;

            // Check if the product exists and its FarmerId matches the current farmer's ID
            return await _context.Products
                                 .AnyAsync(p => p.ProductId == productId && p.FarmerId == farmerProfile.FarmerId);
        }


        // GET: /Farmer/MyProducts
        public async Task<IActionResult> MyProducts()
        {
            var farmerProfile = await GetCurrentFarmerProfileAsync();

            if (farmerProfile == null)
            {
                ViewBag.ErrorMessage = "Farmer profile not found. Please contact support.";
                return View(new List<ProductModel>()); // Return empty list
            }

            // Fetch products specifically linked to this farmer
            var products = await _context.Products
                                         .Where(p => p.FarmerId == farmerProfile.FarmerId)
                                         .OrderByDescending(p => p.ProductionDate)
                                         .ToListAsync();

            return View(products); // Pass list of products to view
        }

        // GET: /Farmer/AddProduct
        public IActionResult AddProduct()
        {
            ViewBag.Categories = new List<string> { "Fruit", "Vegetable", "Dairy", "Poultry", "Grain", "Other" };
            return View();
        }

        // POST: /Farmer/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct([Bind("Name,Category,ProductionDate")] ProductModel product) // Bind only expected properties
        {
            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null)
            {
                ModelState.AddModelError("", "Farmer profile not found.");
                ViewBag.Categories = new List<string> { "Fruit", "Vegetable", "Dairy", "Poultry", "Grain", "Other" };
                return View(product);
            }

            // Assign the FarmerId before validation
            product.FarmerId = farmerProfile.FarmerId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product added successfully!"; // Feedback message
                    return RedirectToAction(nameof(MyProducts));
                }
                catch (DbUpdateException /* ex */)
                {
                    // Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists " +
                        "see your system administrator.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Categories = new List<string> { "Fruit", "Vegetable", "Dairy", "Poultry", "Grain", "Other" };
            return View(product);
        }

        // --- Edit Product Features ---

        // GET: /Farmer/EditProduct/5
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // *** Security Check: Ensure the product belongs to the current farmer ***
            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null || product.FarmerId != farmerProfile.FarmerId)
            {
                // Log attempt to access unauthorized product if needed
                return Forbid(); // Returns a 403 Forbidden response
            }
            // *** End Security Check ***


            ViewBag.Categories = new List<string> { "Fruit", "Vegetable", "Dairy", "Poultry", "Grain", "Other" };
            return View(product);
        }

        // POST: /Farmer/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, [Bind("ProductId,Name,Category,ProductionDate,FarmerId")] ProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound(); // The ID in the route doesn't match the ID in the form data
            }

            // *** Security Check: Fetch the original product to verify ownership before update ***
            // Use AsNoTracking() because we are attaching the 'product' model later
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
            {
                // This could happen if the product was deleted between the GET and POST requests
                TempData["ErrorMessage"] = "The product you were trying to edit was not found.";
                return RedirectToAction(nameof(MyProducts));
            }

            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null || existingProduct.FarmerId != farmerProfile.FarmerId)
            {
                // Log attempt to edit unauthorized product
                return Forbid(); // Returns a 403 Forbidden response
            }
            // *** End Security Check ***

            // Ensure the FarmerId is correctly set from the verified profile, preventing tampering
            product.FarmerId = farmerProfile.FarmerId;


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product); // Attach and mark as Modified
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(MyProducts));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If the original product was deleted or changed concurrently by another process
                    if (!await IsProductOwnedByCurrentFarmerAsync(product.ProductId))
                    {
                        // Product no longer exists or no longer belongs to the farmer
                        TempData["ErrorMessage"] = "The product you were trying to edit was deleted or changed by someone else.";
                        return RedirectToAction(nameof(MyProducts)); // Or show a specific conflict view
                    }
                    else
                    {
                        // A genuine concurrency issue (less likely in a single-farmer view)
                        ModelState.AddModelError("", "The product you were trying to edit was modified by another user after you got the original values. The edit operation was canceled.");
                    }
                }
                catch (DbUpdateException /* ex */)
                {
                    // Log other database errors
                    ModelState.AddModelError("", "Unable to save changes. " +
                       "Try again, and if the problem persists " +
                       "see your system administrator.");
                }
            }

            // If we reach here, something failed, redisplay the form with errors
            ViewBag.Categories = new List<string> { "Fruit", "Vegetable", "Dairy", "Poultry", "Grain", "Other" };
            return View(product);
        }

        // --- Delete Product Features ---

        // GET: /Farmer/DeleteProduct/5
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Farmer) // Include Farmer if you want to display farmer name on delete page
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                // Product not found
                return NotFound();
            }

            // *** Security Check: Ensure the product belongs to the current farmer ***
            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null || product.FarmerId != farmerProfile.FarmerId)
            {
                // Log attempt to access unauthorized product if needed
                return Forbid(); // Returns a 403 Forbidden response
            }
            // *** End Security Check ***

            return View(product); // Return the product to the Delete confirmation view
        }

        // POST: /Farmer/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteProduct")] // Maps this POST method to the /Farmer/DeleteProduct/{id} URL
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // *** Security Check: Find the product and verify ownership before deleting ***
            // FindAsync is suitable here as we just need the entity to remove it.
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                // Product already deleted or wasn't found initially
                TempData["ErrorMessage"] = "Product not found or already deleted.";
                return RedirectToAction(nameof(MyProducts));
            }

            var farmerProfile = await GetCurrentFarmerProfileAsync();
            if (farmerProfile == null || product.FarmerId != farmerProfile.FarmerId)
            {
                // Log attempt to delete unauthorized product
                return Forbid(); // Returns a 403 Forbidden response
            }
            // *** End Security Check ***

            try
            {
                _context.Products.Remove(product); // Mark entity for deletion
                await _context.SaveChangesAsync(); // Execute deletion
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            catch (DbUpdateException /* ex */)
            {
                // Log the error
                TempData["ErrorMessage"] = "Unable to delete product. Try again, and if the problem persists see your system administrator.";
            }

            return RedirectToAction(nameof(MyProducts)); // Redirect back to the product list
        }
    }
}