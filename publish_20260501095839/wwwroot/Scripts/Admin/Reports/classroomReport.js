$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-class-report');
    classroomReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var classroomReportHandler = (function () {

    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $ddlAttended = $('#ddlAttended')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchClassromReportBtn');
    var $clearSearchBtn = $('#clearSearchClassromReportBtn');
    var $downloadBtn = $('#downloadClassroomReport');

    // function to initialise report page
    // bind drop down in search area
    function init() {
        renderClassroomDropDown();
        renderLocationDropDown();
    }

    //function to render list of all classroom in organisation
    function renderClassroomDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllClassrooms(function (data) {
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

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText : $learner.val(),
            Location : $ddlLoc.val(),
            Department : $ddlDep.val(),
            Course : $ddlCourse.val(),
            Status : $ddlAttended.val(),
            From : $txtRptFrom.val(),
            To : $txtRptTo.val(),
        }

        var path = 'DownloadClassroomReport?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        classroomReport.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $ddlAttended.val('0');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        classroomReport.draw();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    var classroomReport = $('#classroomReportTable').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": "LoadClassroomReport",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $learner.val();
                data.Location = $ddlLoc.val();
                data.Department = $ddlDep.val();
                data.Course = $ddlCourse.val();
                data.Status = $ddlAttended.val();
                data.From = $txtRptFrom.val();
                data.To = $txtRptTo.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
            { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
            { "data": "Location", "name": "l.strLocation", "autoWidth": true },
            { "data": "Department", "name": "d.strDepartment", "autoWidth": true },
            { "data": "CourseName", "name": "co.strTitle", "autoWidth": true },
            { "data": "Venue", "name": "pd.strLocation", "autoWidth": true },
            { "data": "AttendedOn", "name": "pd.datAttended", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [0], render: function (a, b, data, d) {
                return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
            }
        }]
    });
    return {
        init: init
    }
})();