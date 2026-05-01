$(function () {
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-auto-allocation-report');
    autoAllocationReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var autoAllocationReportHandler = (function () {

    var $alert = $('#message_autoallocation_report');
    var $autoAllocationReportContainer = $('#autoallocation-report-container')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchLearnerProgress');
    var $downloadBtn = $('#downloadProgressReport');
    //var $clearSearchBtn = $('#clearSearchProgressReport');


    // function to initialise report page
    // bind drop down in search area
    function init() {
        var message = "Using the filters please select the criteria you need and click ‘Search’";
        UTILS.Alert.show($alert, 'info', message);
        $autoAllocationReportContainer.hide();
        renderCourseDropDown();
        renderLocationDropDown();
    }

    //function to render list of all locations in organisation
    function renderCourseDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllCourses(function (data) {
            if (data && data.courseList != null) {
                $.each(data.courseList, function (index, item) {
                    $ddlCourse.append($('<option/>', {
                        value: item.CourseId,
                        text: item.CourseName
                    }))
                });
            }
        })
    }

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

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $autoAllocationReportContainer.show();
        //progressReport.draw();
        $('#autoAllocationReport').DataTable().destroy();
        $('#autoAllocationReport').DataTable({
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadAutoAllocationReport",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "columns": [
                { "data": "CourseName", "name": "c.strCourse", "autoWidth": true },
                { "data": "LocationName", "name": "l.strLocation", "autoWidth": true },
                { "data": "DepartmentName", "name": "d.strDepartment", "autoWidth": true },
                { "data": "IsAutoAllocationOn", "name": "cld.autoAllocation", "autoWidth": true }
            ]
        });
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            Location: $ddlLoc.val(),
            Department: $ddlDep.val(),
            Course: $ddlCourse.val()
        }

        var path = 'DownloadAutoAllocationReport?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    return {
        init: init
    }
})();