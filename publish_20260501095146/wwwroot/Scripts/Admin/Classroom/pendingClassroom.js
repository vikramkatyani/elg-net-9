$(function () {
    UTILS.activateNavigationLink('classroomLink');
    UTILS.activateMenuNavigationLink('menu-class-request');
    pendingRequestHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var pendingRequestHandler = (function () {
    var $classroom = $('#txtClassroom')
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $searchBtn = $('#searchClassroom')
    var $clearSearchBtn = $('#clearSearchClassroom')
    var $alert = $('#divRequestMessage')

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
            "url": "LoadPendingRequestData",
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
            { "data": "Department", "name": "Department", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [1], render: function (a, b, data, d) {
                return '<span>' + data["FirstName"]+' '+data["LastName"]+'</span>';
            }
        },{
            // render action buttons in the last column
            targets: [5], render: function (a, b, data, d) {
                return '<button type="button"id="accept-req-' + data["Course"] + '" class="btn btn-sm btn-dark mb-1" onclick="pendingRequestHandler.acceptRequest(' + data["UserID"] + ',this)"><i class="fa fa-fw fa-check"></i><span> Accept</span></button> '
                    + '<button type="button" id="rej-req-' + data["Course"] + '" class="btn btn-sm btn-dark mb-1" onclick="pendingRequestHandler.rejectRequest(' + data["UserID"] + ',this)"><i class="fa fa-fw fa-times"></i><span> Reject</span></button> '

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

    //function to accept request
    function acceptRequest(learner, btn) {
        if (confirm("Are you sure you want to accept the request?")) {
            var classId = btn.id.split('-').pop();
            var classTable = $('#orgClassroomList').DataTable();
            var url = hdnBaseUrl + "Classroom/AcceptClassroomRequest";
            var data = {
                Course: classId,
                UserID: learner
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', "Request accepted successfully");
                    classTable.row($(btn).parents('tr')).remove().draw();
                }

                else {
                    UTILS.Alert.show($alert, 'error', "Failed to accept request.");
                }
            }, function (err) {
                console.log(err);
                UTILS.Alert.show($alert, 'error', "Something went wrong. Please try again later.");
            });
        }
    }

    //function to reject request
    function rejectRequest(learner, btn) {
        if (confirm("Are you sure you want to reject the request?")) {
            var classId = btn.id.split('-').pop();
            var classTable = $('#orgClassroomList').DataTable();
            var url = hdnBaseUrl + "Classroom/RejectClassroomRequest";
            var data = {
                Course: classId,
                UserID: learner
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', "Request rejected successfully");
                    classTable.row($(btn).parents('tr')).remove().draw();
                }

                else {
                    UTILS.Alert.show($alert, 'error', "Failed to reject request.");
                }
            }, function (err) {
                console.log(err);
                UTILS.Alert.show($alert, 'error', "Something went wrong. Please try again later.");
            });
        }
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
        acceptRequest: acceptRequest,
        rejectRequest: rejectRequest,
        init: init
    }
})();