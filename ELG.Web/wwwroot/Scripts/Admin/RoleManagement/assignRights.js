$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-view-admin');
    rightsAllocationHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});
document.addEventListener('DOMContentLoaded', function () {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
var rightsAllocationHandler = (function () {

    var $adminRightBtn = $('#btnAssignAdminGod');
    var $reportRightBtn = $('#btnAssignReportAdmin');
    var $userRightBtn = $('#btnAssignUserAdmin');
    var $showLocationBtn = $('#btnShowLocations');
    var $showDepartmentBtn = $('#btnShowDepartments');
    var $showLocationBtn_spv = $('#btnShowLocations_spv');
    var $showDepartmentBtn_spv = $('#btnShowDepartments_spv');
    var $alert = $('#divAdminRightErrorMessage');
    var $popUp_da_Alert = $('#divDepartmentAdminRightErrorMessage');
    var $popUp_la_Alert = $('#divLocationAdminRightErrorMessage');
    var $popUp_da_spv_Alert = $('#divDepartmentSupervisorRightErrorMessage');
    var $popUp_la_spv_Alert = $('#divLocationSupervisorRightErrorMessage');

    var $locationModal = $("#locationListForAdminRightsModal");
    var $departmentModal = $("#departmentListForAdminRightsModal");
    var $locationModal_spv = $("#locationListForSupervisorRightsModal");
    var $departmentModal_spv = $("#departmentListForSupervisorRightsModal");
    var $ddlLoc = $("#ddlAssignAdminLocation");
    var $ddlLoc_spv = $("#ddlAssignSupervisorLocation");

    //populate location list with admin rights
    var locationTable = $('#assignAdminToLocationList').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllLocationToAssignAdminRights",
            "type": "POST",
            "datatype": "json",
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "LocationName", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [1],
            render: function (a, b, data, d) {
                const id = data["LocationID"];
                const hasRights = data["HasRights"] === true;

                if (hasRights) {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-success w-100 mb-1"
                        disabled
                        data-bs-toggle="tooltip"
                        title="Admin rights already assigned">
                    <i class="fa fa-check-circle me-1"></i> Assigned
                </button>`;
                } else {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-dark w-100 mb-1"
                        onclick="rightsAllocationHandler.assignLocationAdminRights(this)"
                        data-bs-toggle="tooltip"
                        title="Assign admin rights to this location">
                    <i class="fa fa-plus-circle me-1"></i> Assign
                </button>`;
                }
            }
        }]
    });

    //populate location list with supervisor rights
    var locationTable_spv = $('#assignSupervisorToLocationList').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllLocationToAssignSupervisorRights",
            "type": "POST",
            "datatype": "json",
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "LocationName", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [1],
            render: function (a, b, data, d) {
                const id = data["LocationID"];
                const hasRights = data["HasRights"] === true;

                if (hasRights) {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-success mb-1 w-100"
                        disabled
                        data-bs-toggle="tooltip"
                        title="Already assigned">
                    <i class="fa fa-check-circle me-1"></i> Assigned
                </button>`;
                } else {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-dark mb-1 w-100"
                        onclick="rightsAllocationHandler.assignLocationSupervisorRights(this)"
                        data-bs-toggle="tooltip"
                        title="Assign rights to this location">
                    <i class="fa fa-plus-circle me-1"></i> Assign
                </button>`;
                }
            }
        }]
    });

    // populate department table
    var departmentTable = $('#assignAdminToDepartmentList').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllDepartmentToAssignAdminRights",
            "type": "POST",
            "async": true,
            "datatype": "json",
            "data": function (data) {
                data.location = $ddlLoc.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "DepartmentName", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [1],
            render: function (a, b, data, d) {
                const id = data["DepartmentID"];
                const hasRights = data["HasRights"] === true;

                if (hasRights) {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-success w-100 mb-1"
                        disabled
                        data-bs-toggle="tooltip"
                        title="Admin rights already assigned">
                    <i class="fa fa-check-circle me-1"></i> Assigned
                </button>`;
                } else {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-dark w-100 mb-1"
                        onclick="rightsAllocationHandler.assignDepartmentAdminRights(this)"
                        data-bs-toggle="tooltip"
                        title="Assign admin rights to this department">
                    <i class="fa fa-plus-circle me-1"></i> Assign
                </button>`;
                }
            }
        }]
    });

    // populate department table
    var departmentTable_spv = $('#assignSupervisorToDepartmentList').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllDepartmentToAssignSupervisorRights",
            "type": "POST",
            "async": true,
            "datatype": "json",
            "data": function (data) {
                data.location = $ddlLoc_spv.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "DepartmentName", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [1],
            render: function (a, b, data, d) {
                const id = data["DepartmentID"];
                const hasRights = data["HasRights"] === true;

                if (hasRights) {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-success w-100 mb-1"
                        disabled
                        data-bs-toggle="tooltip"
                        title="Supervisor rights already assigned">
                    <i class="fa fa-check-circle me-1"></i> Assigned
                </button>`;
                } else {
                    return `
                <button type="button"
                        id="assign-la-${id}"
                        class="btn btn-dark w-100 mb-1"
                        onclick="rightsAllocationHandler.assignDepartmentSupervisorRights(this)"
                        data-bs-toggle="tooltip"
                        title="Assign supervisor rights to this department">
                    <i class="fa fa-plus-circle me-1"></i> Assign
                </button>`;
                }
            }
        }]
    });

    //function to initialize the view, bind all drop downs
    function init() {
        renderLocationDropDown();
        renderLocationDropDown_spv();
    }

    // populate department drop down on location change
    $ddlLoc.change(function () {
        departmentTable.draw();
    });

    // populate department drop down on location change
    $ddlLoc_spv.change(function () {
        departmentTable_spv.draw();
    });

    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $ddlLoc.empty();
        $ddlLoc.append($('<option/>', { value: '0', text: 'Select' }));

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

    //function to render list of all locations in organisation
    function renderLocationDropDown_spv() {
        $ddlLoc_spv.empty();
        $ddlLoc_spv.append($('<option/>', { value: '0', text: 'Select' }));

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                $.each(data.locationList, function (index, item) {
                    $ddlLoc_spv.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }
    function getRoleData($btn) {
        const $row = $btn.closest('tr');
        const $roleCell = $row.find('td').first();
        const roleTitle = $roleCell.find('strong').text().trim();
        const roleDescription = $roleCell.contents().filter(function () {
            return this.nodeType === 3; // grabs the text node after <strong>
        }).text().trim();

        return { title: roleTitle, description: roleDescription };
    }

    function getConfirmationText(title, description) {
        return `Are you sure you want to assign ${title} – ${description} to this user?`;
    }

    // assign Super Admin to a learner
    $adminRightBtn.click(function () {
        const roleData = getRoleData($(this));
        const message = getConfirmationText(roleData.title, roleData.description);

        if (confirm(message)) {

            var url = hdnBaseUrl + "RoleManagement/AssignAdminRights";
            var data = {
                adminRight : 1
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, "success", "Admin right assigned successfully");
                } else if (res.success == 2) {
                    UTILS.Alert.show($alert, "warning", "User already has Super Admin rights");
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to assign admin rights. Please try again later");
                }
            })
        }
        return;
    });

    // assign report admin rights to a learner
    $reportRightBtn.click(function () {
        if (confirm("Are you sure you want to assign Report Admin rights to this user?")) {
            var url = hdnBaseUrl + "RoleManagement/AssignAdminRights";
            var data = {
                adminRight: 4
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, "success", "Admin right assigned successfully");
                } else if (res.success == 2) {
                    UTILS.Alert.show($alert, "warning", "User already has admin rights");
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to assign admin rights. Please try again later");
                }
            })
        }
        return;
    });

    // assign user admin rights to a learner
    $userRightBtn.click(function () {
        if (confirm("Are you sure you want to assign User Admin rights to this user?")) {
            var url = hdnBaseUrl + "/RoleManagement/AssignAdminRights/";
            var data = {
                adminRight: 5
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, "success", "Admin right assigned successfully");
                } else if (res.success == 2) {
                    UTILS.Alert.show($alert, "warning", "User already has admin rights");
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to assign admin rights. Please try again later");
                }
            })
        }
        return;
    });

    let currentRole = null;

    $showLocationBtn.click(function () {
        const roleData = getRoleData($(this)); 
        currentRole = roleData; 
        locationTable.draw();
        $locationModal.modal('show');
    });

    // assign department admin rights to a learner
    $showDepartmentBtn.click(function () {
        const roleData = getRoleData($(this));
        currentRole = roleData; 
        departmentTable.draw();
        $departmentModal.modal('show');
    });

    // assign location supervisior rights to a learner
    $showLocationBtn_spv.click(function () {
        const roleData = getRoleData($(this));
        currentRole = roleData; 
        locationTable_spv.draw();
        $locationModal_spv.modal('show');
    });

    // assign department supervisior rights to a learner
    $showDepartmentBtn_spv.click(function () {
        const roleData = getRoleData($(this));
        currentRole = roleData; 
        departmentTable_spv.draw();
        $departmentModal_spv.modal('show');
    });

    //function to assign location admin rights to current user
    function assignLocationAdminRights($btn) {
        const message = getConfirmationText(currentRole.title, currentRole.description);
        if (confirm(message)) {

            var location = $btn.id.split('-').pop();
            var url = hdnBaseUrl + "RoleManagement/AssignLocationAdminRights";
            var data = {
                location: location
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($popUp_la_Alert, "success", "Role assigned successfully");
                    locationTable.draw();
                }  else {
                    UTILS.Alert.show($popUp_la_Alert, "error", "Failed to assign role. Please try again later");
                }
            });
        }
    }

    //function to assign location Supervisor rights to current user
    function assignLocationSupervisorRights($btn) {
        const message = getConfirmationText(currentRole.title, currentRole.description);
        if (confirm(message)) {

            var location = $btn.id.split('-').pop();
            var url = hdnBaseUrl + "RoleManagement/AssignLocationSupervisorRights";
            var data = {
                location: location
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($popUp_la_spv_Alert, "success", "Role assigned successfully");
                    locationTable_spv.draw();
                }  else {
                    UTILS.Alert.show($popUp_la_spv_Alert, "error", "Failed to assign role. Please try again later");
                }
            });
        }
    }

    //function to assign department admin rights to current user
    function assignDepartmentAdminRights($btn) {
        const message = getConfirmationText(currentRole.title, currentRole.description);
        if (confirm(message)) {

            var location = $ddlLoc.val();
            var department = $btn.id.split('-').pop();
            var url = hdnBaseUrl + "RoleManagement/AssignDepartmentAdminRights";
            var data = {
                location: location,
                department: department
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($popUp_da_Alert, "success", "Role right assigned successfully");
                    departmentTable.draw();
                } else {
                    UTILS.Alert.show($popUp_da_Alert, "error", "Failed to assign role. Please try again later");
                }
            });
        }
    }

    //function to assign department supervisor rights to current user
    function assignDepartmentSupervisorRights($btn) {
        const message = getConfirmationText(currentRole.title, currentRole.description);
        if (confirm(message)) {

            var location = $ddlLoc_spv.val();
            var department = $btn.id.split('-').pop();
            var url = hdnBaseUrl + "RoleManagement/AssignDepartmentSupervisorRights";
            var data = {
                location: location,
                department: department
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($popUp_da_spv_Alert, "success", "Role assigned successfully");
                    departmentTable_spv.draw();
                } else {
                    UTILS.Alert.show($popUp_da_spv_Alert, "error", "Failed to assign role. Please try again later");
                }
            });
        }
    }
    return {
        init: init,
        assignLocationAdminRights: assignLocationAdminRights,
        assignLocationSupervisorRights: assignLocationSupervisorRights,
        assignDepartmentAdminRights: assignDepartmentAdminRights,
        assignDepartmentSupervisorRights: assignDepartmentSupervisorRights
    }

})();