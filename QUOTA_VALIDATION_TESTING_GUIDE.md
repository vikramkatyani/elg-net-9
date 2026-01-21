# SCORM Upload Quota Validation - Testing & Configuration Guide

## Current Status

The quota validation framework is fully implemented with both **client-side** and **server-side** validation. However, quota enforcement requires proper configuration.

## How the Quota System Works

### No Quota Set (Default)
- If no quota limit is configured, organizations can upload unlimited courses
- `GetMaxAllowedCourseCount()` returns `null` (converted to 0)
- Upload form displays no quota warnings
- Users can upload freely

### Quota Set (When Configured)
- When max allowed courses is set to a value > 0:
  - Upload form displays quota status alert
  - Form inputs are disabled when limit is reached
  - Client-side validation prevents form submission
  - Server-side validation blocks the upload
  - Error message: "Course quota exhausted. You have reached the maximum limit of X courses."

## Testing the Quota System

### Option 1: Using Environment Variable (Easiest for Testing)

Set the environment variable `COURSE_QUOTA_LIMIT` to test the quota feature:

**Windows (PowerShell):**
```powershell
$env:COURSE_QUOTA_LIMIT = "5"
# Then run/restart the application
```

**Windows (Command Prompt):**
```cmd
set COURSE_QUOTA_LIMIT=5
REM Then run/restart the application
```

**Linux/Mac:**
```bash
export COURSE_QUOTA_LIMIT=5
# Then run/restart the application
```

**Result:** With `COURSE_QUOTA_LIMIT=5`, any organization will be limited to 5 active courses.

### Option 2: Database Configuration (Production)

Add a `maxAllowedCourseCount` column to the `tbOrganisations` table:

```sql
ALTER TABLE tbOrganisations
ADD maxAllowedCourseCount INT NULL;

-- Set quota for specific organization
UPDATE tbOrganisations 
SET maxAllowedCourseCount = 10 
WHERE intOrganisationID = 1;
```

Then update the DAL method in [LMS_DAL/OrgAdminDAL/ModuleRep.cs](LMS_DAL/OrgAdminDAL/ModuleRep.cs):

```csharp
public int? GetMaxAllowedCourseCount(int companyId)
{
    try
    {
        using (var context = new LMS_DAL.DBEntity.lmsdbEntities())
        {
            var org = context.tbOrganisations
                .FirstOrDefault(o => o.intOrganisationID == companyId);
            return org?.maxAllowedCourseCount;
        }
    }
    catch (Exception)
    {
        return null;
    }
}
```

## Testing Steps

### Prerequisites
1. Have a SCORM ZIP package ready for upload
2. Have access to the LMS Admin application

### Test Procedure

1. **Set quota limit** (using environment variable or database)
2. **Create test courses** until you reach the quota limit
   - Example: With `COURSE_QUOTA_LIMIT=2`, upload 2 courses
3. **Navigate** to `CourseManagement/UploadScormPackage`
4. **Verify:**
   - Danger alert appears: "Quota Exhausted"
   - Shows current count: "2 / 2"
   - All form inputs are disabled (greyed out)
   - Upload button is disabled
   - Form input fields show reduced opacity
5. **Attempt upload** (try to bypass by opening browser console)
   - Check browser console for logs: "BLOCKING: Quota exhausted!"
   - toastr error notification appears
   - Form submission is prevented
6. **Check server logs** (Debug output)
   - Look for: "QUOTA EXCEEDED: Current X >= Max Y"

## Implementation Details

### Files Modified

#### 1. [LMS_DAL/OrgAdminDAL/ModuleRep.cs](LMS_DAL/OrgAdminDAL/ModuleRep.cs)
- `GetCourseCountByOrganization()` - Counts active courses for organization
- `GetMaxAllowedCourseCount()` - Returns max allowed (supports env variable override)

#### 2. [LMS_admin/Controllers/CourseManagementController.cs](LMS_admin/Controllers/CourseManagementController.cs)
- GET `UploadScormPackage()` - Fetches quota info, passes to view
- POST `UploadScormPackage()` - Validates quota before accepting upload
- Added debug logging for troubleshooting

#### 3. [LMS_admin/Views/CourseManagement/UploadScormPackage.cshtml](LMS_admin/Views/CourseManagement/UploadScormPackage.cshtml)
- Quota status alerts (danger when exhausted, info when limit exists)
- Disabled form inputs when quota exhausted
- Client-side validation with form submission prevention
- Console logging for debugging

## Validation Layers

### Client-Side (JavaScript)
✓ Checks `quotaExhausted` flag from server  
✓ Prevents form submission with `e.preventDefault()`  
✓ Shows toastr error notification  
✓ Disables UI elements for better UX  
✓ Validates file types and formats  

### Server-Side (C#)
✓ Recalculates current course count  
✓ Fetches max allowed from database/environment  
✓ Rejects upload if limit reached  
✓ Returns JSON error response  
✓ Prevents database entry  
✓ Includes debug logging  

## Debugging

### Console Logs
Open browser DevTools (F12) and check Console tab for:
- `SCORM Upload - Quota Check: { quotaExhausted: false, maxAllowedCourses: 5, currentCourseCount: 2 }`
- `Form submit triggered - Checking quota...`
- `BLOCKING: Quota exhausted!`

### Server Logs
Check application debug output for:
- `SCORM Upload Page - CompanyId: 1, CurrentCount: 2, MaxAllowed: 5, QuotaExhausted: false`
- `SCORM Upload POST - CompanyId: 1, CurrentCount: 2, MaxAllowed: 5`
- `QUOTA EXCEEDED: Current 2 >= Max 5`

## Known Limitations

1. **Environment variable override** - Only works if app restarted
2. **Database column** - Needs to be added manually to `tbOrganisations` table
3. **Per-organization quotas** - Currently supports same quota for all via environment variable
4. **Active courses only** - Only counts courses where `blnCourseActive != false`

## Future Enhancements

- [ ] Add UI to manage per-organization quotas in admin panel
- [ ] Add database migration script for `maxAllowedCourseCount` column
- [ ] Add configuration file support (instead of just environment variable)
- [ ] Add quota alerts/warnings at different thresholds (80%, 90%, etc.)
- [ ] Add audit logging for quota violations
- [ ] Add option to count inactive courses in quota

## Support

If quota validation isn't working:
1. Verify quota is set (environment variable or database)
2. Check browser console for validation logs
3. Check application debug output for server-side logs
4. Verify no JavaScript errors in console
5. Clear browser cache and try again
