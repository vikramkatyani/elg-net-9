$(function () {
    UTILS.activateNavigationLink('resellerCompanyLink');
    UTILS.activateMenuNavigationLink('menu-reseller-companies');
    resellerCompanyReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var resellerCompanyReportHandler = (function () {
    var $company = $('#txtCompany');
    var $searchBtn = $('#searchCompany');
    var $clearSearchBtn = $('#clearSearchCompany');

    var companyTable = $('#registeredResellerCompanyList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + "/Reseller/LoadResellerCompanies",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $company.val();
            },
            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "CompanyName", "name": "o.strOrganisation", "autoWidth": true },
            { "data": "AdminEmail", "name": "c.strEmail", "autoWidth": true },
            { "data": "CreationDate", "name": "o.datCreated", "autoWidth": true },
            { "data": "ExpiryDate", "name": "o.UL_datExpire", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [4], render: function (a, b, data, d) {
                return '<button type="button" id="licence-report-' + data["CompanyId"] + '" class="btn btn-sm btn-dark mb-1" onclick="resellerCompanyReportHandler.showLicencePopup(this)"><i class="fa fa-fw fa-list"></i><span>Licence Report</span></button> '
            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        companyTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $company.val('');
        companyTable.draw();
    });

    //function to initialize the view, bind all drop downs
    function init() {
    }

    function showLicencePopup(btn) {
        var companyName = companyTable.row(btn.closest('tr')).data()["CompanyName"];
        var company = btn.id.split('-').pop();
        $("#ttlCompanyName").html(companyName);
        manageCompanyLicenceListHandler.showLicenceReport(company);
    }

    return {
        init: init,
        showLicencePopup: showLicencePopup
    }
})();


var manageCompanyLicenceListHandler = (function () {
    var company = 0;
    var $modal = $('#companyLicenceUsageModal');
    var $alert = $('#errorCompanyLicence');

    // populate department table
    var licenceTable = $('#listCompanyLicences').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "searching": false,
        "paging": false,
        "ajax": {
            "url": hdnBaseUrl + "Reseller/LoadLicenceListForCompany",
            "type": "POST",
            "datatype": "json",
            "async": true,
            "data": function (data) {
                data.Company = company;
            },
            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "CourseName", "autoWidth": true },
            { "data": "AssignedLicences", "autoWidth": true },
            { "data": "ConsumedLicences", "autoWidth": true }
        ]
    });

    //function to show licence report for a company
    function showLicenceReport(id) {
        company = id;
        licenceTable.draw();
        $modal.modal('show');
    }

    return {
        showLicenceReport: showLicenceReport
    }

})();