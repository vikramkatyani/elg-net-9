$(function () {
    UTILS.activateNavigationLink('settingLink');
    UTILS.activateMenuNavigationLink('menu-announcement');
    announcementReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
    $('.popup-calendar').datepicker({ dateFormat: 'yy-mm-dd', autoHide: true });
});


var addAnnouncementHandler = (function () {
    var $modal = $('#addAnnouncementModal');
    var $title = $('#txtAnnouncementName');
    var $summary = $('#txtAnnouncementSummary');
    var $publishDate = $('#txtPublishDate');
    var $expiryDate = $('#txtExpiryDate');
    var $alert = $('#addAnnouncementMessage');
    var $createBtn = $('#btnCreateAnnouncement');

    $summary.Editor();
    var ann = "";
    var datPublish = "";
    var datExpiry = "";

    function showAddAnnouncementPopUP() {
        UTILS.Alert.hide($alert);
        $title.val('');
        $("#hdntxtSummary").val('');
        setVal();
        $modal.modal('show')
    }

    function setVal() {
        var val = $("#hdntxtSummary").val();
        $(".Editor-editor").html(val);
    }

    function copyAnnouncement() {
        $("#hdntxtSummary").val('');
        ann = $(".Editor-editor").html();
        $("#hdntxtSummary").val(ann);
    }

    $createBtn.click(function (e) {
        e.preventDefault();
        copyAnnouncement();

        datPublish = $publishDate.val();
        datExpiry = $expiryDate.val();

        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "Announcement/CreateAnnouncement"
            var data = {
                Title: $title.val(),
                Summary: ann,
                PublishDate: datPublish,
                ExpiryDate: datExpiry
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Announcement created successfully");
                    $('#orgAnnouncementList').DataTable().draw();
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add announcement.")
                }
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    UTILS.Alert.show($alert, "error", "Failed to add announcement. Please try again later")
                    console.log(err);
                    UTILS.resetButton($createBtn);
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "All fields are mandatory. Please fill value in all fields.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {

        var isValid = true;
        if ($title.val() == null || $title.val().trim() == '') {
            isValid = false;
        }
        if (ann == null || ann == '' ) {

            isValid = false;
        }
        if (datPublish == null || datPublish == '') {

            isValid = false;
        }
        if (datExpiry == null || datExpiry == '') {

            isValid = false;
        }

        return isValid
    }

    function errorMessage() {

    }

    return {
        showAddAnnouncementPopUP: showAddAnnouncementPopUP
    }
})();

var announcementReportHandler = (function () {
    var $announcement = $('#txtAnnouncement');
    var $searchBtn = $('#searchAnnouncement');
    var $clearSearchBtn = $('#clearSearchAnnouncement');
    var $alert = $('#announcementMessage');

    var $updatemodal = $('#updateAnnouncementModal');
    var $updatetitle = $('#txtUpdateAnnouncementName');
    var $updatesummary = $('#txtUpdateAnnouncementSummary');
    var $updatepublishDate = $('#txtUpdatePublishDate');
    var $updateexpiryDate = $('#txtUpdateExpiryDate');
    var $updatealert = $('#updateAnnouncementMessage');
    var $updateBtn = $('#btnUpdateAnnouncement');
    var annId = 0;
    var updateann = "";

    $updatesummary.Editor();

    $('#btnAddAnnouncement').click(function () {
        addAnnouncementHandler.showAddAnnouncementPopUP();
    });

    var announcementTable = $('#orgAnnouncementList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "ajax": {
            "url": "LoadAnnouncementData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $announcement.val()
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "Title", "name": "Title", "autoWidth": true },
            { "data": "PublishDate", "name": "PublishDate", "autoWidth": true },
            { "data": "ExpiryDate", "name": "ExpiryDate", "autoWidth": true },
            { "data": "DateCreated", "name": "DateCreated", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [4], render: function (a, b, data, d) {
                return '<button type="button"id="edit-announcement-' + data["AnnouncementId"] + '" class="btn btn-sm btn-dark mb-1" onclick="announcementReportHandler.updateAnnouncement(this)"><i class="fa fa-fw fa-edit"></i><span>Edit</span></button> '
                    + '<button type="button" id="delete-announcement-' + data["AnnouncementId"] + '" class="btn btn-sm btn-dark mb-1" onclick="announcementReportHandler.deleteAnnouncement(this)"><i class="fa fa-fw fa-trash"></i><span>Archive</span></button> '

            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        announcementTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $announcement.val('');
        announcementTable.draw();
    });

    function init() {

    }

    //function to delete learner and refresh table
    function deleteAnnouncement(btn) {
        if (confirm("Are you sure you want to archive this Announcement?")) {
            var annId = btn.id.split('-').pop();
            var annTable = $('#orgAnnouncementList').DataTable();
            //selector
            $.ajax({
                type: 'post',
                url: 'ArchiveAnnouncement',
                dataType: 'json',
                data: { AnnouncementId: annId },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Announcement archived successfully.');
                        annTable.row($(btn).parents('tr')).remove().draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to delete announcement.');
                },
                error: function (status) {
                    UTILS.Alert.show($alert, 'error', 'Something went wrong. Please try again later.');
                    console.log(status);
                }
            });
        }
    }

    //function to update user info
    function updateAnnouncement(btn) {
        UTILS.Alert.hide($updatealert);
        annId = btn.id.split('-').pop();
        var annTitle = announcementTable.row(btn.closest('tr')).data()["Title"];
        var annSummary = announcementTable.row(btn.closest('tr')).data()["Summary"];
        var annPublishDate = announcementTable.row(btn.closest('tr')).data()["PublishDate"];
        var annExpiryDate = announcementTable.row(btn.closest('tr')).data()["ExpiryDate"];

        $updatetitle.val(annTitle);
        $updatepublishDate.val(annPublishDate);
        $updateexpiryDate.val(annExpiryDate);

        setSummaryVal(annSummary);

        $updatemodal.modal('show');
    }

    $updateBtn.click(function (e) {
        e.preventDefault();
        copyAnnouncement();
        var annTable = $('#orgAnnouncementList').DataTable();
        var announcement = {
            AnnouncementId: annId,
            Title: $updatetitle.val(),
            Summary: updateann,
            PublishDate: $updatepublishDate.val(),
            ExpiryDate: $updateexpiryDate.val()
        }
        $.ajax({
            type: 'post',
            url: hdnBaseUrl + "Announcement/UpdateAnnouncement",
            dataType: 'json',
            data: announcement,
            success: function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($updatealert, "success", 'Announcement updated successfully.');
                    annTable.draw();
                }
                else {
                    UTILS.Alert.show($updatealert, "error", 'Failed to update announcement.');
                }
            },
            error: function (status) {
                console.log(status);
                UTILS.Alert.show($updatealert, "error", 'Something went wrong. Please try agin later');
            }
        });
    });



    function setSummaryVal(summary) {
        $("#updateAnnouncementModal .Editor-editor").html(summary);
    }

    function copyAnnouncement() {
        $("#hdnUpdatetxtSummary").val('');
        updateann = $("#updateAnnouncementModal .Editor-editor").html();
        $("#hdnUpdatetxtSummary").val(updateann);
    }

    return {
        init: init,
        deleteAnnouncement: deleteAnnouncement,
        updateAnnouncement: updateAnnouncement
    }
})();
