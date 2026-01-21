# Learner Area Implementation in LMS_admin

## Overview
This implementation adds a Learner area to the LMS_admin project, allowing both administrators and learners to login through a single login page. After successful authentication, users are redirected based on their role:
- **Admin users** → Admin landing page
- **Learner users** → Learner landing page (Learner Area)

## Changes Made

### 1. AccountController Modifications
**File:** `LMS_admin\Controllers\AccountController.cs`

#### Login Method Enhancement
- Added dual authentication support:
  - First attempts to authenticate as admin using `OrgAdminAccountRep.GetAdmin()`
  - If admin authentication fails, attempts learner authentication using `LearnerAccountRep.GetLearnerInfoByUsernamePassword()`
- Added role-based redirection:
  - Admins are redirected to the admin area: `Home/Dashboard`
  - Learners are redirected to the learner area: `Learner/Home/Dashboard`

#### New Methods Added
- `ValidateLearnerCompany()` - Displays company selection view when multiple learner accounts are found
- `ValidateLearnerCompanyNumber(string companyNumber)` - Validates and processes learner company selection

### 2. SessionHelper Enhancement
**File:** `LMS_admin\Helper\SessionHelper.cs`

#### New Property Added
```csharp
public static bool IsLearnerUser { get; set; }
```
- Used to track if the current user is a learner or admin
- Stored in session for persistent state across requests

### 3. Learner Area Structure

#### Directory Structure
```
LMS_admin/
├── Areas/
│   └── Learner/
│       ├── Controllers/
│       │   └── HomeController.cs
│       └── Views/
│           ├── Home/
│           │   ├── Dashboard.cshtml
│           │   ├── MyCourses.cshtml
│           │   └── MyProfile.cshtml
│           ├── Shared/
│           │   └── _LearnerLayout.cshtml
│           └── _ViewStart.cshtml
```

#### Controllers Created

**LearnerHomeController** (`Areas\Learner\Controllers\HomeController.cs`)
- `Dashboard()` - Learner dashboard view
- `MyCourses()` - View assigned courses
- `GetCourses(string course, string sort)` - AJAX endpoint to fetch learner courses
- `MyProfile()` - View learner profile
- `ResetProgress(Int64 Course, Int64 RecordId)` - Reset course progress

### 4. Views Created

#### Learner Layout
**File:** `Areas\Learner\Views\Shared\_LearnerLayout.cshtml`
- Custom layout for learner area
- Includes navigation menu with:
  - Dashboard
  - My Courses
  - My Profile
  - Logout
- Displays company logo and learner information

#### Dashboard View
**File:** `Areas\Learner\Views\Home\Dashboard.cshtml`
- Welcome message and overview cards
- Quick access to courses, profile, and certificates
- Recent activity section

#### My Courses View
**File:** `Areas\Learner\Views\Home\MyCourses.cshtml`
- Search and filter functionality
- Displays assigned courses with progress
- Course launch capability

#### My Profile View
**File:** `Areas\Learner\Views\Home\MyProfile.cshtml`
- Displays learner information
- Profile picture (if available)
- Company information
- Change password option

#### Company Validation View
**File:** `Views\Account\ValidateLearnerCompany.cshtml`
- Displayed when multiple learner accounts exist with same credentials
- Allows learner to select their company

### 5. Routing Configuration
**File:** `Program.cs`

Added area routing support:
```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

## Login Flow

### For Admin Users
1. User enters credentials on login page
2. System attempts admin authentication
3. If successful, admin session is created
4. User redirected to admin dashboard

### For Learner Users
1. User enters credentials on login page
2. Admin authentication fails
3. System attempts learner authentication
4. If successful, learner session is created with `IsLearnerUser = true`
5. User redirected to learner dashboard in Learner area

### For Users with Multiple Accounts
1. User enters credentials
2. Multiple accounts detected
3. User presented with company selection screen
4. After company selection, appropriate session created
5. User redirected to respective dashboard

## Key Features

### Session Management
- Separate session tracking for learner vs admin users
- `SessionHelper.IsLearnerUser` flag to differentiate user types
- All standard session properties supported for both user types

### Security
- Password encryption maintained for both user types
- Master password support preserved
- Session validation for all protected routes

### Data Access
- Uses existing DAL classes:
  - `LearnerAccountRep` for learner authentication
  - `LearnerCourseRep` for course data
  - All existing learner DAL repositories available

## Usage

### Testing the Implementation

1. **Admin Login:**
   - Navigate to `/Account/Login`
   - Enter admin credentials
   - Verify redirect to admin dashboard

2. **Learner Login:**
   - Navigate to `/Account/Login`
   - Enter learner credentials
   - Verify redirect to `/Learner/Home/Dashboard`

3. **Multiple Account Scenario:**
   - Use credentials associated with multiple companies
   - Verify company selection screen appears
   - Select company and verify correct dashboard loads

## Future Enhancements

Potential additions to the learner area:
1. Course content viewer
2. Certificate download functionality
3. Assessment/quiz taking
4. Risk assessment forms
5. Document viewing
6. Announcements
7. Classroom/training session management
8. Progress tracking and analytics

## Notes

- The learner area uses the same DAL and Model classes from the existing LMS_learner project
- Layout and styling follow the same patterns as admin area for consistency
- All learner functionality from LMS_learner can be gradually migrated to this area
- The implementation maintains backward compatibility with existing admin functionality
