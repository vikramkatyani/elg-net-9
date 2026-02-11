using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ELG.Web.Middleware;
using ELG.DAL;
using ELG.DAL.DBEntity;
using ELG.DAL.DbEntityLearner;
using ELG.DAL.Utilities;
using System;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.DataProtection;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Ensure Azure environment variables are properly loaded
    // Azure App Settings use CONNECTIONSTRINGS_* (note the underscore) naming convention
    var config = builder.Configuration;
    
    // Debug: Log what we're getting from configuration
    string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
    string lmsConnStr = config["ConnectionStrings:lmsdbEntities"] ?? "NOT_FOUND";
    string lmsConnStrFromEnv = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_lmsdbEntities") ?? "NOT_IN_ENV";
    
    System.Diagnostics.Debug.WriteLine($"=== ELG.Web Startup ===");
    System.Diagnostics.Debug.WriteLine($"Environment: {env}");
    System.Diagnostics.Debug.WriteLine($"lmsdbEntities from config: {(string.IsNullOrEmpty(lmsConnStr) ? "NULL" : lmsConnStr.Substring(0, Math.Min(60, lmsConnStr.Length)) + "...")}");
    System.Diagnostics.Debug.WriteLine($"CONNECTIONSTRINGS_lmsdbEntities from env: {(string.IsNullOrEmpty(lmsConnStrFromEnv) ? "NULL" : lmsConnStrFromEnv.Substring(0, Math.Min(60, lmsConnStrFromEnv.Length)) + "...")}");

    // Fallback: if configuration doesn't have connection strings but environment variables do,
    // manually add them to configuration
    if (string.IsNullOrEmpty(lmsConnStr) && !string.IsNullOrEmpty(lmsConnStrFromEnv))
    {
        // Azure environment variables are present but not being picked up
        // This indicates a configuration provider issue
        System.Diagnostics.Debug.WriteLine("WARNING: Connection strings from environment not being read by configuration");
    }

    // Set the IConfiguration on Entity Framework DbContext classes
    // This allows them to read connection strings from appsettings.json in .NET Core
    ELG.DAL.DBEntity.lmsdbEntities._configuration = builder.Configuration;
    ELG.DAL.DbEntityLearner.learnerDBEntities._configuration = builder.Configuration;
    ELG.DAL.DBEntitySA.superadmindbEntities._configuration = builder.Configuration;

    // Allow large uploads (SCORM packages up to ~600 MB)
    const long maxUploadSize = 600L * 1024L * 1024L;
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = maxUploadSize;
    });
    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = maxUploadSize;
    });
    builder.Services.Configure<IISServerOptions>(options =>
    {
        options.MaxRequestBodySize = maxUploadSize;
    });

    // Register IHttpContextAccessor for session helper
    builder.Services.AddHttpContextAccessor();

    // Register Azure Storage utility for SCORM content and thumbnails
    var azureConnStr = builder.Configuration["AzureStorage:ConnectionString"];
    var courseContainer = builder.Configuration["AzureStorage:CourseContentContainer"] ?? "elg-learn";
    var thumbContainer = builder.Configuration["AzureStorage:ThumbnailContainer"] ?? "elg-content";
    if (!string.IsNullOrEmpty(azureConnStr))
    {
        builder.Services.AddSingleton(sp => new AzureStorageUtility(azureConnStr, courseContainer, thumbContainer));
    }

    // Configure data protection to persist keys (prevents session cookie errors on restart)
    var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("ELG.Web");

    // Register distributed cache and session services (required by SessionMiddleware)
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(20);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Add services to the container.
    // Preserve server property names (PascalCase) in JSON responses
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = null;
        })
        .AddRazorRuntimeCompilation();


    var app = builder.Build();

    // Configure for virtual application path (/manage)
    // This is required when deployed as a virtual application in IIS or Azure App Service
    var pathBase = app.Configuration["PathBase"];
    if (!string.IsNullOrEmpty(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    // Set global configuration for CommonHelper
    ELG.Web.Helper.CommonHelper.Configuration = app.Configuration;

    // Wire up SessionHelper's HttpContextAccessor
    ELG.Web.Helper.SessionHelper.HttpContextAccessor = app.Services.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Serve additional static assets from wwwroot subdirectories
    var wwwrootPath = app.Environment.WebRootPath;
    
    var wwwrootContentPath = Path.Combine(wwwrootPath, "Content");
    if (Directory.Exists(wwwrootContentPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(wwwrootContentPath),
            RequestPath = "/Content"
        });
    }

    var wwwrootScriptsPath = Path.Combine(wwwrootPath, "Scripts");
    if (Directory.Exists(wwwrootScriptsPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(wwwrootScriptsPath),
            RequestPath = "/Scripts"
        });
    }

    var wwwrootFontsPath = Path.Combine(wwwrootPath, "fonts");
    if (Directory.Exists(wwwrootFontsPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(wwwrootFontsPath),
            RequestPath = "/fonts"
        });
    }

    app.UseRouting();

    // Ensure session middleware is enabled (and runs before middleware that relies on session)
    app.UseSession();

    // Add custom middleware
    //app.UseMiddleware<OrgDomainMiddleware>();
    //app.UseMiddleware<AdminRoleMiddleware>();

    // Add global error and status code handling
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/StatusErrorCode", "?code={0}");

    app.MapControllers();

    // Add area route mapping
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    // Add default MVC route mapping
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "startup_error.txt");
    File.WriteAllText(logPath, $"Startup Error: {ex.ToString()}");
    throw;
}