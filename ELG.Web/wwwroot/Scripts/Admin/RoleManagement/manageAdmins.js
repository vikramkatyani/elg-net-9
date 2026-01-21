$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-view-admin');
    adminReportHandler.showDefaultMessage();
    //assignAdminHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var adminReportHandler = (function () {
    var $admin = $('#txtAdmin')
    var $rights = $('#ddlAdminRights')
    var $searchBtn = $('#searchAdmin')
    var $clearSearchBtn = $('#clearSearchAdmin')
    var $downloadBtn = $('#downloadAdmin')
    var $assignAdminBtn = $('#btnShowUserPopUp')
    var $alert = $('#errorRemoveRightMessage');
    var $reportContainer = $('#adminReportContainer')

    function showDefaultMessage() {
        $reportContainer.hide();
        $('#registeredAdminLearnerList').DataTable().destroy();
        var message = '<div > <b>How to use this page:</b> <ul>' +
            '<li>Use the input field to search for an admin by name or email.</li>' +
            '<li>Select an admin rights category from the dropdown to filter results.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> Search to view matching admins.</li>' +
            '<li>Click <i class="fa fa-download me-1"></i> Download to export the displayed admin list to Excel.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li>' +
            '<li>Click <i class="fa fa-plus-circle me-1"></i> Add Admin to assign rights to a new user.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alert, 'default', message);
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $reportContainer.show();
        $('#registeredAdminLearnerList').DataTable().destroy();
        $('#registeredAdminLearnerList').DataTable({
            lengthChange: false,
            "processing": true,
            "language": {
                processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> '
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadAdminData",
                "type": "POST",
                "datatype": "json",
                "async": true,
                "data": function (data) {
                    data.SearchText = $admin.val();
                    data.AdminLevel = $rights.val();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "createdRow": function (row, data, dataIndex) {
                if (data["IsDeactive"] == true) {
                    $(row).addClass('table-danger');
                }
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "AdminLevelName", "name": "al.intAdminLevelID", "autoWidth": true }
            ],
            columnDefs: [{
                targets: [0], render(a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            },
            {
                targets: [3], // Adjust if needed
                orderable: false,
                searchable: false,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    const userId = data["UserID"];
                    const adminLevel = data["AdminLevel"];

                    let btn = `
        <div class="d-flex justify-content-center align-items-center">
            <div class="dropdown">
                <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                        id="adminActionDropdown-${userId}" data-bs-toggle="dropdown" aria-expanded="false"
                        style="width: 2.5rem; height: 2.5rem;">
                    <i class="fa fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="adminActionDropdown-${userId}">`;

                    // Show rights based on AdminLevel
                    if (adminLevel === 2) {
                        btn += `
                <li>
                    <a class="dropdown-item" href="#" id="show-dep-admin-${userId}"
                       onclick="manageDepartmentAdminHandler.showDepRights(this)">
                        <i class="fa fa-fw fa-eye me-2"></i>Show
                    </a>
                </li>`;
                    } else if (adminLevel === 3) {
                        btn += `
                <li>
                    <a class="dropdown-item" href="#" id="show-loc-admin-${userId}"
                       onclick="manageLocationAdminHandler.showLocRights(this)">
                        <i class="fa fa-fw fa-eye me-2"></i>Show
                    </a>
                </li>`;
                    } else if (adminLevel === 8) {
                        btn += `
                <li>
                    <a class="dropdown-item" href="#" id="show-loc-supervisor-${userId}"
                       onclick="manageLocationSupervisorHandler.showLocSupervisorRights(this)">
                        <i class="fa fa-fw fa-eye me-2"></i>Show
                    </a>
                </li>`;
                    } else if (adminLevel === 9) {
                        btn += `
                <li>
                    <a class="dropdown-item" href="#" id="show-dep-supervisor-${userId}"
                       onclick="manageDepartmentSupervisorHandler.showDepSupervisorRights(this)">
                        <i class="fa fa-fw fa-eye me-2"></i>Show
                    </a>
                </li>`;
                    }

                    // Remove rights
                    btn += `
                <li>
                    <a class="dropdown-item" href="#" id="remove-admin-${userId}"
                       onclick="adminReportHandler.removeRights(${adminLevel}, ${userId})">
                        <i class="fa fa-fw fa-trash me-2"></i>Remove
                    </a>
                </li>
                </ul>
            </div>
        </div>`;

                    return btn;
                }
            }]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $admin.val('');
        $rights.val('0');
        showDefaultMessage();

    });


    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText : $admin.val(),
            AdminLevel : $rights.val()
        }

        var path = 'DownloadAdmins?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    // show user list to assign admin rights
    $assignAdminBtn.click(function () {
        assignAdminHandler.showUserList();
    });

    // remove global admin rights for a user
    function removeRights(adminLevel, learner) {
        if (confirm("Are you sure you want to remove admin rights for this user?")) {
            var url = hdnBaseUrl + "RoleManagement/RemoveGlobalAdminRights";
            var data = {
                learner: learner,
                adminLevel: adminLevel
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, "success", "Admin right removed successfully");
                    $('#registeredAdminLearnerList').DataTable().draw();
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to remove admin rights. Please try again later");
                }
            })
        }
    }

    return {
        removeRights: removeRights,
        showDefaultMessage: showDefaultMessage
    }
})();


var manageLocationAdminHandler = (function () {

    var $loc_modal = $('#listUserLocationAdminRightsModal');
    var $alert = $('#errorLocAdminRights');
    var learner = 0;

    //populate location list with admin rights
    var locationTable = $('#listUserLocationAdminRights').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllLocationWithAdminRights",
            "type": "POST",
            "datatype": "json",
            "async": true,
            "data": function (data) {
                data.learner = learner;
            },
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
            // render action buttons in the last column
            targets: [1], render: function (a, b, data, d) {
                return '<button type="button" id="remove-la-' + data["LocationID"] + '" class="btn btn-sm btn-dark mb-1"  onclick="manageLocationAdminHandler.removeLocAdmin(this,' + learner + ')"><i class="fa fa-fw fa-trash"></i><span> Remove</span></button> '
            }
        }]
    });

    function showLocRights($btn) {
        learner = $btn.id.split('-').pop(); 
        $loc_modal.modal('show');
        locationTable.draw();
    }

    function removeLocAdmin($btn, learner) {
        $alert.hide();
        var location = $btn.id.split('-').pop();
        var url = hdnBaseUrl + "RoleManagement/RemoveLocationAdminRights";
        var data = {
            learner: learner,
            location: location
        }
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success == 1) {
                UTILS.Alert.show($alert, "success", "Admin right removed successfully");
                locationTable.draw();
            } else {
                UTILS.Alert.show($alert, "error", "Failed to remove admin rights. Please try again later");
            }
        })
    }

    return {
        showLocRights: showLocRights,
        removeLocAdmin: removeLocAdmin
    }
})();

var manageDepartmentAdminHandler = (function () {
    var learner = 0;
    var $modal = $('#listUserDepartmentAdminRightsModal');
    var $alert = $('#errorDepAdminRights');

    // populate department table
    var departmentTable = $('#listUserDepartmentAdminRights').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllDepartmentWithAdminRights",
            "type": "POST",
            "datatype": "json",
            "async": true,
            "data": function (data) {
                data.learner = learner;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "DepartmentName", "autoWidth": true },
            { "data": "LocationName", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                return '<button type="button" id="remove-da-' + data["DepartmentID"] + '" class="btn btn-sm btn-dark mb-1" onclick="manageDepartmentAdminHandler.removeDepAdmin(this,' + learner + ',' + data["LocationID"] + ')"><i class="fa fa-fw fa-trash"></i><span> Remove</span></button> '
            }
        }]
    });

    function showDepRights($btn) {
        $alert.hide();
        learner = $btn.id.split('-').pop(); 
        $modal.modal('show');
        departmentTable.draw();
    }

    function removeDepAdmin($btn, learner, location) {
        var department = $btn.id.split('-').pop();
        var url = hdnBaseUrl + "RoleManagement/RemoveDepartmentAdminRights";
        var data = {
            learner: learner,
            location: location,
            department: department
        }
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success == 1) {
                UTILS.Alert.show($alert, "success", "Admin right removed successfully");
                departmentTable.draw();
            } else {
                UTILS.Alert.show($alert, "error", "Failed to remove admin rights. Please try again later");
            }
        })
    }

    return{
        showDepRights: showDepRights,
        removeDepAdmin: removeDepAdmin
    }

})();

var manageLocationSupervisorHandler = (function () {

    var $loc_modal = $('#listUserLocationSupervisorRightsModal');
    var $alert = $('#errorLocSupervisorRights');
    var learner = 0;

    //populate location list with admin rights
    var locationTable = $('#listUserLocationSupervisorRights').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllLocationWithSupervisorRights",
            "type": "POST",
            "datatype": "json",
            "async": true,
            "data": function (data) {
                data.learner = learner;
            },
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
            // render action buttons in the last column
            targets: [1], render: function (a, b, data, d) {
                return '<button type="button" id="remove-ls-' + data["LocationID"] + '" class="btn btn-sm btn-dark mb-1"  onclick="manageLocationSupervisorHandler.removeLocSupervisor(this,' + learner + ')"><i class="fa fa-fw fa-trash"></i><span> Remove</span></button> '
            }
        }]
    });

    function showLocSupervisorRights($btn) {
        learner = $btn.id.split('-').pop();
        $loc_modal.modal('show');
        locationTable.draw();
    }

    function removeLocSupervisor($btn, learner) {
        $alert.hide();
        var location = $btn.id.split('-').pop();
        var url = hdnBaseUrl + "RoleManagement/RemoveLocationSupervisorRights";
        var data = {
            learner: learner,
            location: location
        }
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success == 1) {
                UTILS.Alert.show($alert, "success", "Supervisor right removed successfully");
                locationTable.draw();
            } else {
                UTILS.Alert.show($alert, "error", "Failed to remove supervisor rights. Please try again later");
            }
        })
    }

    return {
        showLocSupervisorRights: showLocSupervisorRights,
        removeLocSupervisor: removeLocSupervisor
    }
})();

var manageDepartmentSupervisorHandler = (function () {
    var learner = 0;
    var $modal = $('#listUserDepartmentSupervisorRightsModal');
    var $alert = $('#errorDepSupervisorRights');

    // populate department table
    var departmentTable = $('#listUserDepartmentSupervisorRights').DataTable({
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
            "url": hdnBaseUrl + "RoleManagement/LoadAllDepartmentWithSupervisorRights",
            "type": "POST",
            "datatype": "json",
            "async": true,
            "data": function (data) {
                data.learner = learner;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "DepartmentName", "autoWidth": true },
            { "data": "LocationName", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                return '<button type="button" id="remove-ds-' + data["DepartmentID"] + '" class="btn btn-sm btn-dark mb-1" onclick="manageDepartmentSupervisorHandler.removeDepSupervisor(this,' + learner + ',' + data["LocationID"] + ')"><i class="fa fa-fw fa-trash"></i><span> Remove</span></button> '
            }
        }]
    });

    function showDepSupervisorRights($btn) {
        $alert.hide();
        learner = $btn.id.split('-').pop();
        $modal.modal('show');
        departmentTable.draw();
    }

    function removeDepSupervisor($btn, learner, location) {
        var department = $btn.id.split('-').pop();
        var url = hdnBaseUrl + "RoleManagement/RemoveDepartmentSUpervisorRights";
        var data = {
            learner: learner,
            location: location,
            department: department
        }
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success == 1) {
                UTILS.Alert.show($alert, "success", "Supervisor right removed successfully");
                departmentTable.draw();
            } else {
                UTILS.Alert.show($alert, "error", "Failed to remove supervisor rights. Please try again later");
            }
        })
    }

    return {
        showDepSupervisorRights: showDepSupervisorRights,
        removeDepSupervisor: removeDepSupervisor
    }

})();

var assignAdminHandler = (function () {

    var $modal = $('#learnerListModal');
    var $learner = $('#learnerListModal #txtAdminLearner')
    var $location = $('#learnerListModal #ddlAdminLocation')
    var $department = $('#learnerListModal #ddlAdminDepartment')
    var $searchBtn = $('#learnerListModal #searchAdminLearner')
    var $clearSearchBtn = $('#learnerListModal #clearSearchAdminLearner')
    var $alertMessage = $('#learnerListModal #admin-learner-message');
    var $reportContainer = $('#learnerListModal #learnerListForAdmins');

    var $ddlLoc = $('.ddl-loc');
    var $ddlDep = $('.ddl-dep');

    //function to show pop up
    function showUserList() {
        showDefaultMessage();
        $modal.modal('show');
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alertMessage);
        $reportContainer.show();
        $('#learnerListForAdminRights').DataTable().destroy();
        $('#learnerListForAdminRights').DataTable({
            lengthChange: false,
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": true,
            "ajax": {
                "url": hdnBaseUrl + "UserManagement/LoadLearnerData",
                "type": "POST",
                "async": false,
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val();
                    data.SearchLocation = $location.val();
                    data.SearchDepartment = $department.val();
                    data.SearchStatus = 1;
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "createdRow": function (row, data, dataIndex) {
                if (data["IsDeactive"] == true) {
                    $(row).addClass('table-danger');
                }
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "Location", "name": "l.strLocation", "autoWidth": true },
                { "data": "Department", "name": "l.strDepartment", "autoWidth": true }
            ],
            columnDefs: [{
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>';
                }
            }, {
                    targets: [4], // Adjust if your "Action" column index is different
                    orderable: false,
                    searchable: false,
                    className: "text-center align-middle", // Center the ellipsis icon
                    render: function (a, b, data, d) {
                        const userId = data["UserID"];

                        return `
        <div class="d-flex justify-content-center align-items-center">
            <div class="dropdown">
                <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                        id="actionDropdown-${userId}" data-bs-toggle="dropdown" aria-expanded="false"
                        style="width: 2.5rem; height: 2.5rem;">
                    <i class="fa fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${userId}">
                    <li>
                        <a class="dropdown-item" href="#" id="assign-user-${userId}"
                           onclick="assignAdminHandler.assignRights(this)">
                            <i class="fa fa-fw fa-plus-circle me-2"></i>Assign
                        </a>
                    </li>
                </ul>
            </div>
        </div>`;
                    }
                }]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $location.val('0');
        $department.val('0');
        showDefaultMessage();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //function to initialize the view, bind all drop downs
    function init() {

        showDefaultMessage();
        renderLocationDropDown();
    }

    function showDefaultMessage() {
        $reportContainer.hide();
        var message = '<div > <b>How to use this popup:</b> <ul>' +
            '<li>Enter a name or email in the <b>User</b> field to search for specific learners.</li>' +
            '<li>Use the <b>Location</b> and <b>Department</b> dropdowns to narrow down the list.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to display matching users.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> <b>Clear</b> to reset all filters and start over.</li>' +
            '<li>Use the <b>Action</b> column to assign admin rights to the selected user.</li>' +
            '</ul></div>';
        $('#learnerListForAdminRights').DataTable().destroy();
        UTILS.Alert.show($alertMessage, 'default', message);
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

    //function to redirect to assign rights page
    function assignRights(btn) {
        if (!btn || !btn.id) return;
        const parts = btn.id.split('-');
        const learnerId = parts[parts.length - 1].trim();
        if (!learnerId) return;

        const base = (typeof hdnBaseUrl === 'string') ? hdnBaseUrl.replace(/\/+$/, '') : '';
        const url = `${base}/RoleManagement/AssignAdminRights/${encodeURIComponent(learnerId)}`;
        window.location.href = url;
    }

    return {
        init: init,
        renderLocationDropDown: renderLocationDropDown,
        showUserList: showUserList,
        assignRights: assignRights
    }
})();


