$(function () {
    UTILS.activateNavigationLink('classroomLink');
    UTILS.activateMenuNavigationLink('menu-class-attendance');
    pendingRequestHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
    $('.divCdate').hide();
});

var pendingRequestHandler = (function () {
    var $classroom = $('#txtClassroom')
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $searchBtn = $('#searchClassroom')
    var $clearSearchBtn = $('#clearSearchClassroom')
    var $alert = $('#divAttendanceMessage')

    var classroomTable = $('#orgClassroomList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "ajax": {
            "url": "LoadClassAttendanceData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.ClassroomName = $classroom.val();
                data.SearchText = $learner.val();
                data.Location = $ddlLoc.val();
                data.Department = $ddlDep.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "CourseName", "name": "CourseName", "autoWidth": true },
            { "data": "FirstName", "name": "FirstName", "autoWidth": true },
            { "data": "EmailId", "name": "EmailId", "autoWidth": true },
            { "data": "Location", "name": "Location", "autoWidth": true },
            { "data": "Department", "name": "Department", "autoWidth": true },
            { "data": "ClassStatus", "name": "ClassStatus", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [1], render: function (a, b, data, d) {
                return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>';
            }
        }, {
            // render action buttons in the last column
                targets: [6], render: function (a, b, data, d) {
                    if (data["ClassStatus"] == "Invite accepted") {
                        return  '<div class="displayDate">  <a data-html="true" data-toggle="tooltip" title="Mark as attended" onclick="updateClassAttendanceHandler.showPopUpFunction(' + data["Course"] + ',' + data["UserID"] + ')" class="btn btn-secondary" href="#"><span class="fa fa-fw fa-calendar"></span>&nbsp;Attended</a> </div>';
                    }

                    else {
                        return '<div class="displayDate"> <a class="btn btn-default disabled" href="#"><span class="fa fa-fw fa-check"></span>' + data["ClassStatus"] + ' (Attended on - ' + data["AttendedOn"] + ')</a></div>';
                    }
                }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        classroomTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $classroom.val('');
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        classroomTable.draw();
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

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    // bind drop down in search area
    function init() {
        renderLocationDropDown();
    }

    return {
        init:init
    }
})();


var updateClassAttendanceHandler = (function () {
    $modal = $('#attendClassroomModal');
    $title = $('#attendClassroomTitle')
    $attendDate = $('#attendClassDate');
    $status = $('#attendClassStatus');
    $alert = $('#attendClassroomMessage');
    $updateInfo = $('#btnMarkClassAttended');

    //$attendDate.datepicker({ dateFormat: 'yy-mm-dd' });
    $attendDate.datepicker({
        dateFormat: 'yy-mm-dd',
        autoHide: true,
        zIndex: 2048,
    });

    var class_id = 0;
    var learner_id = 0;

    function showPopUpFunction(classId, userId) {
        class_id = classId;
        learner_id = userId;
        $attendDate.val('');
        $status.val('1');
        $modal.modal('show');
        UTILS.Alert.hide($alert);
    }

    $updateInfo.click(function (e) {
        e.preventDefault();

        if ($attendDate.val() == null || $attendDate.val() == '') {
            UTILS.Alert.show($alert, "error", "Please select attended on date to mark as attended.");
            return false;
        }

        var cTable = $('#orgClassroomList').DataTable();
        var classroom = {
            Course: class_id,
            UserID: learner_id,
            ClassStatus: $status.val(),
            AttendedOn: $attendDate.val()
        }

        var url = hdnBaseUrl + "Classroom/MarkClassAttended";
        UTILS.makeAjaxCall(url, classroom, function (result) {
            if (result.success == 1) {
                UTILS.Alert.show($alert, "success", 'Class marked successfully.');
                cTable.draw();
            }
            else {
                UTILS.Alert.show($alert, "error", 'Failed to mark the class.');
            }
        }, function (err) {
            UTILS.Alert.show($alert, 'error', 'Something went wrong. Please try again later.')
            console.log(err);
        });
    });

    return {
        showPopUpFunction: showPopUpFunction
    }
})();