$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-upload-user');

    if (document.getElementById("flmyPostedFile").files.length == 0) {
        $('#btnUploadUsersFile').attr('disabled', 'disabled');
    }
    var uploadStatus = $("#hdnUploadStatus").val()
    if (uploadStatus != "" && uploadStatus != null) {
        $('#divUploadStatusMessage').html(uploadStatus)
        $("#userUploadResultModal").modal('show');
    }
});

$('#flmyPostedFile').on('change', function () {
    //get the file name
    var fileName = document.getElementById("flmyPostedFile").files[0].name;
    $('#spnFileName').html(fileName);

    if (document.getElementById("flmyPostedFile").files.length == 0) {
        $('#btnUploadUsersFile').attr('disabled', 'disabled');
    } else {
        $('#btnUploadUsersFile').removeAttr('disabled');
    }
})