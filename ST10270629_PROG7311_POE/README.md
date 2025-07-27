# AgriEnergy Connect Prototype

## Running the App
1. **Clone** the repository to your local machine.  
2. **Configure** your SQL Server connection in `appsettings.json` under `ConnectionStrings:DefaultConnection`.  
4. **Migrate** the database: 
   Update-Database (Updates your own database on SSMS to connect to the current Migrations Folder)
5. **Run** the application:  

---

## Features
- **Role-Based Access**  
  - **Employee**: Add and list farmers; assign farmer roles; view and filter any farmer’s products.  
  - **Farmer**: Add, edit, delete, and list their own products.  
	
- **Secure Authentication**  
  Built on ASP.NET Identity with seeded “Employee” and “Farmer” users. 

- **Real-Time Data Management**  
  Products and farmer profiles are stored in SQL Server Magemenet Studio

- **User Feedback & Validation**  
  Forms include server-side validation, TempData messages, and ownership checks.  
- **Responsive UI**  
  Bootstrap-powered layout with role-aware navigation and feature highlights.

---

## Code Structure

1. **Program.cs**  
   - Entry point configures services (EF Core, Identity, MVC), seeds roles/users, and sets up the middleware pipeline.

2. **AppDBContext.cs**  
   - Inherits `IdentityDbContext<ApplicationUser>`  
   - `DbSet<FarmerModel>` and `DbSet<ProductModel>`  
   - Seeds initial farmers and products.

3. **Models**  
   - `ApplicationUser` extends `IdentityUser` (add custom properties like `FullName`).  
   - `FarmerModel` (`FarmerId`, `Name`, `Address`, `ContactNumber`, `UserId`, `ApplicationUser`) links to `ApplicationUser`.  
   - `ProductModel` (`ProductId`, `Name`, `Category`, `ProductionDate`, `FarmerId`) with navigation.

4. **Controllers**  
   - **HomeController**: Public landing page, redirects authenticated users to their dashboard.  
   - **EmployeeController** (`[Authorize(Roles="Employee")]`): Manage farmers, assign roles, and view/filter products.  
   - **FarmerController** (`[Authorize(Roles="Farmer")]`): CRUD operations on the logged-in farmer’s products.

5. **Views**  
   - Razor views under `/Views/Home`, `/Views/Employee`, `/Views/Farmer`, and shared layouts.  
   - Includes role-aware navigation in `_Layout.cshtml` and interactive forms for each feature.

6. **Identity UI**  
   - Standard ASP.NET Core Identity pages for **Login**, **Register**, and **Logout** under `Areas/Identity`.

---

## User Roles
- **Employee**  
  - Email: `employee@farm.com`  
  - Password: `Pass123!`  
- **Farmer1**  
  - Email: `farmer1@farm.com`  
  - Password: `Pass123!`  
- **Farmer2**  
  - Email: `farmer2@farm.com`  
  - Password: `Pass123!`

Or any Custom User

---

> AgriEnergy Connect demonstrates how to build a secure, scalable, and user-friendly enterprise web app that bridges agriculture and renewable energy. 
By following this guide, you can explore and extend its functionality to fit your own sustainable-farming scenarios.

For the development of this prototype, a local Microsoft SQL Server database was used to design, integrate, and simulate the platform's data storage requirements. 
This decision aligns with the project brief, which required a relational database to manage farmer and product data populated with realistic sample records. 
While the final product would be expected to operate in a cloud or online environment to support multi-user access and real-time data interaction, the prototype's purpose is to demonstrate core functionality in a controlled, development-friendly setting. 
The local setup allows for easier testing, debugging, and iteration during development, and can be readily migrated to an online database such as Azure SQL in future stages of deployment.

ChatGPT Reference Link for basic scaffoling css, and READMEFile: https://chatgpt.com/share/6822e611-b4e0-800e-ae1c-8b1a018d34de

Gemini For Comments (DIDNT USE MOST COMMENTS): https://g.co/gemini/share/8e76bc9bec4f

ChatGPT for the image: https://chatgpt.com/s/m_6822e3e8f7dc8191bdceec1b1c2f4862
