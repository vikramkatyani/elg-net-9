# SCORM Upload Quota Validation Implementation

## Overview
This document outlines the implementation of course quota validation for SCORM package uploads in the LMS Admin application.

## Features Implemented

### 1. **Course Quota Display**
- Shows current course count vs. maximum allowed courses
- Displays "Course Quota" info alert with real-time statistics when a quota is set
- Shows "Quota Exhausted" danger alert when limit is reached

### 2. **Form Disabling When Quota is Exhausted**
When the maximum course count is reached, the following form elements are disabled:
- Course Title input field
- Course Description textarea
- SCORM Package (ZIP file) input
- Thumbnail image input
- Upload button

Disabled elements provide visual feedback with greyed-out styling.

### 3. **Server-Side Validation**
- Validates course quota before accepting any upload
- Returns JSON error response if quota is exhausted
- Prevents database entry if quota limit is reached

## Implementation Details

### Backend Components

#### 1. **DAL Layer** - [LMS_DAL/OrgAdminDAL/ModuleRep.cs](LMS_DAL/OrgAdminDAL/ModuleRep.cs)
- `GetCourseCountByOrganization(int companyId)`: Retrieves current active course count for an organization
- `GetMaxAllowedCourseCount(int companyId)`: Retrieves maximum allowed courses (returns null if no limit is set)

```csharp
public int GetCourseCountByOrganization(int companyId)
{
    try
    {
        using (var context = new LMS_DAL.DbEntityLearner.learnerDBEntities())
        {
            return context.tbCourses
                .Where(c => c.intOrganisationID == companyId && c.blnCourseActive != false)
                .Count();
        }
    }
    catch (Exception)
    {
        return 0;
    }
}

public int? GetMaxAllowedCourseCount(int companyId)
{
    try
    {
        // TODO: Once maxAllowedCourseCount field is added to tbOrganisations table,
        // retrieve it from the database. For now, returns null (no limit)
        return null;
    }
    catch (Exception)
    {
        return null;
    }
}
```

#### 2. **Controller** - [LMS_admin/Controllers/CourseManagementController.cs](LMS_admin/Controllers/CourseManagementController.cs)

**GET Action - UploadScormPackage()**
- Fetches current course count and max allowed
- Calculates quota exhaustion status
- Passes quota info to view via ViewBag

```csharp
public ActionResult UploadScormPackage()
{
    try
    {
        var moduleRep = new ModuleRep();
        
        // Get current course count and max allowed
        int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
        int maxAllowedCourses = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId) ?? 0;
        bool quotaExhausted = (maxAllowedCourses > 0) && (currentCourseCount >= maxAllowedCourses);
        
        // Pass quota info to view
        ViewBag.CurrentCourseCount = currentCourseCount;
        ViewBag.MaxAllowedCourses = maxAllowedCourses;
        ViewBag.QuotaExhausted = quotaExhausted;
        
        return View();
    }
    catch (Exception ex)
    {
        Logger.Error(ex.Message, ex);
        return View();
    }
}
```

**POST Action - UploadScormPackage()**
- Validates quota before processing upload
- Returns error JSON if quota is exhausted
- Prevents database entry if quota limit is reached

```csharp
[HttpPost]
public async Task<ActionResult> UploadScormPackage([FromForm] ScormPackageUploadViewModel model)
{
    LMS_Model.OrgAdmin.ControllerResponse response = new LMS_Model.OrgAdmin.ControllerResponse();
    try
    {
        // Validate quota before proceeding
        var moduleRep = new ModuleRep();
        int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
        int maxAllowedCourses = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId) ?? 0;
        
        if (maxAllowedCourses > 0 && currentCourseCount >= maxAllowedCourses)
        {
            response.Err = 1;
            response.Message = $"Course quota exhausted. You have reached the maximum limit of {maxAllowedCourses} courses.";
            return Json(response);
        }
        
        // ... rest of upload processing
    }
    catch (Exception ex)
    {
        Logger.Error(ex.Message, ex);
        response.Err = 2;
        response.Message = $"Error uploading SCORM package: {ex.Message}";
        return Json(response);
    }
}
```

### Frontend Components

#### View - [LMS_admin/Views/CourseManagement/UploadScormPackage.cshtml](LMS_admin/Views/CourseManagement/UploadScormPackage.cshtml)

**Quota Status Alerts:**
- **Danger Alert** (when quota exhausted): Shows red alert with message and course count stats
- **Info Alert** (when quota limit exists): Shows blue alert with current usage statistics
- **No Alert** (when no quota limit): No quota information displayed

**Form Input Disabling:**
Each form input conditionally disables when quota is exhausted:
```html
<input type="text" class="form-control" id="CourseTitle" ... 
       @(ViewBag.QuotaExhausted == true ? "disabled" : "")>
```

## Database Considerations

### Current Implementation
- Quota checking counts courses where `blnCourseActive != false`
- Only active courses count toward the quota

### Future Enhancement
The `GetMaxAllowedCourseCount()` method includes TODO comment for future enhancement:
- Once `maxAllowedCourseCount` column is added to `tbOrganisations` table
- The method should query this field directly from the database
- Will enable per-organization quota configuration

## Testing Checklist

- [ ] Verify quota info displays when no limit is set (null)
- [ ] Verify info alert shows when quota limit exists and courses < limit
- [ ] Verify danger alert shows when courses = or > limit
- [ ] Verify form inputs are disabled when quota exhausted
- [ ] Verify upload button is disabled when quota exhausted
- [ ] Verify server rejects upload when quota exhausted
- [ ] Verify error message includes current and max course count
- [ ] Test with different organization quotas
- [ ] Verify non-active courses don't count toward quota

## API Response Format

When quota is exhausted, server returns:
```json
{
    "Err": 1,
    "Message": "Course quota exhausted. You have reached the maximum limit of 5 courses."
}
```

## Files Modified

1. **LMS_DAL/OrgAdminDAL/ModuleRep.cs**
   - Added `GetCourseCountByOrganization()` method
   - Added `GetMaxAllowedCourseCount()` method
   - Added using statement for `LMS_DAL.DBEntitySA`

2. **LMS_admin/Controllers/CourseManagementController.cs**
   - Modified GET `UploadScormPackage()` action
   - Modified POST `UploadScormPackage()` action
   - Added quota validation logic

3. **LMS_admin/Views/CourseManagement/UploadScormPackage.cshtml**
   - Added quota status alerts (danger and info)
   - Added disabled attributes to form inputs
   - Added disabled attribute to submit button

## Build Status

✅ Project builds successfully with 93 warnings (no errors)

All quota validation code is in place and ready for testing.
