$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtUpdateRecordCompletionDate').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-sm-report');
    subModuleProgressReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var subModuleProgressReportHandler = (function () {

    var $alert = $('#message_test_report');
    var $testReportContainer = $('#test-report-container')
    var $learner = $('#txtLearner')
    var $learnerStatus = $('#ddlUserStatus')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $ddlSubModule = $('#ddlSubModule')
    var $ddlAccessStatus = $('#ddlAccessStatus')
    var $status = $('#ddlStatus')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchLearnerProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $downloadBtn = $('#downloadProgressReport');

    var recId = 0;

    function showDefaultMessage() {
        var message = '<div > <b>How to use this page:</b> <ul>  <li>Select filter criteria using the dropdowns and input fields.</li>  <li>Click <i class="fa fa-search me-1"></i> Search to view sub-module results that match your selected filters.</li> <li>Click <i class="fa fa-download me-1"></i> Download to export the filtered report data to Excel.</li> <li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li> </ul></div>';
        UTILS.Alert.show($alert, 'default', message);
        $testReportContainer.hide();
    }

    // function to initialise report page
    // bind drop down in search area
    function init() {
        showDefaultMessage();
        //$.urlParam('trainee');
        $txtRptFrom.val("");
        $txtRptTo.val("");
        $txtRptFrom.attr("disabled", "disabled");
        $txtRptTo.attr("disabled", "disabled");

        renderCourseDropDown();
        renderLocationDropDown();
        applyFiltersInUrl();
    }

    function getQueryParams() {
        const params = new URLSearchParams(window.location.search);
        let queryObject = {};

        params.forEach((value, key) => {
            queryObject[key] = value;
        });

        return queryObject;
    }

    function applyFiltersInUrl() {
        // Usage Example
        const queryParams = getQueryParams();
        $learner.val(queryParams.trainee ? decodeURI(queryParams.trainee) : '');
        $ddlCourse.val(queryParams.course ? decodeURI(queryParams.course) : '-1');

        if (queryParams.trainee || queryParams.course) {
            $searchBtn.click();            
        }

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

    //function to render list of all sub-modules for course
    function renderSubModuleDropDown(courseID) {
        $ddlSubModule.empty();
        $ddlSubModule.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllSubModules(courseID, function (data) {
            if (data && data.subModuleist != null) {
                $.each(data.subModuleist, function (index, item) {
                    $ddlSubModule.append($('<option/>', {
                        value: item.SubModuleId,
                        text: item.SubModuleName
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
            SearchText: $learner.val(),
            UserStatus: $learnerStatus.val(),
            Location:$ddlLoc.val(),
            Department: $ddlDep.val(),
            Course: $ddlCourse.val(),
            AccessStatus: $ddlAccessStatus.val(),
            Status: $status.val(),
            From: $txtRptFrom.val(),
            To: $txtRptTo.val(),
        }

        var path = 'DownloadLearningProgress?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $testReportContainer.show();
        //progressReport.draw();
        $('#learningProgressReport').DataTable().destroy();
        $('#learningProgressReport').DataTable({
            lengthChange: false,
            "processing": true,
            "scrollX": true, 
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadLearningProgress",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val().replace(/'/g, "''");
                    data.UserStatus = $learnerStatus.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                    data.AccessStatus = $ddlAccessStatus.val(),
                        data.Status = $status.val();
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
                { "data": "CourseName", "name": "co.strCourse", "autoWidth": true },
                { "data": "SubModuleName", "name": "sm.strCourse", "autoWidth": true },
                { "data": "AssignedOn", "name": "pd.dateAssignedOn", "autoWidth": true },
                { "data": "LastAccessedDate", "name": "pd.dateLastStarted", "autoWidth": true },
                { "data": "SubModuleStatus", "name": "sm.strStatus", "autoWidth": true },
                { "data": "CompletionDate", "name": "pd.dateCompletedOn", "autoWidth": true }
            ],
            columnDefs: [{
                // render learner name
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }]
        });
    });

    //apply filters for search
    $status.change(function () {
        if ($status.val() != "passed" && $status.val() != "failed" )
        {
            $txtRptFrom.val("");
            $txtRptTo.val("");
            $txtRptFrom.attr("disabled", "disabled");
            $txtRptTo.attr("disabled", "disabled");
        }
        else {
            $txtRptFrom.val("");
            $txtRptTo.val("");
            $txtRptFrom.removeAttr("disabled");
            $txtRptTo.removeAttr("disabled");
        }
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $learnerStatus.val('1');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $ddlSubModule.val('0');
        $ddlAccessStatus.val('0'),
        $status.val('0');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        showDefaultMessage();
        $('#learningProgressReport').DataTable().destroy();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    // populate sub-module drop down on course change
    $ddlCourse.change(function () {
        var selectedCourse = $(this).val();
        renderSubModuleDropDown(selectedCourse);
    });

    return {
        init: init
    }
})();