$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-upload-user');

    var fileInput = document.getElementById("flmyPostedFile");
    if (fileInput && fileInput.files.length == 0) {
        $('#btnUploadUsersFile').attr('disabled', 'disabled');
    }

    var uploadStatus = $("#hdnUploadStatus").val()
    if (uploadStatus != "" && uploadStatus != null) {
        $('#divUploadStatusMessage').html(uploadStatus)
        $("#userUploadResultModal").modal('show');
    }
});

$('#flmyPostedFile').on('change', function () {
    var fileInput = document.getElementById("flmyPostedFile");
    var hasFile = fileInput && fileInput.files.length > 0;
    var fileName = hasFile ? fileInput.files[0].name : 'No file selected';

    if ($('#spnFileName').length)
        $('#spnFileName').html(fileName);

    if ($('#txtSelectedFile').length)
        $('#txtSelectedFile').val(hasFile ? fileName : '');

    if (!hasFile) {
        $('#btnUploadUsersFile').attr('disabled', 'disabled');
    } else {
        $('#btnUploadUsersFile').removeAttr('disabled');
    }
})