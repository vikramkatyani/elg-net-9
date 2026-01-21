// Learner Layout JavaScript

var adminModal = null;

$(document).ready(function() {
    // Initialize modal once document is ready
    var modalElement = document.getElementById('adminSwitchModal');
    if (modalElement) {
        adminModal = new bootstrap.Modal(modalElement);
    }
});

function toggleMenu() {
    var menu = document.querySelector('.learner-menu-bar');
    menu.classList.toggle('show');
}

function switchToAdminView() {
    // Clear previous values
    document.getElementById('adminSwitchPassword').value = '';
    document.getElementById('passwordError').style.display = 'none';
    document.getElementById('passwordError').textContent = '';
    
    // Show modal
    if (adminModal) {
        adminModal.show();
    } else {
        // Fallback if modal wasn't initialized
        var modalElement = document.getElementById('adminSwitchModal');
        adminModal = new bootstrap.Modal(modalElement);
        adminModal.show();
    }
}

// Function to confirm admin switch after password verification
function confirmAdminSwitch() {
    var passwordField = document.getElementById('adminSwitchPassword');
    var password = passwordField.value;
    var errorDiv = document.getElementById('passwordError');
    
    if (!password || password.trim() === '') {
        errorDiv.textContent = 'Password is required';
        errorDiv.style.display = 'block';
        return;
    }

    $.ajax({
        url: '/Account/SwitchToAdminView',
        type: 'POST',
        data: { password: password },
        dataType: 'json',
        success: function(response) {
            if (response.Err === 0) {
                window.location.href = response.Url;
            } else {
                errorDiv.textContent = response.Message || 'Invalid password';
                errorDiv.style.display = 'block';
            }
        },
        error: function(xhr, status, error) {
            console.error('Switch to admin error:', error);
            errorDiv.textContent = 'Error switching view. Please try again.';
            errorDiv.style.display = 'block';
        }
    });
}

// Allow Enter key to submit password
$(document).on('keypress', '#adminSwitchPassword', function(e) {
    if (e.which === 13) {
        confirmAdminSwitch();
        return false;
    }
});
