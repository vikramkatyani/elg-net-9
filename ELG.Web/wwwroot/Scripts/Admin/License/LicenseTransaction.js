$(function () {
    $('#txtTransFromDt').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtTransToDt').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('licenceLink');
    UTILS.activateMenuNavigationLink('menu-license-transaction');
    $('[data-toggle="tooltip"]').tooltip();
});

var licenseTransactionHandler = (function () {
    var $alert = $('#divMessage_LicenseTransactions');
    var $learner = $('#txtTransLearner')
    var $ddlTransCourse = $('#ddlTransCourse')
    var $txtTransFrom = $('#txtTransFromDt')
    var $txtTransTo = $('#txtTransToDt')
    var $searchBtn = $('#searchTransReport');
    var $clearSearchBtn = $('#clearSearchTransReport');
    var $downloadBtn = $('#downloadTransReport');

    renderCourseDropDown();

    //function to render list of all locations in organisation
    function renderCourseDropDown() {
        $ddlTransCourse.empty();
        $ddlTransCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllCourses(function (data) {
            if (data && data.courseList != null) {
                $.each(data.courseList, function (index, item) {
                    $ddlTransCourse.append($('<option/>', {
                        value: item.CourseId,
                        text: item.CourseName
                    }))
                });
            }
        })
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        moduleTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $txtTransFrom.val('');
        $txtTransTo.val('');
        moduleTable.draw();
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $learner.val().replace(/'/g, "''"),
            FromDate: $txtTransFrom.val(),
            ToDate: $txtTransTo.val(),
            Course: $ddlTransCourse.val(),
        }

        var path = 'DownloadLicenseTransactions?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    var moduleTable = $('#licenseTransactionDataTable').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering":true,
        "ajax": {
            "url": "LoadLicenseTransactionsData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $learner.val().replace(/'/g, "''");
                data.FromDate = $txtTransFrom.val();
                data.ToDate = $txtTransTo.val();
                data.Course = $ddlTransCourse.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "ActionDate", "name": "td.TransactionDate", "autoWidth": true },
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true },
            { "data": "Action", "name": "tdes.LicenseTransactionDescription", "autoWidth": true },
            { "data": "LicenseCount", "name": "td.TransactionLicenseCount", "autoWidth": true },
            { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
            { "data": "TransBy_FirstName", "name": "coad.strFirstName", "autoWidth": true }
        ],
        columnDefs: [{
            // render learner name
            targets: [4], render: function (a, b, data, d) {
                if (data["Email"] != null && data["Email"] != '')
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '(' + data["Email"] + ')</span>'
                else
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] +'</span>'
            }
        }, {
            // render number
            targets: [5], render: function (a, b, data, d) {
                if (data["TransBy_Email"] != null && data["TransBy_Email"] != '')
                    return '<span>' + data["TransBy_FirstName"] + ' ' + data["TransBy_LastName"] + '(' + data["TransBy_Email"] + ')</span>'
                else
                    return '<span>' + data["TransBy_FirstName"] + ' ' + data["TransBy_LastName"] + '</span>'
            }
        }]
    });

    return {
    }
})();