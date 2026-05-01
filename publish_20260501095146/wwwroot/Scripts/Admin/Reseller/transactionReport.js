$(function () {
    UTILS.activateNavigationLink('resellerCompanyLink');
    UTILS.activateMenuNavigationLink('menu-reseller-licence-transaction');

    var DATE_FORMAT = "yy-mm-dd";
    var FROM_DATE = new Date(new Date().setDate(new Date().getDate() - 30));
    var TO_DATE = new Date();

    $('#txtRptFrom').datepicker({ dateFormat: DATE_FORMAT }).datepicker("setDate", FROM_DATE);
    $('#txtRptTo').datepicker({ dateFormat: DATE_FORMAT }).datepicker("setDate", TO_DATE);

    resellerCompanyReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var resellerCompanyReportHandler = (function () {
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchCompany');
    var $downloadBtn = $('#downloadResellerTransactionReport');

    var DATE_FORMAT = "yy-mm-dd";
    var FROM_DATE = new Date(new Date().setDate(new Date().getDate() - 30));
    var TO_DATE = new Date();

    if ($txtRptFrom.val() == null || $txtRptFrom.val() == '')
        $txtRptFrom.datepicker({ dateFormat: DATE_FORMAT }).datepicker("setDate", FROM_DATE);
    if ($txtRptTo.val() == null || $txtRptTo.val() == '')
        $txtRptTo.datepicker({ dateFormat: DATE_FORMAT }).datepicker("setDate", TO_DATE);

    var transactionTable = $('#resellerLicenceTransactionReport').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + "/Reseller/LoadResellerLicenceTransactionReport",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.FromDate = $txtRptFrom.val() + ' 00:00:00';
                data.ToDate = $txtRptTo.val() + ' 23:59:59';
            },
            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "OrganisationName", "name": "o.strOrganisation", "autoWidth": true },
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true },
            { "data": "Action", "name": "c.strEmail", "autoWidth": true },
            { "data": "LicenseCount", "name": "o.datCreated", "autoWidth": true },
            { "data": "ActionDate", "name": "o.UL_datExpire", "autoWidth": true }
        ]
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        transactionTable.draw();
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $learner.val(),
            Location: $ddlLoc.val(),
            Department: $ddlDep.val(),
            Course: $ddlCourse.val(),
            Status: $status.val(),
            From: $txtRptFrom.val(),
            To: $txtRptTo.val(),
        }

        var path = hdnBaseUrl + "/Reseller/DownloadResellerTransactionReport?" + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //function to initialize the view, bind all drop downs
    function init() {
    }

    return {
        init: init
    }
})();