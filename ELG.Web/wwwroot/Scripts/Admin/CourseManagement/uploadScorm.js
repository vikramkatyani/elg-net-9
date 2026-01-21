(function () {
    function validateForm() {
        var courseTitle = $('#CourseTitle').val().trim();
        var scormPackage = $('#ScormPackage')[0].files.length;

        if (!courseTitle) {
            toastr.error('Please enter a course title', 'Validation Error');
            return false;
        }

        if (scormPackage === 0) {
            toastr.error('Please select a SCORM package (ZIP file)', 'Validation Error');
            return false;
        }

        return true;
    }

    window.validateZipFile = function (input) {
        var errorDiv = document.getElementById('zipFileError');
        errorDiv.style.display = 'none';
        errorDiv.textContent = '';

        if (input.files && input.files[0]) {
            var file = input.files[0];
            var maxSize = 500 * 1024 * 1024; // 500 MB

            if (file.size > maxSize) {
                errorDiv.textContent = 'File size exceeds 500 MB limit.';
                errorDiv.style.display = 'block';
                input.value = '';
                return false;
            }

            if (!file.name.toLowerCase().endsWith('.zip')) {
                errorDiv.textContent = 'Only ZIP files are allowed.';
                errorDiv.style.display = 'block';
                input.value = '';
                return false;
            }
        }

        return true;
    };

    window.validateThumbnail = function (input) {
        var errorDiv = document.getElementById('thumbnailError');
        var previewDiv = document.getElementById('thumbnailPreview');
        errorDiv.style.display = 'none';
        errorDiv.textContent = '';
        previewDiv.innerHTML = '';

        if (input.files && input.files[0]) {
            var file = input.files[0];
            var maxSize = 5 * 1024 * 1024; // 5 MB
            var allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];

            if (file.size > maxSize) {
                errorDiv.textContent = 'File size exceeds 5 MB limit.';
                errorDiv.style.display = 'block';
                input.value = '';
                return false;
            }

            if (!allowedTypes.includes(file.type)) {
                errorDiv.textContent = 'Only image files (JPEG, PNG, GIF, WebP) are allowed.';
                errorDiv.style.display = 'block';
                input.value = '';
                return false;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                previewDiv.innerHTML = '<img src="' + e.target.result + '" style="max-width: 200px; max-height: 200px; border-radius: 5px; border: 1px solid #ddd; padding: 5px;">';
            };
            reader.readAsDataURL(file);
        }

        return true;
    };

    $(document).ready(function () {
        var $form = $('#scormUploadForm');
        if (!$form.length) return;

        var uploadUrl = $form.data('upload-url') || $form.attr('action');

        $form.on('submit', function (e) {
            e.preventDefault();

            if (!validateForm()) {
                return false;
            }

            var originalBtnText = $('#uploadBtn').html();
            $('#uploadBtn').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Uploading...');

            var formData = new FormData(this);

            $.ajax({
                url: uploadUrl,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                timeout: 600000, // 10 minutes timeout for large files
                success: function (response) {
                    // Handle both camel/lowercase and PascalCase keys
                    var err = (response.Err !== undefined) ? response.Err : response.err;
                    var msg = (response.Message !== undefined) ? response.Message : response.message;
                    var url = (response.Url !== undefined) ? response.Url : response.url;

                    if (err === 0) {
                        toastr.success(msg || 'Uploaded successfully', 'Success');

                        // Clear form inputs, previews, and errors
                        try {
                            var formEl = $form[0];
                            if (formEl && formEl.reset) formEl.reset();
                            $('#ScormPackage').val('');
                            $('#Thumbnail').val('');
                            $('#thumbnailPreview').empty();
                            $('#zipFileError').hide().text('');
                            $('#thumbnailError').hide().text('');
                        } catch (e) { /* no-op */ }

                        // Restore button state
                        $('#uploadBtn').prop('disabled', false).html(originalBtnText);
                        // Stay on this page (no redirect)
                    } else {
                        toastr.error(msg || 'Upload failed', 'Error');
                        $('#uploadBtn').prop('disabled', false).html(originalBtnText);
                    }
                },
                error: function () {
                    toastr.error('An error occurred during upload. Please try again.', 'Error');
                    $('#uploadBtn').prop('disabled', false).html(originalBtnText);
                }
            });

            return false;
        });
    });
})();
