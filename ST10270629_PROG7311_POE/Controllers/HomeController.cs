using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10270629_PROG7311_POE.Models;

namespace ST10270629_PROG7311_POE.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Allow anonymous access to the home page
    [AllowAnonymous]
    public IActionResult Index()
    {
        // If user is logged in, redirect them to their specific dashboard?
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Farmer"))
            {
                return RedirectToAction("MyProducts", "Farmer");
            }
            else if (User.IsInRole("Employee"))
            {
                return RedirectToAction("FarmerList", "Employee"); 
            }
        }
        // Otherwise, show the generic home page (e.g., offering login)
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous] // Allow anyone to see error page
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
