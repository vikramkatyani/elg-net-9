$(function () {
    //$('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    //$('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    //$('#txtUpdateRecordCompletionDate').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-widget-report');
    learningWidgetReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var learningWidgetReportHandler = (function () {

    var $alert = $('#message_test_report');
    var $testReportContainer = $('#test-report-container')
    var $learner = $('#txtLearner')
    var $learnerStatus = $('#ddlUserStatus')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    //var $ddlAccessStatus = $('#ddlAccessStatus')
    //var $status = $('#ddlStatus')
    //var $txtRptFrom = $('#txtRptFrom')
    //var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchLearnerProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $downloadBtn = $('#downloadProgressReport');

    var recId = 0;

    // function to initialise report page
    // bind drop down in search area
    function init() {
        var message = "Using the filters please select the criteria you need and click ‘Search’";
        UTILS.Alert.show($alert, 'info', message);
        $testReportContainer.hide();
        //$.urlParam('trainee');
        //$txtRptFrom.val("");
        //$txtRptTo.val("");
        //$txtRptFrom.attr("disabled", "disabled");
        //$txtRptTo.attr("disabled", "disabled");

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

        UTILS.data.getAllWidgetCourses(function (data) {
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
            SearchText: $learner.val(),
            UserStatus: $learnerStatus.val(),
            Location:$ddlLoc.val(),
            Department: $ddlDep.val(),
            Course: $ddlCourse.val(),
            UserStatus: $learnerStatus.val(),
        }

        var path = 'DownloadWidgetProgress?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $testReportContainer.show();
        //progressReport.draw();
        $('#widgetProgressReport').DataTable().destroy();
        $('#widgetProgressReport').DataTable({
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadWidgetProgress",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val().replace(/'/g, "''");
                    data.UserStatus = $learnerStatus.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                    data.UserStatus = $learnerStatus.val();
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
                { "data": "QuesType", "autoWidth": true },
                { "data": "Question", "autoWidth": true },
                { "data": "Response", "autoWidth": true },
                { "data": "Response_1", "autoWidth": true },
                { "data": "Response_2", "autoWidth": true },
                { "data": "Response_3",  "autoWidth": true },
                { "data": "AfterQuestion", "autoWidth": true },
                { "data": "AfterResponse",  "autoWidth": true },
                { "data": "FeedBackResponse", "autoWidth": true },
                { "data": "FeedBackResponseText", "autoWidth": true }
            ],
            columnDefs: [{
                // render learner name
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                    // render widget type
                    targets: [5], render: function (a, b, data, d) {
                        var qtype = parseInt(data["QuesType"]);
                        switch (qtype) {
                            case 1: 
                                return '<span>TIW</span>';
                            case 2: 
                                return '<span>MAC</span>';
                            default: 
                                return '<span>-</span>';
                        }
                    }
                }
            ]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $learnerStatus.val('1');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $ddlAccessStatus.val('0');
        $('#widgetProgressReport').DataTable().draw();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    return {
        init: init
    }
})();