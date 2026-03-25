# Large File Upload Solution Plan - 502 Error Fix

## Problem Summary

**Issue**: Admin users uploading SCORM course packages (~1GB) receive 502 Bad Gateway errors on the production server (app.elearningate.com).

**Current Behavior**:
- File upload starts successfully
- After extended processing time, server returns 502 error
- Course is not saved to database
- User experience is poor with no progress indication

---

## Root Cause Analysis

### 1. **Request Timeout at Infrastructure Level**
The 502 error indicates a gateway/proxy timeout, not an application error. Based on the codebase analysis:

- **Azure Front Door/Application Gateway**: Most likely culprit (per [413-TROUBLESHOOTING.md](413-TROUBLESHOOTING.md#L39))
  - Default timeout: 60-120 seconds
  - Default max body size: 30 MB (Azure Front Door)
  - Even with increased body size, processing time exceeds timeout threshold

- **App Service Timeout**: 
  - web.config has `requestTimeout="00:10:00"` (10 minutes)
  - This is sufficient, so timeout is happening upstream

### 2. **Synchronous Processing Bottleneck**
Current flow in [CourseManagementController.cs](ELG.Web/Controllers/CourseManagementController.cs#L1133):
```
Client Upload → Controller Receives Full File → Extract ZIP → Upload to Azure → Save to DB → Return Response
```

**Problems**:
- Entire 1GB file must be uploaded before processing begins
- ZIP extraction happens synchronously in [AzureStorageUtility.cs](ELG.DAL/Utilities/AzureStorageUtility.cs#L135):
  - Downloads entire ZIP to temp directory
  - Extracts all files to disk
  - Uploads each file to Azure Blob Storage
  - For 1GB SCORM packages, this takes 3-8 minutes
- Connection remains open the entire time
- No progress feedback to client
- Proxy timeout occurs before response is sent

### 3. **Memory Consumption**
From [AzureStorageUtility.cs](ELG.DAL/Utilities/AzureStorageUtility.cs#L145-L150):
```csharp
// Loads entire ZIP into memory
using (var archive = new ZipArchive(zipFile.OpenReadStream(), ZipArchiveMode.Read))
{
    archive.ExtractToDirectory(tempExtractPath);
}
```
- For 1GB files, this consumes significant memory
- Can trigger garbage collection pauses
- May hit Azure App Service memory limits

### 4. **No Progress Tracking**
- User has no visibility into upload/processing status
- Browser connection may timeout on client side
- No way to resume failed uploads

---

## Solution Options

### **Option 1: Asynchronous Background Processing (RECOMMENDED)**

**Description**: Decouple upload from processing using a background job queue.

**Architecture**:
```
Phase 1: Quick Upload
Client → Upload to Azure Blob (Temp) → Return Job ID → HTTP 202 Accepted
         (2-3 minutes)

Phase 2: Background Processing (separate thread)
Background Job → Extract ZIP → Process SCORM → Update DB → Send Notification
                 (5-10 minutes)

Phase 3: Status Polling
Client JS → Poll /api/upload/status/{jobId} → Show Progress → Redirect when complete
            (Every 5 seconds)
```

**Implementation Steps**:

1. **Add Background Job Infrastructure**
   - Install `Hangfire` NuGet package (simple, self-contained)
   - Configure in `Program.cs` with SQL Server storage
   - No external queue service needed (Azure Service Bus not required)

2. **Modify Upload Endpoint**
   ```csharp
   [HttpPost]
   public async Task<ActionResult> UploadScormPackage([FromForm] ScormPackageUploadViewModel model)
   {
       // Validate inputs (quota, file type, etc.)
       
       // Generate unique job ID
       string jobId = Guid.NewGuid().ToString();
       
       // Upload ZIP directly to temp blob storage (fast, streaming)
       string tempBlobPath = await UploadToTempStorage(model.ScormPackage, jobId);
       
       // Queue background job
       BackgroundJob.Enqueue(() => ProcessScormPackage(jobId, tempBlobPath, model, SessionHelper.CompanyId));
       
       // Return immediately with job ID
       return Json(new { 
           success = 1, 
           jobId = jobId,
           message = "Upload started. Processing in background...",
           statusUrl = Url.Action("GetUploadStatus", new { jobId })
       });
   }
   
   // Background processing method
   public async Task ProcessScormPackage(string jobId, string tempBlobPath, ...)
   {
       try {
           // Update status: "Extracting..."
           UpdateJobStatus(jobId, "extracting", 30);
           
           // Download from temp blob and extract
           string startPath = await ExtractScormPackageFromBlob(tempBlobPath, ...);
           
           // Update status: "Uploading files..."
           UpdateJobStatus(jobId, "uploading", 60);
           
           // Save to database
           int courseId = SaveCourseToDatabase(...);
           
           // Update status: "Complete"
           UpdateJobStatus(jobId, "complete", 100, courseId);
           
           // Delete temp blob
           await DeleteTempBlob(tempBlobPath);
       }
       catch (Exception ex) {
           UpdateJobStatus(jobId, "failed", 0, errorMessage: ex.Message);
       }
   }
   
   // Status endpoint
   [HttpGet]
   public ActionResult GetUploadStatus(string jobId)
   {
       var status = GetJobStatus(jobId); // from database or cache
       return Json(new { 
           status = status.State, // "uploading", "extracting", "complete", "failed"
           progress = status.Progress, // 0-100
           message = status.Message,
           courseId = status.CourseId
       });
   }
   ```

3. **Update Frontend (View)**
   ```javascript
   // After form submission receives jobId
   function pollUploadStatus(jobId) {
       $('#uploadProgress').show();
       
       const interval = setInterval(async () => {
           const response = await fetch(`/CourseManagement/GetUploadStatus?jobId=${jobId}`);
           const status = await response.json();
           
           // Update progress bar
           $('#progressBar').css('width', status.progress + '%').text(status.message);
           
           if (status.status === 'complete') {
               clearInterval(interval);
               window.location.href = '/CourseManagement/Courses';
           }
           else if (status.status === 'failed') {
               clearInterval(interval);
               showError(status.message);
           }
       }, 5000); // Poll every 5 seconds
   }
   ```

4. **Create Job Status Table**
   ```sql
   CREATE TABLE UploadJobs (
       JobId NVARCHAR(50) PRIMARY KEY,
       OrganisationId INT NOT NULL,
       UserId INT NOT NULL,
       CourseTitle NVARCHAR(200),
       Status NVARCHAR(20), -- 'pending', 'extracting', 'uploading', 'complete', 'failed'
       Progress INT DEFAULT 0, -- 0-100
       Message NVARCHAR(500),
       CourseId INT NULL,
       ErrorMessage NVARCHAR(MAX) NULL,
       CreatedAt DATETIME DEFAULT GETDATE(),
       UpdatedAt DATETIME DEFAULT GETDATE()
   );
   ```

**Advantages**:
- ✅ **Eliminates 502 errors**: Client receives immediate response
- ✅ **Better UX**: Progress indication, no frozen browser
- ✅ **Scalable**: Can process multiple uploads concurrently
- ✅ **Resilient**: Failed jobs can be retried
- ✅ **Memory efficient**: Can use streaming for large files
- ✅ **No infrastructure changes needed**: Works within existing Azure setup

**Disadvantages**:
- ⚠️ Requires code changes in controller, DAL, and view
- ⚠️ Adds Hangfire dependency (but simple to configure)
- ⚠️ Requires new database table for job tracking

**Estimated Implementation Time**: 4-6 hours

---

### **Option 2: Chunked Upload with Resumability**

**Description**: Split large files into smaller chunks and upload progressively.

**Architecture**:
```
Client → Split 1GB file into 10MB chunks
      → Upload chunk 1 → Upload chunk 2 → ... → Upload chunk N
      → Merge chunks on server → Process normally
```

**Implementation Steps**:

1. **Frontend: Chunked Upload Library**
   ```html
   <!-- Add Resumable.js or UpChunk.js -->
   <script src="https://cdn.jsdelivr.net/npm/resumablejs@1.1.0/resumable.min.js"></script>
   ```

   ```javascript
   var r = new Resumable({
       target: '/CourseManagement/UploadChunk',
       chunkSize: 10 * 1024 * 1024, // 10MB chunks
       simultaneousUploads: 3,
       testChunks: false,
       throttleProgressCallbacks: 1,
       headers: {
           'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
       }
   });
   
   r.on('fileSuccess', function(file, message) {
       // All chunks uploaded, trigger processing
       $.post('/CourseManagement/ProcessChunkedUpload', {
           identifier: file.uniqueIdentifier,
           filename: file.fileName,
           courseTitle: $('#CourseTitle').val()
       });
   });
   
   r.on('fileProgress', function(file) {
       var progress = Math.floor(file.progress() * 100);
       $('#progressBar').css('width', progress + '%');
   });
   ```

2. **Backend: Chunk Receiver**
   ```csharp
   [HttpPost]
   [DisableRequestSizeLimit] // Allow chunks of any size
   public async Task<ActionResult> UploadChunk()
   {
       var chunkNumber = int.Parse(Request.Form["resumableChunkNumber"]);
       var totalChunks = int.Parse(Request.Form["resumableTotalChunks"]);
       var identifier = Request.Form["resumableIdentifier"].ToString();
       var filename = Request.Form["resumableFilename"].ToString();
       var file = Request.Form.Files[0];
       
       // Store chunk in temp location
       string chunkDir = Path.Combine(Path.GetTempPath(), "chunks", identifier);
       Directory.CreateDirectory(chunkDir);
       
       string chunkPath = Path.Combine(chunkDir, $"chunk_{chunkNumber}");
       using (var stream = new FileStream(chunkPath, FileMode.Create))
       {
           await file.CopyToAsync(stream);
       }
       
       return Ok();
   }
   
   [HttpPost]
   public async Task<ActionResult> ProcessChunkedUpload(string identifier, string filename, string courseTitle)
   {
       try
       {
           // Merge chunks into single file
           string chunkDir = Path.Combine(Path.GetTempPath(), "chunks", identifier);
           string mergedPath = Path.Combine(Path.GetTempPath(), identifier + ".zip");
           
           using (var outputStream = new FileStream(mergedPath, FileMode.Create))
           {
               var chunkFiles = Directory.GetFiles(chunkDir).OrderBy(f => f);
               foreach (var chunk in chunkFiles)
               {
                   using (var chunkStream = new FileStream(chunk, FileMode.Open))
                   {
                       await chunkStream.CopyToAsync(outputStream);
                   }
               }
           }
           
           // Now process as normal (extract, upload to Azure, save to DB)
           // ... existing logic ...
           
           // Cleanup
           Directory.Delete(chunkDir, true);
           File.Delete(mergedPath);
           
           return Json(new { success = 1, message = "Upload complete" });
       }
       catch (Exception ex)
       {
           return Json(new { success = 0, message = ex.Message });
       }
   }
   ```

**Advantages**:
- ✅ **Resumable**: Can restart from failed chunk
- ✅ **Better progress feedback**: Real-time chunk-by-chunk progress
- ✅ **Works with existing infrastructure**: No changes to Azure Front Door needed
- ✅ **Network resilient**: Can handle connection drops

**Disadvantages**:
- ⚠️ Still has processing bottleneck after merge
- ⚠️ Complex client-side JavaScript required
- ⚠️ Disk space needed for chunk storage
- ⚠️ Still susceptible to timeout during final processing phase
- ⚠️ Does not solve the 502 error during SCORM extraction/processing

**Estimated Implementation Time**: 6-8 hours

---

### **Option 3: Direct Azure Blob Upload + Notification**

**Description**: Upload directly to Azure Blob Storage from client, then notify server to process.

**Architecture**:
```
Phase 1: Client-Side Upload
Client → Azure SAS Token from Server → Direct upload to Azure Blob Storage
         (Bypasses server completely)

Phase 2: Processing Trigger
Client → Notify server (POST /ProcessUploadedBlob) → Background processing
```

**Implementation Steps**:

1. **Generate SAS Token Endpoint**
   ```csharp
   [HttpGet]
   public ActionResult GetUploadSasToken(string filename)
   {
       // Validate quota first
       int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
       int maxAllowedCourses = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId) ?? 0;
       
       if (maxAllowedCourses > 0 && currentCourseCount >= maxAllowedCourses)
       {
           return Json(new { success = 0, message = "Course quota exhausted" });
       }
       
       // Generate SAS token for blob upload
       string jobId = Guid.NewGuid().ToString();
       string blobPath = $"temp-uploads/{SessionHelper.CompanyId}/{jobId}/{filename}";
       
       var azureStorage = new AzureStorageUtility(...);
       string sasUrl = azureStorage.GenerateBlobSasUrl(blobPath, TimeSpan.FromHours(2));
       
       return Json(new { 
           success = 1, 
           sasUrl = sasUrl,
           jobId = jobId
       });
   }
   ```

2. **Frontend: Azure Blob Upload**
   ```javascript
   // Use Azure Storage JavaScript SDK
   async function uploadToAzure(file, sasUrl) {
       const blockBlobClient = new BlockBlobClient(sasUrl);
       
       // Upload with progress tracking
       await blockBlobClient.uploadData(file, {
           onProgress: (progress) => {
               const percent = (progress.loadedBytes / file.size) * 100;
               $('#progressBar').css('width', percent + '%');
           },
           blockSize: 4 * 1024 * 1024, // 4MB blocks
           concurrency: 5 // Parallel uploads
       });
       
       // Notify server to process
       const response = await fetch('/CourseManagement/ProcessUploadedBlob', {
           method: 'POST',
           body: JSON.stringify({
               jobId: jobId,
               courseTitle: $('#CourseTitle').val(),
               courseDescription: $('#CourseDescription').val()
           })
       });
       
       // Poll for status...
   }
   ```

3. **Backend: Process Notification**
   ```csharp
   [HttpPost]
   public ActionResult ProcessUploadedBlob([FromBody] ProcessBlobRequest request)
   {
       // Queue background job to process the blob
       BackgroundJob.Enqueue(() => ProcessScormFromBlob(
           request.JobId, 
           request.CourseTitle,
           request.CourseDescription,
           SessionHelper.CompanyId
       ));
       
       return Json(new { 
           success = 1, 
           message = "Processing started",
           statusUrl = Url.Action("GetUploadStatus", new { jobId = request.JobId })
       });
   }
   ```

**Advantages**:
- ✅ **Fastest upload**: Direct to Azure, no server hop
- ✅ **Scalable**: Azure handles all bandwidth/concurrency
- ✅ **Eliminates 502 errors**: Server is not involved in upload
- ✅ **Cost efficient**: No egress from App Service for uploads
- ✅ **Parallel uploads**: Azure SDK can upload multiple blocks concurrently

**Disadvantages**:
- ⚠️ Requires Azure Storage JavaScript SDK
- ⚠️ SAS token management needed
- ⚠️ More complex client-side logic
- ⚠️ Still needs background processing (combine with Option 1)

**Estimated Implementation Time**: 5-7 hours

---

### **Option 4: Increase Azure Front Door/Gateway Timeouts (TEMPORARY FIX)**

**Description**: Modify Azure infrastructure to allow longer request durations.

**Implementation Steps**:

1. **Identify the Reverse Proxy**
   ```powershell
   # Check DNS
   nslookup app.elearningate.com
   
   # If points to *.azureedge.net → Azure Front Door
   # If points to *.trafficmanager.net → Traffic Manager
   # If points to *.cloudapp.azure.com → Application Gateway
   ```

2. **For Azure Front Door**:
   ```powershell
   # Increase timeout to 4 minutes (maximum allowed)
   az afd route update \
     --resource-group atf-prod-core-infra-rg \
     --profile-name <front-door-name> \
     --endpoint-name <endpoint-name> \
     --route-name <route-name> \
     --origin-response-timeout-seconds 240
   
   # Increase max request body size to 1GB
   az afd rule set create \
     --profile-name <front-door-name> \
     --resource-group atf-prod-core-infra-rg \
     --rule-set-name LargeUploadRules
   
   az afd rule create \
     --profile-name <front-door-name> \
     --resource-group atf-prod-core-infra-rg \
     --rule-set-name LargeUploadRules \
     --rule-name AllowLargeUploads \
     --action-name ModifyRequestHeader \
     --header-action Append \
     --header-name X-Max-Upload-Size \
     --header-value 1073741824
   ```

3. **For Application Gateway**:
   ```powershell
   # Increase connection timeout
   az network application-gateway http-settings update \
     --resource-group atf-prod-core-infra-rg \
     --gateway-name <gateway-name> \
     --name <settings-name> \
     --timeout 600 \
     --max-request-body-size 1024
   ```

4. **Optimize Processing Speed**
   - Stream file to Azure instead of extracting locally
   - Increase App Service tier for more CPU/memory
   - Use faster storage for temp extraction

**Advantages**:
- ✅ **Quick fix**: Can be done immediately
- ✅ **No code changes**: Infrastructure-only solution
- ✅ **May solve problem**: If timeout is the only issue

**Disadvantages**:
- ❌ **Not a real solution**: Processing will still take 5-10 minutes
- ❌ **Azure Front Door limit**: Max timeout is 4 minutes (may still timeout)
- ❌ **Poor UX**: User still waits with no feedback
- ❌ **Not scalable**: Concurrent uploads will still bottleneck
- ❌ **Memory issues remain**: Large files still cause memory pressure

**Estimated Implementation Time**: 1-2 hours (if you have Azure access)

---

## Recommended Solution

**Primary Recommendation**: **Option 1 (Asynchronous Background Processing)** combined with elements of **Option 3 (Direct Azure Upload)**

### Why This Combination?

1. **Phase 1 - Immediate Fix (2-3 hours)**
   - Implement Option 1 for existing upload flow
   - Add Hangfire background processing
   - Create job status tracking
   - Update UI for progress polling
   - **Result**: 502 errors eliminated, better UX

2. **Phase 2 - Performance Optimization (3-4 hours)**  
   - Add Option 3 for direct Azure upload
   - Keep background processing from Phase 1
   - Reduce server bandwidth usage
   - **Result**: Faster uploads, more scalable

### Implementation Priority

**High Priority (Do First)**:
1. ✅ Add Hangfire for background job processing
2. ✅ Create UploadJobs database table
3. ✅ Modify UploadScormPackage to return immediately with job ID
4. ✅ Add ProcessScormPackage background method
5. ✅ Create GetUploadStatus endpoint
6. ✅ Update frontend to poll status and show progress

**Medium Priority (Do Later)**:
7. Add direct Azure blob upload (Option 3)
8. Optimize SCORM extraction to stream instead of temp files
9. Add email notification when processing completes
10. Add retry logic for failed background jobs

**Low Priority (Nice to Have)**:
11. Implement chunked upload (Option 2) for network resilience
12. Add upload history/logs for admins
13. Pre-validate SCORM package structure before processing

---

## Testing Plan

### Unit Tests
- [ ] Background job queuing works correctly
- [ ] Status updates persist to database
- [ ] Error handling captures all failure scenarios
- [ ] Temp blob/file cleanup happens reliably

### Integration Tests
- [ ] Upload 100MB file → completes successfully
- [ ] Upload 500MB file → completes successfully
- [ ] Upload 1GB file → completes without 502 error
- [ ] Concurrent uploads (3 users) → all succeed
- [ ] Network interruption → job continues processing
- [ ] Server restart during processing → job resumes on restart (Hangfire automatic)

### Performance Tests
- [ ] Memory usage stays under 70% during processing
- [ ] Processing time < 10 minutes for 1GB file
- [ ] Status polling doesn't cause database load
- [ ] Azure blob write speed is acceptable

### User Acceptance Tests
- [ ] Progress bar shows accurate status
- [ ] User can navigate away and come back
- [ ] Error messages are clear and actionable
- [ ] Success notification appears correctly

---

## Rollback Plan

If background processing causes issues:

1. **Keep new upload-to-blob logic** (fast, safe)
2. **Temporarily disable background processing**:
   ```csharp
   // In UploadScormPackage
   // Comment out: BackgroundJob.Enqueue(...)
   // Keep: await ProcessScormPackage(...) // Synchronous fallback
   ```
3. **Increase timeout temporarily** (Option 4) to 4 minutes
4. **Schedule maintenance window** to re-enable async processing

---

## Alternative: Quick Wins Without Major Refactoring

If full implementation is not feasible immediately, these quick wins can help:

1. **Stream ZIP extraction directly to Azure** (2 hours)
   - Don't extract to temp disk
   - Extract each file in ZIP and immediately upload to Azure
   - Reduces disk I/O and temp storage needs

2. **Add client-side progress indicator** (1 hour)
   - Use JavaScript to show "uploading, please wait" message
   - Set realistic expectation: "Large files may take 5-10 minutes"
   - Prevent user from closing tab

3. **Optimize ZIP processing** (1 hour)
   - Use parallel file uploads to Azure
   - Increase buffer sizes
   - Skip unnecessary manifest parsing steps

4. **Increase Azure App Service tier** (15 minutes)
   - Move to P2V3 or P3V3 for more CPU/memory
   - Faster processing = less chance of timeout

---

## Cost Analysis

### Option 1 (Hangfire Background Jobs)
- **NuGet Package**: Free
- **Database Storage**: ~100MB for job tracking (minimal)
- **Compute**: Uses existing App Service resources
- **Total Additional Cost**: $0/month

### Option 3 (Direct Azure Upload)
- **Blob Storage**: Already using Azure Storage
- **Bandwidth**: Reduced (direct upload instead of App Service hop)
- **SAS Token Generation**: Free
- **Total Additional Cost**: $0/month (actually saves money on bandwidth)

### Option 4 (Infrastructure Changes)
- **Azure Front Door**: May require premium tier (~$35/month)
- **Application Gateway**: May require higher tier (~$50/month)
- **App Service**: P2V3 tier (~$150/month, currently on P1V3 at ~$75/month)
- **Total Additional Cost**: $75-110/month

**Recommendation**: Option 1 + 3 has zero additional cost and best ROI.

---

## Security Considerations

### Background Processing
- ✅ Validate user permissions before queuing job
- ✅ Store session data (CompanyId, UserId) with job
- ✅ Re-validate permissions when processing starts
- ✅ Use encrypted connection strings for Hangfire

### Direct Azure Upload
- ✅ Generate SAS tokens with minimal permissions (write-only)
- ✅ Set short expiration (2 hours)
- ✅ Validate file before generating token
- ✅ Use HTTPS-only SAS URLs
- ✅ Clean up temp blobs after 24 hours

### Status Polling
- ✅ Only allow users to query their own jobs
- ✅ Implement rate limiting (max 1 request per 3 seconds)
- ✅ Don't expose sensitive error details in status endpoint

---

## Success Criteria

The solution is successful when:

1. ✅ **No 502 errors** for uploads up to 2GB
2. ✅ **Processing time** < 15 minutes for 1GB files
3. ✅ **User experience** includes progress indication
4. ✅ **Memory usage** stays under 80% during concurrent uploads
5. ✅ **Error rate** < 1% of all uploads
6. ✅ **User satisfaction** improves (measured by support tickets)

---

## Next Steps

1. **Review this plan** with stakeholders
2. **Decide on solution approach** (recommend Option 1 + 3)
3. **Set up development environment** with Hangfire
4. **Create feature branch** for implementation
5. **Implement Phase 1** (background processing)
6. **Test thoroughly** in staging environment
7. **Deploy to production** during low-traffic window
8. **Monitor performance** for 48 hours
9. **Implement Phase 2** if needed

---

## Questions & Discussion

### Q: Can we just increase the timeout?
**A**: No. Azure Front Door has a maximum timeout of 4 minutes. Even at 4 minutes, processing 1GB SCORM packages often takes 5-10 minutes. The user experience would also be terrible (frozen browser for 10 minutes).

### Q: Why not use Azure Functions instead of Hangfire?
**A**: Azure Functions is excellent but:
- Requires separate deployment/configuration
- Adds complexity (Event Grid/Queue trigger setup)
- Costs more ($20-50/month for dedicated plan)
- Hangfire is simpler and runs in-process

### Q: What if the background job fails?
**A**: Hangfire automatically retries failed jobs. We also:
- Log failures to database (UploadJobs table)
- Show error message to user via status endpoint
- Send email notification (optional)
- Allow admin to manually retry from admin panel

### Q: How do we handle server restarts during processing?
**A**: Hangfire persists job state to database. On restart:
- Jobs in "processing" state automatically resume
- No data loss occurs
- User continues to see accurate status

### Q: Can users upload multiple courses simultaneously?
**A**: Yes! Background jobs run concurrently (configurable):
- Default: 5 concurrent jobs
- Can increase based on server capacity
- Each job is isolated and tracked separately

---

## References

- [Current Upload Implementation](ELG.Web/Controllers/CourseManagementController.cs#L1133)
- [Azure Storage Utility](ELG.DAL/Utilities/AzureStorageUtility.cs#L135)
- [413 Error Troubleshooting](413-TROUBLESHOOTING.md)
- [Deployment Checklist](DEPLOYMENT_CHECKLIST.md)
- [Hangfire Documentation](https://www.hangfire.io/)
- [Azure Blob Storage SDK](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet)

---

**Document Version**: 1.0  
**Created**: March 6, 2026  
**Last Updated**: March 6, 2026  
**Author**: GitHub Copilot  
**Status**: Ready for Review
