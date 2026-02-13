// Layout-specific JavaScript functionality

// Base URL - will be set from the server
var hdnBaseUrl;

// Function to switch to learner view (no password required)
function switchToLearnerView() {
    $.ajax({
        url: hdnBaseUrl + 'Account/SwitchToLearnerView',
        type: 'GET',
        success: function(response) {
            if (response.err === 0) {
                window.location.href = response.url;
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Error switching view. Please try again.');
        }
    });
}

// Toggle menu function for mobile hamburger
function toggleMenu() {
    console.log('toggleMenu() called');
    const menuBar = document.querySelector('.learner-menu-bar');
    console.log('menuBar element:', menuBar);
    
    if (menuBar) {
        menuBar.classList.toggle('active');
        const isActive = menuBar.classList.contains('active');
        console.log('Menu is now:', isActive ? 'OPEN' : 'CLOSED');
        console.log('Menu classes:', menuBar.className);
    } else {
        console.error('Could not find .learner-menu-bar element');
    }
}

// Close menu when clicking outside
function closeMenuOnClickOutside() {
    document.addEventListener('click', function(event) {
        const menuBar = document.querySelector('.learner-menu-bar');
        const hamburger = document.querySelector('.hamburger');
        
        if (menuBar && hamburger) {
            // Check if click is outside both hamburger and menu
            if (!menuBar.contains(event.target) && !hamburger.contains(event.target)) {
                menuBar.classList.remove('active');
            }
        }
    });
}

// Close menu when a menu item is clicked
function closeMenuOnItemClick() {
    const menuItems = document.querySelectorAll('.learner-menu a');
    const menuBar = document.querySelector('.learner-menu-bar');
    
    menuItems.forEach(item => {
        item.addEventListener('click', function() {
            if (menuBar && window.innerWidth <= 768) {
                menuBar.classList.remove('active');
            }
        });
    });
}

// Browser compatibility check - IE detection
$(document).ready(function () {
    var ua = window.navigator.userAgent; //Check the userAgent property of the window.navigator object
    var msie = ua.indexOf('MSIE '); // IE 10 or older
    var trident = ua.indexOf('Trident/'); //IE 11

    if (msie > 0 || trident > 0) {
        // if (msie > 0) {
        alert("This browser is no longer supported, you will not be able to log in to your account.\nPlease use an alternative browser such as Google Chrome, Microsoft Edge or Apple Safari.");
        window.open('', '_self', '');
        window.close();
    }

    // Initialize mobile menu handlers
    closeMenuOnClickOutside();
    closeMenuOnItemClick();
});
