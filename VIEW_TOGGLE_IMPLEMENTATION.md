# View Toggle Feature - Admin/Learner Switch

## Overview
Users with both admin and learner rights can now toggle between admin and learner views within the LMS_admin application. Security is maintained by requiring password re-entry when switching from learner to admin view.

## Implementation Details

### 1. Session Management Enhancements

**New Session Properties** ([SessionHelper.cs](LMS_admin/Helper/SessionHelper.cs)):
- `HasAdminRights` - Boolean indicating if user has admin access
- `HasLearnerRights` - Boolean indicating if user has learner access
- `IsLearnerUser` - Boolean indicating current active view (true = learner, false = admin)

### 2. Login Flow Enhancement

**Modified Login Method** ([AccountController.cs](LMS_admin/Controllers/AccountController.cs)):
- Detects dual rights during authentication
- For admin users: Checks if they also have learner account via `LearnerAccountRep.GetLearnerInfoByUserID()`
- For learner users: Checks admin rights via `LearnerAccountRep.CheckIfLearnerHasAdminRights()`
- Sets all three flags appropriately based on detection

### 3. Toggle Methods

**Two new controller actions** in AccountController:

#### SwitchToLearnerView (GET)
- **No password required** - switches from admin to learner view
- Validates user has learner rights
- Sets `IsLearnerUser = true`
- Redirects to learner dashboard
- Returns JSON response

```csharp
public IActionResult SwitchToLearnerView()
```

#### SwitchToAdminView (POST)
- **Password required** - switches from learner to admin view
- Validates user has admin rights
- Prompts for password verification
- Re-authenticates against admin credentials
- Refreshes admin privileges and company settings
- Sets `IsLearnerUser = false`
- Redirects to admin dashboard
- Returns JSON response with success/error

```csharp
[HttpPost]
public IActionResult SwitchToAdminView(string password)
```

### 4. UI Integration

#### Admin Layout ([_Layout.cshtml](LMS_admin/Views/Shared/_Layout.cshtml))
**Location:** Settings menu dropdown
- Shows "Switch to Learner View" option if `HasLearnerRights = true`
- No password prompt - immediate switch
- JavaScript function: `switchToLearnerView()`

#### Learner Layout ([_LearnerLayout.cshtml](Areas/Learner/Views/Shared/_LearnerLayout.cshtml))
**Location:** Left navigation menu
- Shows "Switch to Admin View" option if `HasAdminRights = true`
- Opens password verification modal
- JavaScript functions:
  - `switchToAdminView()` - Shows modal
  - `confirmAdminSwitch()` - Validates and submits password

**Password Modal:**
- Bootstrap modal with password input
- Error message display
- Enter key support for submission
- Validates password via AJAX POST

## Security Features

### Password Verification
- Required when switching from learner → admin
- Not required when switching from admin → learner (already authenticated)
- Validates against current user's admin credentials
- Supports master password option
- Password not stored or cached

### Session Security
- View preference stored in session
- Privileges refreshed on admin switch
- Settings reloaded for current context
- No credential exposure in client-side code

## User Experience Flow

### Scenario 1: Admin User with Learner Rights
1. Login with admin credentials
2. See "Switch to Learner View" in Settings menu
3. Click to switch (no password)
4. Redirected to learner dashboard
5. See "Switch to Admin View" in learner menu
6. Click to switch back
7. Enter password in modal
8. Redirected to admin dashboard

### Scenario 2: Learner User with Admin Rights
1. Login with learner credentials
2. Land on learner dashboard
3. See "Switch to Admin View" in menu
4. Click to switch
5. Enter password in modal
6. Redirected to admin dashboard
7. See "Switch to Learner View" in Settings
8. Click to switch back (no password)

### Scenario 3: Single-Role Users
- Admin-only users: No toggle option shown
- Learner-only users: No toggle option shown
- UI adapts automatically based on `HasAdminRights` and `HasLearnerRights` flags

## Technical Details

### JavaScript Functions

**In Admin Layout:**
```javascript
function switchToLearnerView() {
    // AJAX GET to /Account/SwitchToLearnerView
    // Redirect on success
}
```

**In Learner Layout:**
```javascript
function switchToAdminView() {
    // Show password modal
}

function confirmAdminSwitch() {
    // AJAX POST password to /Account/SwitchToAdminView
    // Redirect on success or show error
}
```

### AJAX Endpoints

**GET:** `/Account/SwitchToLearnerView`
- Returns: `{ err: 0/1, url: string, message: string }`

**POST:** `/Account/SwitchToAdminView`
- Parameter: `password` (string)
- Returns: `{ err: 0/1, url: string, message: string }`

## Error Handling

### Common Error Messages
- "You do not have admin access rights"
- "You do not have learner access rights"
- "Password is required"
- "Invalid password"
- "Error switching view"

### Validation
- Password field cannot be empty
- User must have corresponding rights to switch
- Password must match current admin credentials
- Session must be valid

## Testing Checklist

- [ ] Admin-only user: No toggle shown
- [ ] Learner-only user: No toggle shown
- [ ] Dual-rights user logging in as admin: Toggle shown
- [ ] Dual-rights user logging in as learner: Toggle shown
- [ ] Switch admin → learner: No password required
- [ ] Switch learner → admin: Password modal shown
- [ ] Invalid password: Error displayed
- [ ] Empty password: Error displayed
- [ ] Correct password: Switch successful
- [ ] Enter key submits password in modal
- [ ] Cancel button closes modal
- [ ] Session persists view preference
- [ ] Admin privileges refresh on switch to admin
- [ ] Navigation appropriate for current view

## Future Enhancements

1. Remember last view preference across sessions
2. Add notification/toast on successful switch
3. Add keyboard shortcut for quick toggle
4. Add switch history/audit log
5. Support for SSO users with dual rights
6. Add role indicator badge in header
7. Add "View as" label to indicate current context
