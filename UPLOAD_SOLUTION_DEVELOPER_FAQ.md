# Large File Upload Solution - Developer FAQ

## General Questions

### Q: Why are we getting 502 errors for large file uploads?

**A**: The 502 (Bad Gateway) error occurs because Azure Front Door (or Application Gateway) times out before the server finishes processing. Here's what happens:

1. User uploads 1GB SCORM package
2. Server receives the file (3 minutes)
3. Server starts extracting ZIP and uploading to Azure Blob Storage (5+ minutes)
4. **Azure Front Door times out after 2-4 minutes** → Returns 502 to user
5. Server may still be processing, but user thinks it failed

The timeout is at the infrastructure level, not in our application code. Even though `web.config` has 10-minute timeout, the reverse proxy times out first.

---

### Q: Why can't we just increase the Front Door timeout?

**A**: Azure Front Door has a **maximum timeout of 4 minutes**. There's no way to make it wait longer. Some reasons:

- It's a shared service with global limits
- Long-held connections impact scalability
- CDN/proxy best practices limit connection duration
- Even at 4 minutes, 1GB uploads take 8-10 minutes total

This is why we need asynchronous processing - the HTTP request must complete within 4 minutes.

---

### Q: What is Hangfire and why are we using it?

**A**: Hangfire is an open-source .NET library for background job processing. It:

- **Queues jobs** to be processed asynchronously
- **Persists job state** in SQL database (survives server restarts)
- **Automatically retries** failed jobs
- **Provides dashboard** for monitoring jobs
- **Handles concurrency** - multiple jobs can run in parallel

We chose Hangfire because:
- ✅ Easy to integrate (3-4 hours setup)
- ✅ No external services required (uses our SQL database)
- ✅ Well-documented and widely used
- ✅ Free and open-source
- ✅ Battle-tested (10+ years in production use)

**Alternatives considered**:
- Azure Functions - More complex, requires separate deployment, costs more
- Azure Service Bus + Worker - Overkill for our use case
- Manual threading - No persistence, no monitoring, error-prone

---

### Q: How does the job persistence work?

**A**: Hangfire creates several tables in your SQL database (in the `HangFire` schema):

```
HangFire.Job         - Job definitions and parameters
HangFire.State       - Job state history (pending, processing, succeeded, failed)
HangFire.JobQueue    - Queue of pending jobs
HangFire.Server      - Active worker servers
HangFire.Hash        - Key-value storage for job data
HangFire.Counter     - Performance counters
```

When you queue a job:
1. Hangfire serializes the method call and parameters
2. Inserts a record into `HangFire.Job` table
3. Adds entry to `HangFire.JobQueue`
4. Worker picks it up and processes
5. Updates state in `HangFire.State` on success/failure

If server restarts:
- Worker checks `HangFire.JobQueue` on startup
- Resumes any in-progress jobs
- No data loss, automatic recovery

---

### Q: What happens if two admins upload at the same time?

**A**: Both uploads work fine! Hangfire processes jobs concurrently:

- Default: 5 concurrent workers (configurable)
- Each job is isolated with its own temp files
- Azure Blob Storage handles concurrent writes
- Database transactions ensure no conflicts

Example:
```
T+0:00  User A uploads 1GB course → Job 1 queued → Worker 1 starts
T+0:30  User B uploads 500MB course → Job 2 queued → Worker 2 starts
T+1:00  User C uploads 2GB course → Job 3 queued → Worker 3 starts
T+3:00  Job 2 completes (500MB, fastest)
T+5:00  Job 1 completes (1GB)
T+8:00  Job 3 completes (2GB, largest)
```

All three succeed without interfering with each other.

---

## Implementation Questions

### Q: Where does the code go?

**A**: Main changes are in 3 areas:

1. **Program.cs** (startup configuration)
   ```csharp
   // Add Hangfire services
   builder.Services.AddHangfire(...);
   builder.Services.AddHangfireServer(...);
   
   // Add Hangfire middleware
   app.UseHangfireDashboard("/hangfire");
   ```

2. **CourseManagementController.cs** (upload endpoint)
   ```csharp
   [HttpPost]
   public async Task<ActionResult> UploadScormPackage(...)
   {
       // Upload to temp blob (fast)
       // Queue background job (returns immediately)
       // Return job ID to client
   }
   
   [HttpGet]
   public ActionResult GetUploadStatus(string jobId)
   {
       // Query UploadJobs table
       // Return current status
   }
   ```

3. **Services/CourseUploadService.cs** (NEW - background processing)
   ```csharp
   public class CourseUploadService
   {
       public async Task ProcessScormPackageAsync(string jobId, ...)
       {
           // Download from temp blob
           // Extract ZIP
           // Upload to Azure
           // Save to database
           // Update job status
       }
   }
   ```

---

### Q: How do I test this locally?

**A**: Steps for local testing:

1. **Run database migration**:
   ```sql
   -- Hangfire creates its own tables automatically
   -- You just need to create the UploadJobs table
   CREATE TABLE UploadJobs (...);
   ```

2. **Start the application**:
   ```bash
   dotnet run --project ELG.Web
   ```

3. **Check Hangfire dashboard**:
   - Navigate to `http://localhost:5000/hangfire`
   - Login with your admin credentials
   - Should see "No jobs yet" initially

4. **Upload a test file**:
   - Login as admin
   - Go to "Upload SCORM Package"
   - Upload a ZIP file (any size)
   - Watch the progress bar update

5. **Monitor in Hangfire**:
   - Refresh `/hangfire` dashboard
   - Go to "Jobs" → "Processing"
   - Should see your job executing
   - When done, check "Succeeded" tab

6. **Check database**:
   ```sql
   SELECT * FROM UploadJobs ORDER BY CreatedAt DESC;
   ```

---

### Q: How do I debug a background job?

**A**: Several ways:

**Option 1: Use the Hangfire Dashboard**
- Go to `/hangfire`
- Click on the failed job
- See full exception stack trace
- Can manually retry the job
- Can delete the job

**Option 2: Check Application Logs**
```csharp
// Add logging in CourseUploadService
Logger.Info($"[Job {jobId}] Starting extraction");
Logger.Error(ex, $"[Job {jobId}] Failed: {ex.Message}");
```

**Option 3: Debug in Visual Studio**
1. Set breakpoint in `ProcessScormPackageAsync`
2. Upload a file
3. Hangfire worker will hit the breakpoint
4. Step through code normally

**Option 4: Query UploadJobs table**
```sql
SELECT JobId, Status, Progress, Message, ErrorMessage
FROM UploadJobs
WHERE Status = 'failed'
ORDER BY CreatedAt DESC;
```

---

### Q: What if a job gets stuck?

**A**: Hangfire has automatic timeout handling:

- Default job timeout: 30 minutes
- If job exceeds timeout, marked as "Failed"
- Can be retried manually from dashboard

To manually unstick a job:

```sql
-- Find stuck jobs (processing for > 30 min)
SELECT * FROM HangFire.Job j
JOIN HangFire.State s ON j.Id = s.JobId
WHERE s.Name = 'Processing'
AND s.CreatedAt < DATEADD(MINUTE, -30, GETDATE());

-- Manually fail the job (Hangfire will retry)
UPDATE HangFire.State
SET Name = 'Failed', Reason = 'Manual intervention - exceeded timeout'
WHERE JobId = @JobId;
```

Or use the Hangfire dashboard:
1. Go to `/hangfire`
2. Click "Processing" tab
3. Find the stuck job
4. Click "Delete" or "Re-queue"

---

### Q: How do I handle errors in background jobs?

**A**: Use try-catch with job status updates:

```csharp
public async Task ProcessScormPackageAsync(string jobId, ...)
{
    try
    {
        UpdateJobStatus(jobId, "extracting", 20, "Extracting ZIP...");
        
        // Do extraction
        await ExtractZipAsync(...);
        
        UpdateJobStatus(jobId, "uploading", 60, "Uploading files...");
        
        // Do upload
        await UploadToAzureAsync(...);
        
        UpdateJobStatus(jobId, "complete", 100, "Success!");
    }
    catch (InvalidDataException ex)
    {
        // User error (bad ZIP file)
        Logger.Error(ex, $"[Job {jobId}] Invalid ZIP file");
        UpdateJobStatus(jobId, "failed", 0, "Invalid SCORM package", 
            errorMessage: "The uploaded file is not a valid ZIP file or is corrupted.");
        // Don't rethrow - job is "complete" (failed, but we handled it)
    }
    catch (Exception ex)
    {
        // Unexpected error - log and rethrow (Hangfire will retry)
        Logger.Error(ex, $"[Job {jobId}] Unexpected error");
        UpdateJobStatus(jobId, "failed", 0, "Processing failed", 
            errorMessage: ex.Message);
        throw; // Rethrow so Hangfire marks as failed and retries
    }
}
```

**Error handling strategy**:
- **Expected errors** (bad user input): Update status to "failed", DON'T rethrow
- **Unexpected errors** (network, database): Update status to "failed", RETHROW for retry
- **Always log** with job ID for troubleshooting

---

### Q: How do I configure the number of concurrent jobs?

**A**: In `Program.cs`:

```csharp
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Number of concurrent jobs
    options.ServerName = "LMS-Upload-Worker"; // Identifier for this server
});
```

**Choosing worker count**:
- **Too low** (1-2): Jobs queue up, users wait longer
- **Too high** (10+): Memory pressure, CPU contention
- **Recommended**: 5 for small/medium App Service, 10 for large

Factor in:
- App Service tier (CPU cores)
- Memory available
- Azure Storage bandwidth
- Database connection pool

**Azure App Service P1V3**: 2 cores, 8GB RAM → 5 workers ✅  
**Azure App Service P2V3**: 4 cores, 16GB RAM → 10 workers ✅  
**Azure App Service P3V3**: 8 cores, 32GB RAM → 15 workers ✅

---

### Q: How often does the client poll for status?

**A**: Default is every 5 seconds:

```javascript
setInterval(async () => {
    const response = await fetch('/CourseManagement/GetUploadStatus?jobId=' + jobId);
    // Update progress bar
}, 5000); // 5 seconds
```

**Trade-offs**:
- **Too frequent** (< 3 sec): Database load, unnecessary requests
- **Too slow** (> 10 sec): Progress bar updates feel sluggish
- **Recommended**: 5 seconds

For large files:
- First 2 minutes: Poll every 5 seconds
- After 2 minutes: Poll every 10 seconds (slower update, less load)

---

### Q: Can users navigate away during upload?

**A**: Yes! The job continues in background:

User flow:
1. Upload starts → Job queued → User sees progress
2. User closes tab or navigates away
3. Job continues processing on server
4. User returns later → Status endpoint shows "complete"
5. User manually navigates to Courses page

**Optional enhancement**: Store job IDs in session or database by user:
```csharp
// On upload:
SaveUserJob(SessionHelper.UserId, jobId);

// On page return:
var pendingJobs = GetUserPendingJobs(SessionHelper.UserId);
if (pendingJobs.Any())
{
    // Show notification: "You have uploads in progress"
}
```

---

## Deployment Questions

### Q: What database changes are needed?

**A**: Two types:

**1. UploadJobs table (manual creation required)**:
```sql
CREATE TABLE UploadJobs (
    JobId NVARCHAR(50) PRIMARY KEY,
    OrganisationId INT NOT NULL,
    UserId INT NOT NULL,
    UserEmail NVARCHAR(200),
    CourseTitle NVARCHAR(200) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
    Progress INT NOT NULL DEFAULT 0,
    Message NVARCHAR(500),
    CourseId INT NULL,
    TempBlobPath NVARCHAR(500),
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME NULL
);
```

**2. Hangfire tables (automatic creation by Hangfire)**:
- Hangfire creates ~10 tables on first run
- All in `HangFire` schema
- No manual intervention needed

**Migration steps**:
1. Run UploadJobs CREATE TABLE script on target database
2. Deploy application
3. Restart App Service
4. Hangfire creates its tables automatically
5. Verify in SQL: `SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('HangFire')`

---

### Q: What environment variables need to be set?

**A**: For Azure deployment:

Hangfire uses the existing connection string:
```
ConnectionStrings__lmsdbEntities = <your connection string>
```

No additional environment variables needed! Hangfire reads from `appsettings.json` or environment variables automatically.

**Verify in Azure Portal**:
1. Go to App Service → Configuration
2. Check `ConnectionStrings__lmsdbEntities` exists
3. Should point to your Azure SQL Database

---

### Q: How do I monitor jobs in production?

**A**: Multiple ways:

**1. Hangfire Dashboard** (recommended):
- Navigate to: `https://app.elearningate.com/hangfire`
- Requires authentication (only admins can access)
- See real-time job counts: Succeeded, Failed, Processing, Queued
- Click on individual jobs for details

**2. SQL Queries**:
```sql
-- Active jobs
SELECT COUNT(*) FROM HangFire.Job j
JOIN HangFire.State s ON j.Id = s.JobId
WHERE s.Name = 'Processing';

-- Failed jobs in last hour
SELECT j.*, s.Reason FROM HangFire.Job j
JOIN HangFire.State s ON j.Id = s.JobId
WHERE s.Name = 'Failed'
AND s.CreatedAt > DATEADD(HOUR, -1, GETDATE());

-- Job success rate
SELECT 
    s.Name,
    COUNT(*) as Count,
    COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() as Percentage
FROM HangFire.Job j
JOIN HangFire.State s ON j.Id = s.JobId
WHERE j.CreatedAt > DATEADD(DAY, -1, GETDATE())
GROUP BY s.Name;
```

**3. Application Insights** (if configured):
```csharp
// Add telemetry tracking
Logger.Info($"Job {jobId} completed in {duration}ms");
```

**4. UploadJobs Table**:
```sql
-- Recent upload activity
SELECT 
    OrganisationId,
    COUNT(*) as TotalUploads,
    SUM(CASE WHEN Status = 'complete' THEN 1 ELSE 0 END) as Successful,
    SUM(CASE WHEN Status = 'failed' THEN 1 ELSE 0 END) as Failed,
    AVG(DATEDIFF(SECOND, CreatedAt, CompletedAt)) as AvgDurationSeconds
FROM UploadJobs
WHERE CreatedAt > DATEADD(DAY, -7, GETDATE())
GROUP BY OrganisationId
ORDER BY TotalUploads DESC;
```

---

### Q: How do I roll back if there's an issue?

**A**: Two rollback strategies:

**Option 1: Disable Hangfire (5 minutes)**
```csharp
// In Program.cs, comment out:
// builder.Services.AddHangfireServer();

// In CourseManagementController.cs, change:
// BackgroundJob.Enqueue(...); // DISABLED
await ProcessScormPackageAsync(...); // Synchronous fallback

// Redeploy
```

**Option 2: Keep Hangfire, increase timeout temporarily**
```xml
<!-- In web.config -->
<aspNetCore requestTimeout="00:20:00" ... />
```

**Full rollback**:
```powershell
# Revert to previous deployment slot
az webapp deployment slot swap `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --slot staging `
  --action swap

# Or redeploy previous version
az webapp deployment source config-zip `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --src ./previous-version.zip
```

**Database cleanup (optional)**:
```sql
-- Remove UploadJobs table
DROP TABLE UploadJobs;

-- Remove Hangfire schema (optional, no harm if left)
DROP SCHEMA HangFire;
```

---

## Troubleshooting

### Issue: Hangfire tables not created

**Symptoms**: Error on startup: "Invalid object name 'HangFire.Job'"

**Solution**:
1. Check connection string is correct
2. Verify database user has CREATE TABLE permission
3. Manually create HangFire schema:
   ```sql
   CREATE SCHEMA [HangFire];
   ```
4. Restart application

---

### Issue: Jobs stuck in "Processing" state

**Symptoms**: Job shows "Processing" for > 30 minutes

**Solution**:
```sql
-- Check if worker server is active
SELECT * FROM HangFire.Server;

-- If no servers, restart App Service

-- Manually fail stuck job (will auto-retry)
UPDATE HangFire.State
SET Name = 'Failed', Reason = 'Timeout'
WHERE JobId = (
    SELECT Id FROM HangFire.Job
    WHERE Id = @StuckJobId
);
```

---

### Issue: Progress bar not updating

**Symptoms**: Progress bar stuck at one percentage

**Solution**:
1. **Check browser console** for JavaScript errors
2. **Verify status endpoint** works:
   ```
   curl https://app.elearningate.com/CourseManagement/GetUploadStatus?jobId=xxx
   ```
3. **Check UploadJobs table**:
   ```sql
   SELECT * FROM UploadJobs WHERE JobId = 'xxx';
   ```
4. **Verify polling interval**:
   ```javascript
   console.log('Polling status for job:', jobId);
   ```

---

### Issue: Memory errors during processing

**Symptoms**: OutOfMemoryException, slow processing

**Solution**:
1. **Reduce concurrent workers**:
   ```csharp
   options.WorkerCount = 3; // Reduce from 5
   ```

2. **Stream ZIP extraction** (don't load all into memory):
   ```csharp
   // Instead of:
   var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
   
   // Use:
   using (var fileStream = File.OpenRead(zipPath))
   using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
   {
       // Process one entry at a time
   }
   ```

3. **Scale up App Service**:
   ```powershell
   az appservice plan update `
     --name elg-prod-plan `
     --resource-group atf-prod-core-infra-rg `
     --sku P2V3  # More memory
   ```

---

## Best Practices

### 1. **Always update job status**
```csharp
// BAD: No status updates
await ExtractZip();
await UploadFiles();
await SaveToDatabase();

// GOOD: Status updates at each phase
UpdateJobStatus(jobId, "extracting", 30);
await ExtractZip();

UpdateJobStatus(jobId, "uploading", 60);
await UploadFiles();

UpdateJobStatus(jobId, "saving", 90);
await SaveToDatabase();

UpdateJobStatus(jobId, "complete", 100);
```

### 2. **Cleanup temp files**
```csharp
try
{
    // Process files
}
finally
{
    // Always cleanup, even if exception
    if (File.Exists(tempPath))
        File.Delete(tempPath);
    
    if (Directory.Exists(tempDir))
        Directory.Delete(tempDir, true);
}
```

### 3. **Log with job ID**
```csharp
Logger.Info($"[Job {jobId}] Starting extraction");
Logger.Error(ex, $"[Job {jobId}] Failed: {ex.Message}");
```

### 4. **Use descriptive messages**
```csharp
// BAD
UpdateJobStatus(jobId, "processing", 50);

// GOOD
UpdateJobStatus(jobId, "uploading", 50, "Uploading file 45 of 120 to Azure Storage...");
```

### 5. **Test failure scenarios**
- Simulate network timeout
- Upload invalid ZIP file
- Restart server during processing
- Fill disk space
- Corrupt database connection

---

## Performance Tips

### 1. **Parallel file uploads**
```csharp
var uploadTasks = files.Select(file => UploadFileAsync(file));
await Task.WhenAll(uploadTasks); // Parallel instead of sequential
```

### 2. **Use streaming for large files**
```csharp
// Stream directly to blob, don't buffer in memory
using (var blobStream = await blobClient.OpenWriteAsync())
using (var fileStream = File.OpenRead(filePath))
{
    await fileStream.CopyToAsync(blobStream);
}
```

### 3. **Batch database updates**
```csharp
// Update status every 5 files, not every file
if (fileCount % 5 == 0)
{
    UpdateJobStatus(jobId, "uploading", progress);
}
```

### 4. **Configure Hangfire for performance**
```csharp
builder.Services.AddHangfire(configuration => configuration
    .UseSqlServerStorage(connStr, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero, // Instant pickup
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true // Better concurrency
    }));
```

---

## Additional Resources

- **Hangfire Documentation**: https://docs.hangfire.io/
- **Azure Blob Storage SDK**: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet
- **ASP.NET Core Background Tasks**: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services

---

**Last Updated**: March 6, 2026  
**Status**: Living Document (update as new questions arise)
