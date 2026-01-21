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

try
{
    var builder = WebApplication.CreateBuilder(args);

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