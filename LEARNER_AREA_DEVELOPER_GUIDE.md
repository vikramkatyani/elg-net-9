# LMS_admin Learner Area - Developer Quick Reference

## Quick Start

### File Structure
```
LMS_admin/
├── Areas/Learner/
│   ├── Controllers/
│   │   └── HomeController.cs (Dashboard, MyCourses, MyProfile actions)
│   └── Views/
│       ├── Home/
│       │   ├── Dashboard.cshtml
│       │   ├── MyCourses.cshtml
│       │   └── MyProfile.cshtml
│       └── Shared/
│           └── _LearnerLayout.cshtml (Master layout)
└── wwwroot/
    └── Content/css/
        ├── learner.css (NEW - comprehensive styling)
        ├── egate.css (Shared from LMS_learner)
        ├── courseCard.css (Shared from LMS_learner)
        └── custom.css (Domain-specific styling)
```

## CSS Classes Reference

### Layout Classes
```html
<div class="learner-content-container">
    <!-- Flexbox container for entire layout -->
    
    <nav class="learner-menu-bar">
        <!-- Sidebar navigation (fixed width: 250px) -->
        <!-- Background: gradient (667eea to 764ba2) -->
    </nav>
    
    <div class="learner-main-content">
        <!-- Main content area -->
        <!-- Margin-left: 250px (or 0 on mobile) -->
        <!-- Background: #f8f9fa -->
    </div>
</div>
```

### Typography Classes
```html
<h1 class="page-heading">Page Title</h1>
<!-- Font-size: 32px (24px mobile) -->
<!-- Font-weight: 600 -->
<!-- Border-bottom: 2px solid #007bff -->
<!-- Margin-bottom: 30px -->
```

### Card Classes
```html
<div class="card shadow mb-4">
    <!-- Box-shadow: 0 0.5rem 1rem rgba(0,0,0,0.15) -->
    <!-- Hover: translateY(-2px) -->
    <!-- Margin-bottom: 1.5rem -->
    
    <div class="card-header bg-light">
        <!-- Background-color: #f8f9fa -->
        <!-- Border-bottom: 1px solid #ddd -->
    </div>
    
    <div class="card-body">
        <!-- Padding: 20px -->
    </div>
</div>
```

### Menu Classes
```html
<nav class="learner-menu-bar">
    <ul class="learner-menu">
        <li class="nav-item">
            <a href="#">Item</a>
            <!-- Padding: 12px 15px -->
            <!-- Transition: 0.3s -->
        </li>
        
        <li class="nav-item parent-menu">
            <a class="parent-menu-a">
                Parent <i class="fa fa-chevron-down"></i>
            </a>
            <ul class="nav child_menu">
                <!-- Display: none by default -->
                <!-- Display: block when .parent-menu.open -->
                <li><a href="#">Child Item</a></li>
            </ul>
        </li>
    </ul>
    
    <div class="line-partition">
        <!-- Border-top: 1px solid rgba(255,255,255,0.2) -->
    </div>
    
    <div class="profile">
        <!-- Flex column, centered -->
        <!-- Margin-top: 20px -->
        <!-- Padding-top: 15px -->
        
        <img class="img-circle profile_img" src="...">
        <!-- Width: 80px, Height: 80px -->
        <!-- Border: 3px solid white -->
        <!-- Border-radius: 50% -->
        
        <div class="profile-details">
            <span>Welcome,</span>
            <h2>User Name</h2>
        </div>
    </div>
</nav>
```

### Button Classes
```html
<!-- Bootstrap buttons -->
<button class="btn btn-primary">Primary Button</button>
<button class="btn btn-secondary">Secondary Button</button>
<button class="btn btn-success">Success Button</button>

<!-- Custom buttons -->
<button class="elg-btn elg-btn-search">
    <i class="fa fa-search"></i> Search
</button>
<!-- Padding: 8px 16px -->
<!-- Background: #007bff -->
<!-- Hover: #0056b3 -->
```

### Badge Classes
```html
<span class="badge status-completed">Completed</span>
<!-- Background: #28a745 (green) -->

<span class="badge status-in-progress">In Progress</span>
<!-- Background: #ffc107 (yellow) -->

<span class="badge status-not-started">Not Started</span>
<!-- Background: #6c757d (gray) -->
```

### Search Box Classes
```html
<div class="input-group search-box">
    <!-- Box-shadow: 0 0.125rem 0.25rem rgba(0,0,0,0.075) -->
    <!-- Border-radius: 5px -->
    
    <input type="text" class="form-control" placeholder="...">
    <button class="btn elg-btn elg-btn-search">Search</button>
</div>
```

## Common HTML Patterns

### Dashboard Quick Link Card
```html
<div class="col-lg-3 col-md-6 col-sm-6 col-xs-12">
    <div class="card shadow mb-4">
        <div class="card-body text-center">
            <i class="fa fa-book fa-3x text-primary mb-3"></i>
            <h5 class="card-title">My Courses</h5>
            <p class="card-text text-muted">Access your assigned courses...</p>
            <a href="@Url.Action(...)" class="btn btn-primary btn-sm">
                View Courses
            </a>
        </div>
    </div>
</div>
```

### Course Card (JavaScript Generated)
```javascript
html += '<div class="col-lg-6 col-md-12 mb-4">';
html += '<div class="card shadow h-100">';
html += '<div class="card-body">';
html += '<div class="d-flex justify-content-between align-items-start mb-2">';
html += '<h5 class="card-title mb-0">' + course.CourseName + '</h5>';
html += '<span class="badge bg-success">' + statusText + '</span>';
html += '</div>';
html += '<p class="card-text text-muted small">' + description + '</p>';
html += '<div class="progress mb-3" style="height: 5px;">';
html += '<div class="progress-bar" style="width: ' + score + '%"></div>';
html += '</div>';
html += '<p class="mb-3"><small class="text-muted">Progress: <strong>' + score + '%</strong></small></p>';
html += '<a href="#" class="btn btn-primary btn-sm"><i class="fa fa-play-circle"></i> Launch</a>';
html += '</div></div></div>';
```

### Profile Information Card
```html
<div class="card shadow">
    <div class="card-header bg-light">
        <h5 class="mb-0"><i class="fa fa-info-circle"></i> Profile Information</h5>
    </div>
    <div class="card-body">
        <div class="row mb-4">
            <div class="col-md-4">
                <strong><i class="fa fa-user text-primary"></i> Full Name:</strong>
            </div>
            <div class="col-md-8">
                <p>@userDisplayName</p>
            </div>
        </div>
        <!-- Repeat for other fields -->
    </div>
</div>
```

## Responsive Breakpoints

```css
/* Desktop (no breakpoint) */
.col-lg-3 { flex: 0 0 25%; }
.col-md-4 { flex: 0 0 33.333%; }

/* Tablet (max-width: 768px) */
@media (max-width: 768px) {
    .learner-menu-bar {
        left: -250px;  /* Hidden by default */
    }
    .learner-menu-bar.active {
        left: 0;  /* Shown when active */
    }
    .learner-main-content {
        margin-left: 0;
        padding-top: 60px;  /* Account for hamburger */
    }
}

/* Mobile (max-width: 576px) */
@media (max-width: 576px) {
    .col-md-4 { flex: 0 0 100%; }
    .col-md-6 { flex: 0 0 100%; }
    .page-heading { font-size: 20px; }
}
```

## JavaScript Patterns

### Toggle Mobile Menu
```javascript
function toggleMenu() {
    var menuBar = document.querySelector('.learner-menu-bar');
    menuBar.classList.toggle('active');
}

// Hamburger click
document.querySelector('.hamburger')
    .addEventListener('click', toggleMenu);
```

### Dropdown Menu Toggle
```javascript
document.querySelectorAll('.learner-menu .parent-menu-a')
    .forEach(function(element) {
        element.addEventListener('click', function(e) {
            e.preventDefault();
            this.parentElement.classList.toggle('open');
        });
    });
```

### Modal for Admin Switch
```javascript
function switchToAdminView() {
    var modal = new bootstrap.Modal(
        document.getElementById('adminSwitchModal')
    );
    modal.show();
}

function confirmAdminSwitch() {
    var password = document.getElementById('adminPassword').value;
    // AJAX call to verify and switch...
}
```

## Color Palette

```css
:root {
    /* Primary colors */
    --primary-color: #007bff;        /* Blue */
    --secondary-color: #6c757d;      /* Gray */
    --success-color: #28a745;        /* Green */
    --warning-color: #ffc107;        /* Yellow */
    --danger-color: #dc3545;         /* Red */
    --info-color: #17a2b8;           /* Cyan */
    
    /* Background colors */
    --light-color: #f8f9fa;          /* Light Gray */
    --dark-color: #343a40;           /* Dark Gray */
    
    /* Text and borders */
    --text-color: #333;              /* Dark Text */
    --border-color: #ddd;            /* Light Border */
    
    /* Sidebar */
    --sidebar-background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    
    /* Shadows */
    --shadow-sm: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
    --shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}
```

## Adding New Pages

### 1. Create View File
Create `LMS_admin/Areas/Learner/Views/Home/NewPage.cshtml`:
```html
@{
    ViewBag.Title = "New Page";
}

<div class="learner-main-content">
    <h1 class="page-heading">New Page Title</h1>
    
    <!-- Your content here -->
</div>
```

### 2. Add Controller Action
Add method to `LMS_admin/Areas/Learner/Controllers/HomeController.cs`:
```csharp
public IActionResult NewPage()
{
    return View();
}
```

### 3. Add Menu Item
In `_LearnerLayout.cshtml`, add to `.learner-menu`:
```html
<li class="nav-item">
    <a href="@Url.Action("NewPage", "Home", new { area = "Learner" })">
        <i class="fa fa-icon-name"></i> New Page
    </a>
</li>
```

### 4. Apply Styling
Use the existing CSS classes for consistency:
- `.learner-main-content` for page wrapper
- `.page-heading` for title
- `.card.shadow` for cards
- `.btn btn-primary` for buttons
- Responsive grid classes

## Troubleshooting

### Sidebar Not Showing
- Check `.learner-menu-bar` has `position: fixed`
- Verify sidebar width matches margin in `.learner-main-content`
- Check z-index is higher than content (1000)

### CSS Not Applied
- Ensure CSS file is linked in `_LearnerLayout.cshtml`
- Check file path: `~/content/css/learner.css`
- Clear browser cache (Ctrl+Shift+Del)
- Check for CSS specificity issues

### Responsive Not Working
- Verify viewport meta tag in head
- Check media queries use correct breakpoints
- Test with browser DevTools (F12)
- Ensure Bootstrap 5 is loaded before custom CSS

### Mobile Menu Issues
- Verify hamburger button displays on mobile
- Check `.hamburger` has `display: flex` on max-width: 768px
- Ensure `toggleMenu()` function is defined
- Check `.learner-menu-bar.active` has `left: 0`

## Performance Tips

1. **CSS Optimization**
   - learner.css is ~500 lines - consider splitting if grows
   - Use CSS variables for theme changes
   - Minimize color palette to reduce complexity

2. **Layout Performance**
   - Use flexbox (already implemented)
   - Avoid fixed heights where possible
   - Use CSS transforms for animations (will-change)

3. **Image Optimization**
   - Profile pictures: max 150x150px
   - Company logos: use SVG if possible
   - Lazy load images if many on page

## Browser Support

- Chrome/Edge: Full support
- Firefox: Full support
- Safari: Full support (iOS 12+)
- IE11: Not supported (Bootstrap 5 requires modern browsers)

## Related Documentation

- [UI Modernization Complete](./UI_MODERNIZATION_COMPLETE.md)
- [Before & After Comparison](./BEFORE_AFTER_COMPARISON.md)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)
- [Font Awesome Icons](https://fontawesome.com/icons)
