# Large File Upload - Implementation Quick Reference

This document provides ready-to-use code snippets for implementing the asynchronous background processing solution.

---

## Phase 1: Background Processing Setup

### 1.1 Install Hangfire NuGet Package

```powershell
# Run in Package Manager Console
Install-Package Hangfire.AspNetCore
Install-Package Hangfire.SqlServer
```

Or add to `ELG.Web.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Hangfire.AspNetCore" Version="1.8.9" />
  <PackageReference Include="Hangfire.SqlServer" Version="1.8.9" />
</ItemGroup>
```

### 1.2 Configure Hangfire in Program.cs

```csharp
// Add after line 54 (after existing service configurations)
// Location: ELG.Web/Program.cs

// Configure Hangfire for background job processing
var hangfireConnStr = builder.Configuration.GetConnectionString("lmsdbEntities");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnStr, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
        SchemaName = "HangFire"
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Process up to 5 uploads concurrently
    options.ServerName = "LMS-Upload-Worker";
});
```

```csharp
// Add after app.UseSession() in the middleware pipeline
// Location: ELG.Web/Program.cs (around line 140)

// Add Hangfire Dashboard (only for authenticated admins)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

### 1.3 Create Hangfire Authorization Filter

Create new file: `ELG.Web/Middleware/HangfireAuthorizationFilter.cs`
```csharp
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace ELG.Web.Middleware
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // Allow access only to authenticated admin users
            // Check if user is logged in and has admin role
            if (httpContext.Session.GetInt32("UserId") != null)
            {
                var userType = httpContext.Session.GetString("UserType");
                return userType == "OrgAdmin" || userType == "SuperAdmin";
            }
            
            return false;
        }
    }
}
```

### 1.4 Create Database Table for Job Tracking

Execute this SQL script on your database:
```sql
-- Create table to track upload job status
CREATE TABLE UploadJobs (
    JobId NVARCHAR(50) PRIMARY KEY,
    OrganisationId INT NOT NULL,
    UserId INT NOT NULL,
    UserEmail NVARCHAR(200),
    CourseTitle NVARCHAR(200) NOT NULL,
    CourseDescription NVARCHAR(MAX),
    Status NVARCHAR(20) NOT NULL DEFAULT 'pending', 
    -- Status values: 'pending', 'uploading', 'extracting', 'processing', 'complete', 'failed'
    Progress INT NOT NULL DEFAULT 0, -- 0-100
    Message NVARCHAR(500),
    CourseId INT NULL,
    TempBlobPath NVARCHAR(500),
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME NULL,
    
    INDEX IX_UploadJobs_OrgUser (OrganisationId, UserId, CreatedAt DESC),
    INDEX IX_UploadJobs_Status (Status, CreatedAt DESC)
);

-- Set retention policy: auto-delete completed jobs after 7 days
-- Run this as a scheduled SQL Agent job or Azure SQL elastic job
-- DELETE FROM UploadJobs 
-- WHERE Status IN ('complete', 'failed') 
-- AND CreatedAt < DATEADD(DAY, -7, GETDATE());
```

---

## Phase 2: Backend Implementation

### 2.1 Add Background Job Service

Create new file: `ELG.Web/Services/CourseUploadService.cs`
```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using ELG.DAL.Utilities;
using ELG.DAL.OrgAdminDAL;
using ELG.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NLog;

namespace ELG.Web.Services
{
    public class CourseUploadService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;

        public CourseUploadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ProcessScormPackageAsync(
            string jobId, 
            string tempBlobPath,
            string courseTitle,
            string courseDescription,
            string thumbnailBlobPath,
            int organisationId,
            string companyNumber,
            int userId)
        {
            try
            {
                // Update status: Starting processing
                UpdateJobStatus(jobId, "extracting", 10, "Downloading and extracting SCORM package...");
                
                Logger.Info($"[Job {jobId}] Starting SCORM processing for org {organisationId}");
                
                // Get Azure storage connection
                string connectionString = _configuration["AZStorageConnectionString"] 
                    ?? _configuration["AzureStorage:ConnectionString"];
                string courseContentContainer = _configuration["AzureStorage:CourseContentContainer"] ?? "elg-learn";
                string thumbnailContainer = _configuration["AzureStorage:ThumbnailContainer"] ?? "elg-content";
                
                var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
                
                // Generate unique course ID
                string uniqueCourseId = Guid.NewGuid().ToString();
                
                // Download blob to temp file
                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{jobId}.zip");
                await azureStorage.DownloadBlobToFileAsync(tempBlobPath, tempZipPath);
                
                Logger.Info($"[Job {jobId}] Downloaded blob to {tempZipPath}");
                
                // Update status: Extracting
                UpdateJobStatus(jobId, "extracting", 30, "Extracting SCORM content...");
                
                // Extract SCORM package using existing utility
                string startPath = await ExtractScormFromLocalFileAsync(
                    tempZipPath, 
                    companyNumber, 
                    uniqueCourseId,
                    azureStorage);
                
                Logger.Info($"[Job {jobId}] Extracted SCORM, start path: {startPath}");
                
                // Update status: Saving to database
                UpdateJobStatus(jobId, "processing", 70, "Saving course information...");
                
                // Build course launch path
                var trimmedStart = (startPath ?? string.Empty).TrimStart('/', '\\');
                string courseLaunchPath = string.IsNullOrEmpty(trimmedStart)
                    ? $"{companyNumber}/course/{uniqueCourseId}"
                    : $"{companyNumber}/course/{uniqueCourseId}/{trimmedStart}";
                
                // Process thumbnail if exists
                string thumbnailPath = string.Empty;
                if (!string.IsNullOrEmpty(thumbnailBlobPath))
                {
                    string extension = Path.GetExtension(thumbnailBlobPath);
                    thumbnailPath = $"thumbnails/thumbnail_{uniqueCourseId}{extension}";
                    
                    // Thumbnail is already uploaded to blob storage during initial upload
                    // Just record the path
                }
                
                // Save to database
                var courseRep = new ModuleRep();
                int courseId = courseRep.CreateScormCourse(
                    organisationId,
                    courseTitle,
                    courseDescription ?? "",
                    thumbnailPath,
                    courseLaunchPath,
                    uniqueCourseId);
                
                if (courseId > 0)
                {
                    Logger.Info($"[Job {jobId}] Course created with ID {courseId}");
                    
                    // Update status: Complete
                    UpdateJobStatus(jobId, "complete", 100, "Course uploaded successfully!", courseId);
                    
                    // Cleanup temp files
                    if (File.Exists(tempZipPath))
                        File.Delete(tempZipPath);
                    
                    // Delete temp blob
                    await azureStorage.DeleteBlobAsync(tempBlobPath);
                    if (!string.IsNullOrEmpty(thumbnailBlobPath))
                        await azureStorage.DeleteBlobAsync(thumbnailBlobPath);
                }
                else
                {
                    throw new Exception("Failed to save course to database");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[Job {jobId}] Processing failed: {ex.Message}");
                UpdateJobStatus(jobId, "failed", 0, "Upload failed", errorMessage: ex.Message);
            }
        }

        private async Task<string> ExtractScormFromLocalFileAsync(
            string zipFilePath, 
            string companyNumber, 
            string courseId,
            AzureStorageUtility azureStorage)
        {
            // This mirrors the existing ExtractScormPackageAsync logic
            // but works with a file path instead of IFormFile
            
            string tempExtractPath = Path.Combine(Path.GetTempPath(), $"scorm_{courseId}_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempExtractPath);

            try
            {
                // Extract ZIP file to temp location
                using (var fileStream = File.OpenRead(zipFilePath))
                using (var archive = new System.IO.Compression.ZipArchive(fileStream, System.IO.Compression.ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(tempExtractPath);
                }

                // Find SCORM start path (existing logic from AzureStorageUtility)
                string startPath = azureStorage.FindScormStartPath(tempExtractPath);

                // Upload extracted contents to Azure
                await azureStorage.UploadDirectoryToAzureAsync(tempExtractPath, companyNumber, courseId);

                return startPath;
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
            }
        }

        private void UpdateJobStatus(
            string jobId, 
            string status, 
            int progress, 
            string message = null, 
            int? courseId = null,
            string errorMessage = null)
        {
            try
            {
                using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
                {
                    // Use direct SQL to update job status
                    // (Assuming you've added UploadJobs to your DbContext)
                    
                    var sql = @"
                        UPDATE UploadJobs 
                        SET Status = @Status, 
                            Progress = @Progress, 
                            Message = @Message,
                            CourseId = @CourseId,
                            ErrorMessage = @ErrorMessage,
                            UpdatedAt = GETDATE(),
                            CompletedAt = CASE WHEN @Status IN ('complete', 'failed') THEN GETDATE() ELSE NULL END
                        WHERE JobId = @JobId";
                    
                    context.Database.ExecuteSqlCommand(sql,
                        new System.Data.SqlClient.SqlParameter("@JobId", jobId),
                        new System.Data.SqlClient.SqlParameter("@Status", status),
                        new System.Data.SqlClient.SqlParameter("@Progress", progress),
                        new System.Data.SqlClient.SqlParameter("@Message", (object)message ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@CourseId", (object)courseId ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@ErrorMessage", (object)errorMessage ?? DBNull.Value));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to update job status for {jobId}");
            }
        }
    }
}
```

### 2.2 Modify CourseManagementController - Upload Endpoint

Replace the existing `UploadScormPackage` POST method:
```csharp
// Location: ELG.Web/Controllers/CourseManagementController.cs
// Replace lines 1133-1270

[HttpPost]
public async Task<ActionResult> UploadScormPackage([FromForm] ScormPackageUploadViewModel model)
{
    ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
    
    try
    {
        long fileSize = model?.ScormPackage?.Length ?? 0;
        Logger.Info($"UploadScormPackage: Received file upload - {fileSize} bytes");
        
        // Validate quota before proceeding
        var moduleRep = new ModuleRep();
        int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
        int? maxAllowedCoursesNullable = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId);
        int maxAllowedCourses = maxAllowedCoursesNullable ?? 0;
        
        if (maxAllowedCourses > 0 && currentCourseCount >= maxAllowedCourses)
        {
            response.Err = 1;
            response.Message = $"Course quota exhausted. You have reached the maximum limit of {maxAllowedCourses} courses.";
            return Json(response);
        }

        // Validate model
        if (string.IsNullOrEmpty(model.CourseTitle) || 
            model.ScormPackage == null || model.ScormPackage.Length == 0)
        {
            response.Err = 1;
            response.Message = "Course title and SCORM package are required.";
            return Json(response);
        }

        // Validate file types
        var allowedZipMimeTypes = new[] { "application/zip", "application/x-zip-compressed", "application/octet-stream" };
        if (!allowedZipMimeTypes.Contains(model.ScormPackage.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            response.Err = 1;
            response.Message = "Only ZIP files are allowed for SCORM packages.";
            return Json(response);
        }

        // Validate thumbnail if provided
        if (model.Thumbnail != null && model.Thumbnail.Length > 0)
        {
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedImageTypes.Contains(model.Thumbnail.ContentType))
            {
                response.Err = 1;
                response.Message = "Only image files (JPEG, PNG, GIF, WebP) are allowed for thumbnails.";
                return Json(response);
            }
        }

        // Get company info
        string companyNumber = SessionHelper.CompanyNumber;
        if (string.IsNullOrEmpty(companyNumber))
        {
            response.Err = 1;
            response.Message = "Company information not found.";
            return Json(response);
        }

        // Generate job ID
        string jobId = Guid.NewGuid().ToString();
        
        // Get Azure storage configuration
        string connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString")
            ?? CommonHelper.GetAppSettingValue("AzureStorage:ConnectionString");
        string courseContentContainer = CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
        string thumbnailContainer = CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";
        
        var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
        
        // Upload SCORM package to temp blob storage (quick, streaming upload)
        string tempBlobPath = $"temp-uploads/{companyNumber}/{jobId}/{model.ScormPackage.FileName}";
        using (var stream = model.ScormPackage.OpenReadStream())
        {
            await azureStorage.UploadBlobAsync(tempBlobPath, stream, model.ScormPackage.ContentType);
        }
        
        Logger.Info($"[Job {jobId}] Uploaded SCORM to temp blob: {tempBlobPath}");
        
        // Upload thumbnail to temp blob if provided
        string tempThumbnailPath = null;
        if (model.Thumbnail != null && model.Thumbnail.Length > 0)
        {
            tempThumbnailPath = $"temp-uploads/{companyNumber}/{jobId}/thumbnail_{model.Thumbnail.FileName}";
            using (var stream = model.Thumbnail.OpenReadStream())
            {
                await azureStorage.UploadBlobAsync(tempThumbnailPath, stream, model.Thumbnail.ContentType);
            }
            Logger.Info($"[Job {jobId}] Uploaded thumbnail to temp blob: {tempThumbnailPath}");
        }
        
        // Create job record in database
        CreateUploadJob(jobId, model.CourseTitle, model.CourseDescription, tempBlobPath, 
            (int)SessionHelper.CompanyId, (int)SessionHelper.UserId, SessionHelper.EmailId);
        
        // Queue background job using Hangfire
        BackgroundJob.Enqueue<CourseUploadService>(service => 
            service.ProcessScormPackageAsync(
                jobId,
                tempBlobPath,
                model.CourseTitle,
                model.CourseDescription,
                tempThumbnailPath,
                (int)SessionHelper.CompanyId,
                companyNumber,
                (int)SessionHelper.UserId));
        
        Logger.Info($"[Job {jobId}] Queued background processing job");
        
        // Return immediately with job ID and status URL
        response.Err = 0;
        response.Message = "Upload started. Processing in background...";
        response.Data = new {
            jobId = jobId,
            statusUrl = Url.Action("GetUploadStatus", "CourseManagement", new { jobId = jobId })
        };
        
        return Json(response);
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "UploadScormPackage error: " + ex.Message);
        response.Err = 1;
        response.Message = "An error occurred during upload. Please try again.";
        return Json(response);
    }
}

private void CreateUploadJob(string jobId, string courseTitle, string courseDescription, 
    string tempBlobPath, int organisationId, int userId, string userEmail)
{
    try
    {
        using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
        {
            var sql = @"
                INSERT INTO UploadJobs 
                (JobId, OrganisationId, UserId, UserEmail, CourseTitle, CourseDescription, 
                 Status, Progress, Message, TempBlobPath, CreatedAt, UpdatedAt)
                VALUES 
                (@JobId, @OrgId, @UserId, @Email, @Title, @Description, 
                 'pending', 0, 'Upload queued...', @BlobPath, GETDATE(), GETDATE())";
            
            context.Database.ExecuteSqlCommand(sql,
                new System.Data.SqlClient.SqlParameter("@JobId", jobId),
                new System.Data.SqlClient.SqlParameter("@OrgId", organisationId),
                new System.Data.SqlClient.SqlParameter("@UserId", userId),
                new System.Data.SqlClient.SqlParameter("@Email", userEmail ?? ""),
                new System.Data.SqlClient.SqlParameter("@Title", courseTitle),
                new System.Data.SqlClient.SqlParameter("@Description", courseDescription ?? ""),
                new System.Data.SqlClient.SqlParameter("@BlobPath", tempBlobPath));
        }
    }
    catch (Exception ex)
    {
        Logger.Error(ex, $"Failed to create upload job record: {jobId}");
    }
}
```

### 2.3 Add Status Endpoint

Add this new method to `CourseManagementController`:
```csharp
[HttpGet]
public ActionResult GetUploadStatus(string jobId)
{
    try
    {
        if (string.IsNullOrEmpty(jobId))
        {
            return Json(new { success = 0, message = "Job ID is required" });
        }
        
        // Get job status from database
        using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
        {
            var sql = @"
                SELECT Status, Progress, Message, CourseId, ErrorMessage, CreatedAt, CompletedAt
                FROM UploadJobs 
                WHERE JobId = @JobId 
                AND OrganisationId = @OrgId";
            
            var job = context.Database.SqlQuery<UploadJobStatusDto>(sql,
                new System.Data.SqlClient.SqlParameter("@JobId", jobId),
                new System.Data.SqlClient.SqlParameter("@OrgId", SessionHelper.CompanyId)).FirstOrDefault();
            
            if (job == null)
            {
                return Json(new { success = 0, message = "Job not found" });
            }
            
            return Json(new { 
                success = 1,
                status = job.Status,
                progress = job.Progress,
                message = job.Message,
                courseId = job.CourseId,
                errorMessage = job.ErrorMessage,
                isComplete = job.Status == "complete" || job.Status == "failed",
                redirectUrl = job.Status == "complete" ? Url.Action("Courses", "CourseManagement") : null
            });
        }
    }
    catch (Exception ex)
    {
        Logger.Error(ex, $"GetUploadStatus error for job {jobId}");
        return Json(new { success = 0, message = "Error checking status" });
    }
}

// DTO for status query
public class UploadJobStatusDto
{
    public string Status { get; set; }
    public int Progress { get; set; }
    public string Message { get; set; }
    public int? CourseId { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

---

## Phase 3: Frontend Implementation

### 3.1 Update Upload View

Modify `ELG.Web/Views/CourseManagement/UploadScormPackage.cshtml`:

Add progress indicator HTML (after the form):
```html
<!-- Add after the form closing tag -->
<div id="uploadProgressContainer" style="display: none; margin-top: 20px;">
    <div class="card">
        <div class="card-header">
            <h4>Upload Progress</h4>
        </div>
        <div class="card-body">
            <div id="uploadProgressMessage" class="alert alert-info">
                Uploading file...
            </div>
            <div class="progress" style="height: 30px;">
                <div id="uploadProgressBar" class="progress-bar progress-bar-striped progress-bar-animated" 
                     role="progressbar" style="width: 0%;" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">
                    0%
                </div>
            </div>
            <small class="form-text text-muted mt-2">
                Large files may take several minutes to process. You can leave this page and check back later.
            </small>
        </div>
    </div>
</div>
```

Update the JavaScript (replace existing form submit handler):
```javascript
<script>
$(document).ready(function() {
    $('#uploadScormForm').on('submit', function(e) {
        e.preventDefault();
        
        // Validate form
        if (!$('#CourseTitle').val() || !$('#ScormPackage')[0].files[0]) {
            showErrorAlert('Please provide a course title and select a SCORM package file.');
            return;
        }
        
        // Show progress container
        $('#uploadProgressContainer').show();
        $('#uploadProgressMessage').removeClass('alert-danger').addClass('alert-info');
        $('#uploadProgressMessage').text('Uploading file to server...');
        updateProgressBar(10);
        
        // Disable form inputs
        $('#uploadScormForm :input').prop('disabled', true);
        
        // Create FormData
        var formData = new FormData(this);
        
        // Submit via AJAX
        $.ajax({
            url: '@Url.Action("UploadScormPackage", "CourseManagement")',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function(response) {
                if (response.Err === 0 && response.Data && response.Data.jobId) {
                    // Upload initiated successfully, start polling
                    $('#uploadProgressMessage').text('Upload complete. Processing SCORM package...');
                    updateProgressBar(20);
                    pollUploadStatus(response.Data.jobId);
                }
                else {
                    showUploadError(response.Message || 'Upload failed');
                    $('#uploadScormForm :input').prop('disabled', false);
                }
            },
            error: function(xhr, status, error) {
                showUploadError('Network error: ' + error);
                $('#uploadScormForm :input').prop('disabled', false);
            }
        });
    });
    
    function pollUploadStatus(jobId) {
        var pollInterval = setInterval(function() {
            $.ajax({
                url: '@Url.Action("GetUploadStatus", "CourseManagement")',
                type: 'GET',
                data: { jobId: jobId },
                dataType: 'json',
                success: function(response) {
                    if (response.success === 1) {
                        updateProgressBar(response.progress);
                        $('#uploadProgressMessage').text(response.message || 'Processing...');
                        
                        if (response.isComplete) {
                            clearInterval(pollInterval);
                            
                            if (response.status === 'complete') {
                                // Success!
                                $('#uploadProgressMessage')
                                    .removeClass('alert-info')
                                    .addClass('alert-success')
                                    .text('Course uploaded successfully! Redirecting...');
                                
                                // Redirect to courses page after 2 seconds
                                setTimeout(function() {
                                    window.location.href = response.redirectUrl || '@Url.Action("Courses", "CourseManagement")';
                                }, 2000);
                            }
                            else if (response.status === 'failed') {
                                // Failed
                                showUploadError(response.errorMessage || 'Upload processing failed');
                                $('#uploadScormForm :input').prop('disabled', false);
                            }
                        }
                    }
                    else {
                        clearInterval(pollInterval);
                        showUploadError(response.message || 'Status check failed');
                        $('#uploadScormForm :input').prop('disabled', false);
                    }
                },
                error: function() {
                    // Don't stop polling on network errors, just log
                    console.warn('Status poll failed, will retry...');
                }
            });
        }, 5000); // Poll every 5 seconds
    }
    
    function updateProgressBar(percent) {
        $('#uploadProgressBar')
            .css('width', percent + '%')
            .attr('aria-valuenow', percent)
            .text(percent + '%');
    }
    
    function showUploadError(message) {
        $('#uploadProgressMessage')
            .removeClass('alert-info')
            .addClass('alert-danger')
            .text('Error: ' + message);
        updateProgressBar(0);
    }
    
    function showErrorAlert(message) {
        alert(message); // Or use a nicer notification library
    }
});
</script>
```

---

## Phase 4: Testing & Deployment

### 4.1 Local Testing Checklist

```powershell
# 1. Verify Hangfire is running
# Navigate to: http://localhost:5000/hangfire
# You should see the Hangfire dashboard

# 2. Check database table exists
$connStr = "Server=localhost;Database=new_elg_data;Integrated Security=True;"
Invoke-Sqlcmd -ConnectionString $connStr -Query "SELECT COUNT(*) FROM UploadJobs"

# 3. Test small file upload (100MB)
# Login as admin → Go to Upload SCORM Package → Upload small file
# Watch Hangfire dashboard for job execution

# 4. Test large file upload (1GB)
# Upload large SCORM package
# Verify progress bar updates
# Check job completes successfully
```

### 4.2 Azure Deployment

```powershell
# 1. Build and publish
dotnet publish ELG.Web/ELG.Web.csproj -c Release -o ./publish

# 2. Deploy to Azure
az webapp deployment source config-zip `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --src ./publish.zip

# 3. Verify Hangfire tables were created
# Check Azure SQL Database for HangFire.* tables

# 4. Add Hangfire dashboard URL to monitoring
# https://app.elearningate.com/hangfire
```

### 4.3 Monitoring & Troubleshooting

```sql
-- Check active upload jobs
SELECT JobId, CourseTitle, Status, Progress, Message, CreatedAt, UpdatedAt
FROM UploadJobs
WHERE Status NOT IN ('complete', 'failed')
ORDER BY CreatedAt DESC;

-- Check failed jobs in last 24 hours
SELECT JobId, CourseTitle, Status, ErrorMessage, CreatedAt
FROM UploadJobs
WHERE Status = 'failed'
AND CreatedAt > DATEADD(HOUR, -24, GETDATE())
ORDER BY CreatedAt DESC;

-- Get upload statistics
SELECT 
    Status,
    COUNT(*) as Count,
    AVG(DATEDIFF(SECOND, CreatedAt, COALESCE(CompletedAt, GETDATE()))) as AvgDurationSeconds
FROM UploadJobs
WHERE CreatedAt > DATEADD(DAY, -7, GETDATE())
GROUP BY Status;
```

---

## Troubleshooting Common Issues

### Issue: Hangfire jobs not processing
**Solution**:
```csharp
// Check Hangfire worker count in Program.cs
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Increase if needed
});

// Check Hangfire dashboard: /hangfire
// Look for "Failed Jobs" tab
```

### Issue: Database permission errors
**Solution**:
```sql
-- Grant Hangfire schema permissions
GRANT CREATE TABLE TO [your_app_user];
GRANT CREATE SCHEMA TO [your_app_user];

-- Or create schema manually
CREATE SCHEMA [HangFire];
```

### Issue: Temp blobs not deleting
**Solution**:
```csharp
// Add cleanup job in Program.cs
RecurringJob.AddOrUpdate(
    "cleanup-temp-blobs",
    () => CleanupTempBlobs(),
    Cron.Daily(3)); // Run at 3 AM daily

public void CleanupTempBlobs()
{
    // Delete blobs older than 24 hours in temp-uploads folder
    var azureStorage = new AzureStorageUtility(...);
    azureStorage.DeleteOldTempBlobsAsync(TimeSpan.FromHours(24));
}
```

---

## Performance Tuning

### Optimize for faster processing:

```csharp
// In CourseUploadService.cs
// Use parallel upload for SCORM files
private async Task UploadScormFilesInParallelAsync(string extractPath, string blobPrefix)
{
    var files = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
    var uploadTasks = files.Select(file => UploadSingleFileAsync(file, blobPrefix));
    
    // Process 10 files at a time
    await Task.WhenAll(uploadTasks.Batch(10).Select(batch => Task.WhenAll(batch)));
}
```

### Monitor memory usage:

```csharp
// Add memory logging
var memoryBefore = GC.GetTotalMemory(false);
await ProcessScormPackageAsync(...);
var memoryAfter = GC.GetTotalMemory(false);
Logger.Info($"Memory used: {(memoryAfter - memoryBefore) / 1024 / 1024}MB");
```

---

**Document Version**: 1.0  
**Last Updated**: March 6, 2026  
**Status**: Ready for Implementation
