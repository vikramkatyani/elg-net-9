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
    const menuBar = document.querySelector('.learner-menu-bar');
    if (menuBar) {
        menuBar.classList.toggle('active');
    }
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
});
