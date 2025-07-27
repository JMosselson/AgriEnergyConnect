using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10270629_PROG7311_POE.Data;  
using ST10270629_PROG7311_POE.Models; 

namespace ST10270629_PROG7311_POE 
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add Database Context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(connectionString)); // Or UseSqlite, UseNpgsql etc.

            // 2. Add Identity Services
            // Use AddDefaultIdentity for common setup, including cookie configuration and default UI support
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
                // Configure Identity options if needed (e.g., password requirements)
                options.SignIn.RequireConfirmedAccount = false;                                                                 
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 0;
            })
                .AddRoles<IdentityRole>() // *** Add Role support ***
                .AddEntityFrameworkStores<AppDBContext>();

            // Configure Application Cookie settings (optional customizations)
            // AddDefaultIdentity handles basic cookie setup, but you can customize further
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login"; // Default path
                options.LogoutPath = "/Identity/Account/Logout"; // Default path
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Default path OR your custom "/Home/AccessDenied"                                                                            
            });

            // 3. Add MVC Controllers and Views
            builder.Services.AddControllersWithViews();
            // Add Razor Pages support is needed for Default Identity UI
            builder.Services.AddRazorPages();

            var app = builder.Build();
                        
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint(); 
                app.UseDeveloperExceptionPage(); 
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // User-friendly error page                
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); 

            app.UseRouting();
            
            app.UseAuthentication(); 
            app.UseAuthorization();  

           
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Map Razor Pages (needed for Identity UI)
            app.MapRazorPages();
                        
            
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Get required services for seeding
                    var context = services.GetRequiredService<AppDBContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();                                      

                    // Call the seeding logic
                    await SeedData.Initialize(context, userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                    // Decide if you want the app to stop or continue if seeding fails
                }
            }            

            app.Run(); // Start the application
        }
    }
    
}