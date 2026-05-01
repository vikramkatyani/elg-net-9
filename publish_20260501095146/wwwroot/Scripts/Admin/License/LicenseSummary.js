$(function () {
    UTILS.activateNavigationLink('licenceLink');
    UTILS.activateMenuNavigationLink('menu-review-license');
    $('[data-toggle="tooltip"]').tooltip();
});

var licenseSummaryHandler = (function () {
    var $course = $('#txtCourse')
    var $searchBtn = $('#searchReviewLicense')
    var $clearSearchBtn = $('#clearSearchReviewLicense')
    var $alert = $('#divMessage_ReviewLicenses');
    var $downloadBtn = $('#downloadReviewLicenceReport');

    var moduleTable = $('#licenseSummaryDataTable').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": "LoadLicenseSummaryData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $course.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true },
            { "data": "TotalLicenses", "name": "cd.AssignedLicenses", "autoWidth": true },
            { "data": "FreeLicenses", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [3], render: function (a, b, data, d) {
                return '<div>' + data["AllocatedLicenses"]+'</div>'+
                    '<button type = "button" class="btn btn-sm btn-secondary mb-1" id="btn-used-' + data["ModuleID"] +'" onclick="manageConsumedLicenseHandler.showLearnerWithConsumedLicenses(this)"> <i class="fa fa-fw fa-eye"></i> <span> Used : ' + data["UsedLicenses"] +'</span></button >'+
                    '&nbsp;<button type="button" class="btn btn-sm btn-secondary mb-1" id="btn-used-' + data["ModuleID"] + '" onclick="manageFreeLicenseHandler.showLearnerWithFreeLicenses(this)"><i class="fa fa-fw fa-eye"></i> <span>Unused : ' + data["AvailableToRevokeLicenses"] + '</span></button>' +
                    '&nbsp;<button type="button" class="btn btn-sm btn-secondary mb-1" disabled id="btn-deleted-' + data["ModuleID"] + '"><i class="fa fa-fw fa-times"></i> <span>Deleted : ' + data["DeletedLicenses"] + '</span></button>'
            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        moduleTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $course.val('');
        moduleTable.draw();
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $course.val().replace(/'/g, "''")
        }

        var path = 'DownloadLicenseSummary?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });


    return {
    }
})();

var pageHandler = (function () {
    var $ddlLoc = $('.ddl-loc');
    var $ddlDep = $('.ddl-dep');

    renderLocationDropDown();

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $ddlLoc.empty();
        $ddlLoc.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                $.each(data.locationList, function (index, item) {
                    $ddlLoc.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }

    //function to render list of all departments for location
    function renderDepartmentDropDown(locationId) {
        $ddlDep.empty();
        $ddlDep.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllDepartments(locationId, function (data) {
            if (data && data.departmentList != null) {
                $.each(data.departmentList, function (index, item) {
                    $ddlDep.append($('<option/>', {
                        value: item.DepartmentId,
                        text: item.DepartmentName
                    }))
                });
            }
        })
    }
})();