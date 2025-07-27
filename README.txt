
# **AgriEnergy Connect Prototype**

## 🚀 Running the App

1. **Clone** the repository to your local machine:

   ```bash
   git clone <your-repo-url>
   ```
2. **Configure** your SQL Server connection in `appsettings.json` under:

   ```
   ConnectionStrings:DefaultConnection
   ```
3. **Apply Database Migrations**:

   ```powershell
   Update-Database
   ```

   *(This updates your database in SQL Server Management Studio using the current Migrations folder.)*
4. **Run** the application in Visual Studio:

   * Press **F5** or click **Run**.

---

## ✅ Features

* **Role-Based Access**

  * **Employee**:

    * Add and list farmers
    * Assign farmer roles
    * View and filter any farmer’s products
  * **Farmer**:

    * Add, edit, delete, and list their own products

* **Secure Authentication**
  Built on **ASP.NET Identity** with seeded “Employee” and “Farmer” roles.

* **Real-Time Data Management**
  Products and farmer profiles are stored in **SQL Server Management Studio**.

* **User Feedback & Validation**
  Includes server-side validation, TempData messages, and ownership checks.

* **Responsive UI**
  Bootstrap-powered layout with role-aware navigation and feature highlights.

---

## 📂 Code Structure

### **1. Program.cs**

* Configures services (EF Core, Identity, MVC)
* Seeds roles/users
* Sets up middleware pipeline

### **2. AppDBContext.cs**

* Inherits `IdentityDbContext<ApplicationUser>`
* Includes `DbSet<FarmerModel>` and `DbSet<ProductModel>`
* Seeds initial farmers and products

### **3. Models**

* **ApplicationUser** → Extends `IdentityUser` (adds custom properties like `FullName`)
* **FarmerModel** → Links to `ApplicationUser` with properties:
  `FarmerId`, `Name`, `Address`, `ContactNumber`, `UserId`
* **ProductModel** → Contains:
  `ProductId`, `Name`, `Category`, `ProductionDate`, `FarmerId`

### **4. Controllers**

* **HomeController** → Public landing page; redirects authenticated users to their dashboard
* **EmployeeController** → `[Authorize(Roles="Employee")]`; manages farmers, assigns roles, and views products
* **FarmerController** → `[Authorize(Roles="Farmer")]`; CRUD operations on farmer products

### **5. Views**

* Razor views under:
  `/Views/Home`, `/Views/Employee`, `/Views/Farmer`
* Shared layout in `_Layout.cshtml`
* Role-aware navigation and interactive forms

### **6. Identity UI**

* Standard ASP.NET Core Identity pages for:
  **Login**, **Register**, **Logout** (under `Areas/Identity`)

---

## 👤 User Roles & Credentials

* **Employee**

  * Email: `employee@farm.com`
  * Password: `Pass123!`

* **Farmer1**

  * Email: `farmer1@farm.com`
  * Password: `Pass123!`

* **Farmer2**

  * Email: `farmer2@farm.com`
  * Password: `Pass123!`

Or register a custom user.

---

## ℹ️ About the Project

**AgriEnergy Connect** demonstrates a secure, scalable, and user-friendly enterprise web app that bridges **agriculture** and **renewable energy**.

For this prototype:

* A **local Microsoft SQL Server database** was used for development and testing.
* This approach ensures easier debugging and iteration during the development phase.
* In a real-world scenario, this would migrate to **Azure SQL** or a similar cloud database for multi-user access and real-time interaction.

---

## 🔗 References & Credits

* **ChatGPT** for scaffolding CSS and README: [View Reference](https://chatgpt.com/share/6822e611-b4e0-800e-ae1c-8b1a018d34de)
* **Gemini** for some comments: [View Reference](https://g.co/gemini/share/8e76bc9bec4f)
* **ChatGPT** for image generation: [View Reference](https://chatgpt.com/s/m_6822e3e8f7dc8191bdceec1b1c2f4862)

---

