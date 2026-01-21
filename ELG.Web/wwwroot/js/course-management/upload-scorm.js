// SCORM Package Upload Handler
$(document).ready(function () {
    // Activate navigation links
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-upload-scorm');
    
    // Set quota exhausted flag from server
    const quotaExhausted = window.quotaExhausted || false;
    const maxAllowedCourses = window.maxAllowedCourses || 0;
    const currentCourseCount = window.currentCourseCount || 0;
    
    console.log('SCORM Upload - Quota Check:', {
        quotaExhausted: quotaExhausted,
        maxAllowedCourses: maxAllowedCourses,
        currentCourseCount: currentCourseCount
    });

    // Prevent form submission if quota is exhausted
    $('#scormUploadForm').on('submit', function (e) {
        e.preventDefault();
        
        console.log('Form submit triggered - Checking quota...');
        console.log('quotaExhausted:', quotaExhausted, 'maxAllowedCourses:', maxAllowedCourses);
        
        if (quotaExhausted && maxAllowedCourses > 0) {
            console.log('BLOCKING: Quota exhausted!');
            toastr.error('Course quota exhausted. You have reached the maximum limit of ' + maxAllowedCourses + ' courses.', 'Quota Limit Reached');
            return false;
        }

        console.log('Validating form...');
        // Validate form before submission
        if (!validateForm()) {
            return false;
        }
        
        console.log('Form validation passed - submitting via AJAX');
        submitFormAjax();
    });

    // Submit form via AJAX
    function submitFormAjax() {
        const formData = new FormData($('#scormUploadForm')[0]);
        const uploadBtn = $('#uploadBtn');
        const originalButtonText = uploadBtn.html();

        // Disable button and show loading state
        uploadBtn.prop('disabled', true);
        uploadBtn.html('<i class="fa fa-spinner fa-spin"></i> Uploading...');

        $.ajax({
            url: $('#scormUploadForm').attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            timeout: 600000, // 10 minute timeout for large files
            success: function (response) {
                console.log('Upload response:', response);
                
                if (response.Err === 0) {
                    toastr.success(response.Message, 'Success!');
                    
                    // Clear the form
                    $('#scormUploadForm')[0].reset();
                    $('#thumbnailPreview').html('');
                    $('#zipFileError').hide();
                    $('#thumbnailError').hide();
                    
                    // Re-enable button
                    uploadBtn.prop('disabled', false);
                    uploadBtn.html(originalButtonText);
                    
                    // Scroll to top to show success message
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                } else {
                    toastr.error(response.Message || 'An error occurred during upload.', 'Upload Failed');
                    // Re-enable button
                    uploadBtn.prop('disabled', false);
                    uploadBtn.html(originalButtonText);
                }
            },
            error: function (xhr, status, error) {
                console.error('Upload error:', error);
                toastr.error('An error occurred during upload. Please try again.', 'Upload Error');
                // Re-enable button
                uploadBtn.prop('disabled', false);
                uploadBtn.html(originalButtonText);
            }
        });
    }

    // Validate form inputs
    function validateForm() {
        const courseTitle = $('#CourseTitle').val().trim();
        const scormPackage = $('#ScormPackage').val();

        if (!courseTitle) {
            toastr.error('Please enter a course title', 'Validation Error');
            return false;
        }

        if (!scormPackage) {
            toastr.error('Please select a SCORM package file', 'Validation Error');
            return false;
        }

        // Check if file is a ZIP
        const fileName = scormPackage.split('\\').pop();
        if (!fileName.toLowerCase().endsWith('.zip')) {
            toastr.error('Only ZIP files are allowed for SCORM packages', 'Invalid File Type');
            return false;
        }

        // Validate thumbnail if provided
        const thumbnail = $('#Thumbnail').val();
        if (thumbnail) {
            const allowedImageTypes = ['jpg', 'jpeg', 'png', 'gif', 'webp'];
            const fileExtension = thumbnail.split('.').pop().toLowerCase();
            if (!allowedImageTypes.includes(fileExtension)) {
                toastr.error('Only image files (JPEG, PNG, GIF, WebP) are allowed for thumbnails', 'Invalid Thumbnail');
                return false;
            }
        }

        return true;
    }

    // Validate ZIP file on change
    $('#ScormPackage').on('change', function () {
        validateZipFile(this);
    });

    // Validate thumbnail on change
    $('#Thumbnail').on('change', function () {
        validateThumbnail(this);
    });
});

// Validate ZIP file selection
function validateZipFile(input) {
    const fileName = input.files[0]?.name || '';
    const errorDiv = $('#zipFileError');

    if (fileName && !fileName.toLowerCase().endsWith('.zip')) {
        errorDiv.text('Only ZIP files are allowed').show();
        $(input).val('');
        return false;
    }

    errorDiv.hide();
    return true;
}

// Validate and preview thumbnail
function validateThumbnail(input) {
    const file = input.files[0];
    const errorDiv = $('#thumbnailError');
    const previewDiv = $('#thumbnailPreview');

    if (!file) {
        previewDiv.html('');
        errorDiv.hide();
        return true;
    }

    const allowedImageTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedImageTypes.includes(file.type)) {
        errorDiv.text('Only image files (JPEG, PNG, GIF, WebP) are allowed').show();
        $(input).val('');
        previewDiv.html('');
        return false;
    }

    // Show thumbnail preview
    const reader = new FileReader();
    reader.onload = function (e) {
        previewDiv.html('<img src="' + e.target.result + '" alt="Thumbnail Preview" style="max-width: 150px; max-height: 150px; border: 1px solid #ddd; padding: 5px;">');
    };
    reader.readAsDataURL(file);

    errorDiv.hide();
    return true;
}
