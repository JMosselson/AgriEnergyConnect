// File: Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using ST10270629_PROG7311_POE.Models; 

namespace ST10270629_PROG7311_POE.Models 
{
    // Ensure it inherits from IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // Navigation property for the one-to-one relationship with FarmerModel
        public virtual FarmerModel? FarmerProfile { get; set; }
                
    }
}