/// <summary>
/// Entity Framework DbContext Dependency Injection Guide
/// 
/// This file documents how to properly inject and use DbContext instances
/// in controllers and services for both local development and Azure deployment.
/// </summary>

/*
SUMMARY OF CHANGES FOR RUNTIME CONNECTION STRING INJECTION
===========================================================

1. CREATED FILES:
   - LMS_DAL/DbContextFactory.cs
     Static factory class with methods to create DbContext instances
   
   - LMS_DAL/DBEntity/AdminDBEntity.Partial.cs
     Partial class adding constructor for lmsdbEntities context
   
   - LMS_DAL/DbEntityLearner/LearnerDbEntity.Partial.cs
     Partial class adding constructor for learnerDBEntities context
   
   - LMS_DAL/DBEntitySA/SuperAdminDbEntity.Partial.cs
     Partial class adding constructor for superadmindbEntities context

2. UPDATED FILES:
   - LMS_admin/Program.cs
     Registered DbContext with dependency injection
   
   - LMS_learner/Program.cs
     Registered DbContext with dependency injection

3. HOW IT WORKS:
   
   The auto-generated Entity Framework DbContext classes (lmsdbEntities, 
   learnerDBEntities, superadmindbEntities) are extended with partial classes
   that add constructors accepting a connection string parameter.
   
   In Program.cs, we register these contexts with the DI container, reading
   the connection string from appsettings.json (which can be different for
   Development, Production, etc.).
   
   When a controller or service requests the DbContext via constructor 
   injection, the DI container automatically instantiates it with the 
   configured connection string.


USAGE IN CONTROLLERS
====================

OLD WAY (DO NOT USE):
---------------------
public class YourController : Controller
{
    private lmsdbEntities db = new lmsdbEntities();  // ❌ Bad
    
    public ActionResult Index()
    {
        var data = db.tbContacts.ToList();
        return View(data);
    }
}

Problem: 
- Uses hardcoded "name=lmsdbEntities" from App.Config
- Always connects to localhost with integrated security
- Does not work on Azure
- Not testable


NEW WAY (RECOMMENDED):
----------------------
public class YourController : Controller
{
    private readonly lmsdbEntities _db;
    
    // Constructor injection - DI container provides the context
    public YourController(lmsdbEntities db)
    {
        _db = db;
    }
    
    public ActionResult Index()
    {
        var data = _db.tbContacts.ToList();
        return View(data);
    }
}

Benefits:
- Uses connection string from appsettings.json
- Works locally and on Azure
- Testable (can inject mock DbContext for unit tests)
- Respects environment-specific configuration


HOW TO MIGRATE EXISTING CONTROLLERS
===================================

For each controller that creates DbContext instances:

1. Find all lines like:
   private lmsdbEntities db = new lmsdbEntities();

2. Replace with constructor injection:
   
   Before:
   -------
   public class AdminController : Controller
   {
       private lmsdbEntities db = new lmsdbEntities();
       
       public ActionResult Index()
       {
           var data = db.GetData();
       }
   }
   
   After:
   ------
   public class AdminController : Controller
   {
       private readonly lmsdbEntities _db;
       
       public AdminController(lmsdbEntities db)
       {
           _db = db;
       }
       
       public ActionResult Index()
       {
           var data = _db.GetData();
       }
   }

3. Update all references from 'db' to '_db'

4. Test locally and on Azure


CONFIGURATION FILES
===================

appsettings.Development.json (Local Development):
--------------------------------------------------
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Initial Catalog=new_elg_data;Integrated Security=true;TrustServerCertificate=true;"
  }
}

appsettings.Production.json (Azure):
-------------------------------------
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=ELG-Prod-DB;User ID=your-username;Password=your-password;..."
  }
}


HOW THE DI CONTAINER WORKS
===========================

In Program.cs:

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<lmsdbEntities>(provider => 
    DbContextFactory.CreateAdminContext(connectionString)
);

This tells the DI container:
1. When someone requests an lmsdbEntities instance (constructor parameter)
2. Call DbContextFactory.CreateAdminContext(connectionString)
3. Pass the connection string from appsettings.json
4. Return a fully configured DbContext instance
5. Dispose it automatically when the request ends (Scoped)


LIFETIME MANAGEMENT
===================

AddScoped<T> - Creates one instance per HTTP request
  - Used for DbContext (recommended)
  - Instances are disposed at end of request
  
AddSingleton<T> - Creates one instance for entire application
  - Not recommended for DbContext
  - Thread safety concerns
  
AddTransient<T> - Creates new instance every time
  - Not recommended for DbContext
  - Performance overhead


TESTING WITH DEPENDENCY INJECTION
==================================

Unit tests can now mock the DbContext:

[TestClass]
public class AdminControllerTests
{
    [TestMethod]
    public void Index_ReturnsData()
    {
        // Create a mock DbContext
        var mockDb = new Mock<lmsdbEntities>();
        mockDb.Setup(x => x.tbContacts).Returns(
            new List<tbContact> { new tbContact { id = 1, name = "Test" } }
        );
        
        // Inject the mock into the controller
        var controller = new AdminController(mockDb.Object);
        
        // Test the action
        var result = controller.Index();
        
        // Assert
        Assert.IsNotNull(result);
    }
}


MIGRATION CHECKLIST
===================

To migrate all controllers to use DI:

□ Created factory methods (DbContextFactory.cs)
□ Created partial classes with new constructors
□ Updated Program.cs in LMS_admin
□ Updated Program.cs in LMS_learner
□ Added using statements to Program.cs
□ Updated all controller constructors to accept DbContext
□ Removed all 'new lmsdbEntities()' instantiations
□ Changed all 'db.' references to '_db.'
□ Tested locally with connection string
□ Tested on Azure with Azure SQL connection string


TROUBLESHOOTING
===============

Issue: "The connection string is not configured"
Solution: Check appsettings.json has "ConnectionStrings": { "DefaultConnection": "..." }

Issue: "Cannot inject lmsdbEntities"
Solution: Verify DbContextFactory and partial classes were created correctly

Issue: "Connection denied from Azure"
Solution: Check Azure SQL firewall allows Azure App Service

Issue: "Integrated security not supported"
Solution: This is expected on Azure; use SQL Server credentials instead


FILES REFERENCE
===============

DbContextFactory.cs
  - Factory methods for creating contexts
  - Validates connection strings
  - Handles creation logic

AdminDBEntity.Partial.cs
  - Extends lmsdbEntities class
  - Adds connection string constructor
  - Does NOT modify auto-generated code

LearnerDbEntity.Partial.cs
  - Extends learnerDBEntities class
  - Adds connection string constructor
  - Does NOT modify auto-generated code

SuperAdminDbEntity.Partial.cs
  - Extends superadmindbEntities class
  - Adds connection string constructor
  - Does NOT modify auto-generated code

Program.cs (LMS_admin & LMS_learner)
  - Registers DbContext with DI container
  - Reads connection string from configuration
  - Validates configuration at startup


AZURE DEPLOYMENT
================

When deploying to Azure:

1. appsettings.Production.json contains Azure SQL connection string
2. App Service configuration overrides appsettings values (if needed)
3. DbContext automatically uses Azure SQL server
4. No hardcoded localhost connections
5. No integrated security issues
6. Can scale horizontally (multiple instances)


NEXT STEPS
==========

1. Update all controllers to use constructor injection
2. Test locally with Development connection string
3. Test on Azure with Production connection string
4. Remove all hardcoded 'new lmsdbEntities()' calls
5. Use DbContextFactory only if creating contexts outside DI (rare)


QUESTIONS?
==========

The key concept: DbContext should be injected, not instantiated directly.
This allows configuration management and testing.
*/
