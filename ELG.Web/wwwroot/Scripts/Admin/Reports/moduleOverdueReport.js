$(function () {

    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-overdue-report');
    courseOverdueReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var courseOverdueReportHandler = (function () {

    var fromDate = new Date();
    var toDate = new Date(new Date().setDate(new Date().getDate() + 30));

    $('#txtOverdueFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $("#txtOverdueFrom").datepicker("setDate", fromDate);

    $('#txtOverdueTo').datepicker({ dateFormat: 'yy-mm-dd' });
    $("#txtOverdueTo").datepicker("setDate", toDate);

    var $learner = $('#txtLearner')
    //var $ddlLoc = $('#ddlLocation')
    //var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    //var $status = $('#ddlStatus')
    var $txtOverdueFrom = $('#txtOverdueFrom')
    var $txtOverdueTo = $('#txtOverdueTo')
    var $searchBtn = $('#searchOverdueProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $downloadBtn = $('#downloadProgressReport');

    // function to initialise report page
    // bind drop down in search area
    function init() {
        renderCourseDropDown();
        //renderLocationDropDown();
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

    ////function to render list of all locations in organisation
    //function renderLocationDropDown() {
    //    $ddlLoc.empty();
    //    $ddlLoc.append($('<option/>', { value: '0', text: 'All' }));

    //    UTILS.data.getAllLocations(function (data) {
    //        if (data && data.locationList != null) {
    //            $.each(data.locationList, function (index, item) {
    //                $ddlLoc.append($('<option/>', {
    //                    value: item.LocationId,
    //                    text: item.LocationName
    //                }))
    //            });
    //        }
    //    })
    //}

    ////function to render list of all departments for location
    //function renderDepartmentDropDown(locationId) {
    //    $ddlDep.empty();
    //    $ddlDep.append($('<option/>', { value: '0', text: 'All' }));

    //    UTILS.data.getAllDepartments(locationId, function (data) {
    //        if (data && data.departmentList != null) {
    //            $.each(data.departmentList, function (index, item) {
    //                $ddlDep.append($('<option/>', {
    //                    value: item.DepartmentId,
    //                    text: item.DepartmentName
    //                }))
    //            });
    //        }
    //    })
    //}

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $learner.val(),
            //Location: $ddlLoc.val(),
            //Department: $ddlDep.val(),
            Course: $ddlCourse.val(), 
            //Status: $status.val(),
            From: $txtOverdueFrom.val(),
            To: $txtOverdueTo.val(),
        }

        var path = 'DownloadOverDue?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        progressReport.draw();
    });

    ////apply filters for search
    //$status.change(function () {
    //    if ($status.val() != "incomplete") { $txtRptFrom.val(""); $txtRptTo.val(""); $txtRptFrom.attr("disabled", "disabled"); $txtRptTo.attr("disabled", "disabled"); }
    //    else { $txtRptFrom.val(""); $txtRptTo.val(""); $txtRptFrom.removeAttr("disabled"); $txtRptTo.removeAttr("disabled"); }
    //});

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        //$ddlLoc.val('0');
        //$ddlDep.val('0');
        $ddlCourse.val('0');
        //$status.val('0');
        //$txtRptFrom.val('');
        //$txtRptTo.val('');
        progressReport.draw();
    });

    //// populate department drop down on location change
    //$ddlLoc.change(function () {
    //    var selectedLoc = $(this).val();
    //    renderDepartmentDropDown(selectedLoc);
    //});

    var progressReport = $('#overdueCourseReport').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": "LoadOverdueCourseReport",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $learner.val();
                //data.Location = $ddlLoc.val();
                //data.Department = $ddlDep.val();
                data.Course = $ddlCourse.val();
                //data.Status = $status.val();
                data.From = $txtOverdueFrom.val();
                data.To = $txtOverdueTo.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "FirstName", "name": "co.strFirstName", "autoWidth": true },
            { "data": "EmailId", "name": "co.strEmail", "autoWidth": true }, 
            { "data": "CourseName", "name": "c.strCourse", "autoWidth": true },
            { "data": "AssignedOn", "name": "d.dateAssignedOn", "autoWidth": true },
            { "data": "OverDueDate", "name": "overDue", "autoWidth": true },
            { "data": "CompletionDate", "name": "d.dateCompletedOn", "autoWidth": true }
        ],
        columnDefs: [{
            // render learner name
            targets: [0], render: function (a, b, data, d) {
                return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
            }
        }]
    });
    return {
        init: init
    }
})();