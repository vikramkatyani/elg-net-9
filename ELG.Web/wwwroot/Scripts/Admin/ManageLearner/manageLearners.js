$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-view-user');
    learnerReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var learnerReportHandler = (function () {
    var $learnerTableContainer = $('#learner-table-container')
    var $learner = $('#txtLearner')
    var $location = $('#ddlLocation')
    var $department = $('#ddlDepartment')
    var $status = $('#ddlStatus')
    var $searchBtn = $('#searchLearner')
    var $clearSearchBtn = $('#clearSearchLearner')
    var $downloadBtn = $('#downloadLearnerList');
    var $alert = $('#message_learner_list');
    var $toggleAdvancedBtn = $('#toggleAdvancedFilters');
    var $advancedLocFilter = $('#advancedLocationFilter');
    var $advancedDepFilter = $('#advancedDepartmentFilter');
    var $advancedFilterIcon = $('#advancedFilterIcon');

    var $pillTotalUsers = $('#pillTotalUsers');
    var $pillActiveUsers = $('#pillActiveUsers');
    var $pillInactiveUsers = $('#pillInactiveUsers');
    var $statusPills = $('.um-stat-pill[data-status]');

    var $ddlLoc = $('.ddl-loc');
    var $ddlDep = $('.ddl-dep');

    var adminPrev = 0;
    var ssoOrg = 0;
    var selectedStatus = 0;

    //var learnerTable = 

    //apply filters for search
    $searchBtn.on('click',function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $learnerTableContainer.show();
        //learnerTable.draw();
        $('#registeredLearnerList').DataTable().destroy();
        $('#registeredLearnerList').DataTable({
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
                "url": "LoadLearnerData",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val().replace(/'/g, "''''");
                    data.SearchLocation = $location.val();
                    data.SearchDepartment = $department.val();
                    data.SearchStatus = getSearchStatus();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "drawCallback": function (respData) {
                adminPrev = respData.json.adminPrev;
                ssoOrg = respData.json.ssoOrg;

                if (adminPrev == 8 || adminPrev == 9)
                    $('.dlt-usr-btn').remove();

                if (ssoOrg)
                    $('.sso-btns').remove();
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "Location", "name": "l.strLocation", "autoWidth": true },
                { "data": "Department", "name": "d.strDepartment", "autoWidth": true },
                { "data": "Status", "name": "c.blnCancelled", "autoWidth": true }
            ],
            columnDefs: [{
                // render action buttons in the last column
                targets: [0], render: function (a, b, data, d) {
                    return '<span class="spn-al-uname">' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                // render email text
                targets: [1],
                render: function (a, b, data, d) {
                    return '<span class="spn-al-uemail">' + data["EmailId"] + '</span>'
                }
                }, {
                    // render status as badge chip instead of row background color
                    targets: [4],
                    render: function (a, b, data, d) {
                        var isInactive = data["IsDeactive"] === true;
                        var statusText = isInactive ? 'Suspended' : 'Active';
                        var statusClass = isInactive ? 'um-status-badge um-status-suspended' : 'um-status-badge um-status-active';
                        return '<span class="' + statusClass + '">' + statusText + '</span>';
                    }
                }, {
                    targets: [5], // Adjust if your "Action" column index is different
                    orderable: false,
                    searchable: false,
                    className: "text-center align-middle", // Ensures vertical and horizontal centering
                    render: function (a, b, data, d) {
                        const userId = data["UserID"];
                        const isDeactive = data["IsDeactive"];

                        let btn = `
        <div class="d-flex justify-content-center align-items-center">
            <div class="dropdown um-row-actions">
                <button class="btn border-0 um-row-actions-trigger" type="button"
                        id="dropdownMenu-${userId}" data-bs-toggle="dropdown" aria-expanded="false"
                        >
                    <i class="fa fa-ellipsis-h"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end um-row-actions-menu" aria-labelledby="dropdownMenu-${userId}">`;

                        btn += `
            <a class="dropdown-item um-row-actions-item" href="#" id="edit-user-${userId}" onclick="updateLearnerHandler.updateLearner(this)">
                <i class="fa fa-fw fa-edit um-row-actions-item-icon"></i><span>Edit</span>
            </a>`;

                        if (isDeactive) {
                            btn += `
            <a class="dropdown-item um-row-actions-item" href="#" id="update-user-${userId}" onclick="learnerReportHandler.updateActiveStatus(this, false)">
                <i class="fa fa-fw fa-refresh um-row-actions-item-icon"></i><span>Activate</span>
            </a>
            <a class="dropdown-item um-row-actions-item dlt-usr-btn" href="#" id="delete-user-${userId}" onclick="learnerReportHandler.deleteLearner(this)"
               data-loading-text="Deleting..." data-button-icon="trash" data-original-text="Delete">
                <i class="fa fa-fw fa-trash um-row-actions-item-icon"></i><span>Delete</span>
            </a>
            <a class="dropdown-item um-row-actions-item sso-btns disabled" href="#" id="reset-pswd-${userId}">
                <i class="fa fa-fw fa-refresh um-row-actions-item-icon"></i><span>Reset Password</span>
            </a>
            <a class="dropdown-item um-row-actions-item sso-btns disabled" href="#" id="resend-mail-${userId}">
                <i class="fa fa-fw fa-paper-plane um-row-actions-item-icon"></i><span>Resend Activation Email</span>
            </a>`;
                        } else {
                            btn += `
            <a class="dropdown-item um-row-actions-item" href="#" id="update-user-${userId}" onclick="learnerReportHandler.updateActiveStatus(this, true)">
                <i class="fa fa-fw fa-power-off um-row-actions-item-icon"></i><span>Deactivate</span>
            </a>
            <a class="dropdown-item um-row-actions-item dlt-usr-btn" href="#" id="delete-user-${userId}" onclick="learnerReportHandler.deleteLearner(this)"
               data-loading-text="Deleting..." data-button-icon="trash" data-original-text="Delete">
                <i class="fa fa-fw fa-trash um-row-actions-item-icon"></i><span>Delete</span>
            </a>
            <a class="dropdown-item um-row-actions-item sso-btns" href="#" id="reset-pswd-${userId}" onclick="learnerReportHandler.resetLearnerPassword(this)">
                <i class="fa fa-fw fa-refresh um-row-actions-item-icon"></i><span>Reset Password</span>
            </a>
            <a class="dropdown-item um-row-actions-item sso-btns" href="#" id="resend-mail-${userId}" onclick="learnerReportHandler.resendLearnerActivationEmail(this)">
                <i class="fa fa-fw fa-paper-plane um-row-actions-item-icon"></i><span>Resend Activation Email</span>
            </a>`;
                        }

                        btn += `
                </ul>
            </div>
        </div>`;
            //<a class="dropdown-item" href="#" id="training-report-${userId}" onclick="learnerReportHandler.showTrainingReport(this)">
            //    <i class="fa fa-fw fa-list me-2"></i>Training Card
            //</a>

                        return btn;
                    }
                }],
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $location.val('0');
        $department.val('0');
        selectedStatus = 0;
        if ($status.length)
            $status.val('0');

        setActiveStatusPill(0);
        //learnerTable.draw();
        $('#registeredLearnerList').DataTable().draw();
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $learner.val(),
            Location: $location.val(),
            Department: $department.val(),
            Status: getSearchStatus()
        }

        var path = 'DownloadLearners?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //function to initialize the view, bind all drop downs
    function init() {
        $learnerTableContainer.hide();
        var message = '<div > <b>How to use this page:</b> <ul>  <li>Select filter criteria from the dropdowns or input fields.</li>  <li>Click <i class="fa fa-search me-1"></i> Search to display the matching users.</li> <li>Click <i class="fa fa-download me-1"></i> Download to export the displayed records to Excel.</li> <li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start over.</li> </ul></div>';
        UTILS.Alert.show($alert, 'default', message);
        renderLocationDropDown();
        initModernFilterToggle();
        initStatusPills();
        loadQuickStats();
    }

    function getSearchStatus() {
        if ($status.length)
            return $status.val();

        return selectedStatus;
    }

    function setActiveStatusPill(status) {
        if (!$statusPills.length)
            return;

        $statusPills.removeClass('is-selected');
        $statusPills.filter('[data-status="' + status + '"]').addClass('is-selected');
    }

    function initStatusPills() {
        if (!$statusPills.length)
            return;

        setActiveStatusPill(selectedStatus);

        $statusPills.off('click keydown').on('click keydown', function (e) {
            if (e.type === 'keydown' && e.key !== 'Enter' && e.key !== ' ')
                return;

            if (e.type === 'keydown')
                e.preventDefault();

            selectedStatus = Number($(this).data('status')) || 0;
            if ($status.length)
                $status.val(String(selectedStatus));

            setActiveStatusPill(selectedStatus);
            $searchBtn.trigger('click');
        });
    }

    function initModernFilterToggle() {
        if (!$toggleAdvancedBtn.length || !$advancedLocFilter.length || !$advancedDepFilter.length)
            return;

        $toggleAdvancedBtn.off('click').on('click', function (e) {
            e.preventDefault();

            $advancedLocFilter.toggleClass('d-none');
            $advancedDepFilter.toggleClass('d-none');

            var isHidden = $advancedLocFilter.hasClass('d-none');
            if ($advancedFilterIcon.length) {
                $advancedFilterIcon.toggleClass('fa-sliders', isHidden);
                $advancedFilterIcon.toggleClass('fa-filter', !isHidden);
            }
        });
    }

    function loadQuickStats() {
        if (!$pillTotalUsers.length || !$pillActiveUsers.length || !$pillInactiveUsers.length)
            return;

        $pillTotalUsers.html('<i class="fa fa-spinner fa-spin"></i>');
        $pillActiveUsers.html('<i class="fa fa-spinner fa-spin"></i>');
        $pillInactiveUsers.html('<i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            type: 'get',
            url: 'LoadLearnerQuickStats',
            dataType: 'json',
            success: function (res) {
                if (res && res.success == 1) {
                    $pillTotalUsers.text(res.totalUsers);
                    $pillActiveUsers.text(res.activeUsers);
                    $pillInactiveUsers.text(res.inactiveUsers);
                } else {
                    $pillTotalUsers.text('-');
                    $pillActiveUsers.text('-');
                    $pillInactiveUsers.text('-');
                }
            },
            error: function () {
                $pillTotalUsers.text('-');
                $pillActiveUsers.text('-');
                $pillInactiveUsers.text('-');
            }
        });
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

     //function to delete learner and refresh table
    function resetLearnerPassword(btn) {
        if (confirm("Are you sure you want to reset password for this user?")) {
            var learnerId = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: 'ResetLearnerPassword',
                dataType: 'json',
                data: { learnerId: learnerId },
                success: function (result) {
                    if (result.success == 1)
                        alert('Success! This password is not emailed to the user, you need to inform the user to log in with: \nWeb@ELG_25')
                    else
                        alert('Failed to reset password.')
                },
                error: function (status) {
                    console.log(status);
                }
            });
        }
    }

     //function to resend activation email to learner
    function resendLearnerActivationEmail(btn) {
        var userName = $(btn).closest('tr').find('.spn-al-uname').html();
        var userEmail = $(btn).closest('tr').find('.spn-al-uemail').html();
        if (confirm("Are you sure you want to resend activation email to " + userName+" ?")) {
            var learnerId = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: 'ResendLearnerActivationEmail',
                dataType: 'json',
                data: { learnerId: learnerId },
                success: function (result) {
                    if (result.success == 1)
                        alert('Success! Activation email has been sent to ' + userName + ' at ' + userEmail);
                    else
                        alert('Failed to send activation email.')
                },
                error: function (status) {
                    console.log(status);
                }
            });
        }
    }


     //function to delete learner and refresh table
    function deleteLearner(btn) {
        if (confirm("This action will delete the user and all training records permanently.\nThis action cannot be undone, are you sure you want to delete this user?")) {
            UTILS.disableButton($(btn));
            var learnerId = btn.id.split('-').pop();
            var learnerTable = $('#registeredLearnerList').DataTable(); 
            //selector
            $.ajax({
                type: 'post',
                url: 'DeleteLearner',
                dataType: 'json',
                data: { learnerId: learnerId },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.resetButton($(btn));
                        learnerTable.row($(btn).parents('tr')).remove().draw();
                    } else if(result.success == -2) {
                        UTILS.resetButton($(btn));
                        alert('You are not allowed to delete the user.')
                    }else {
                        UTILS.resetButton($(btn));
                        alert('Failed to delete the user.')
                    }
                },
                error: function (status) {
                    console.log(status);
                }
            });
        }
    }
    //function to update active status
    function updateActiveStatus(btn, status) {
        if (confirm("Are you sure you want to change active status for this user?")) {
            var learnerId = btn.id.split('-').pop();
            var learnerTable = $('#registeredLearnerList').DataTable();
            //selector
            $.ajax({
                type: 'post',
                url: 'UpdateActiveStatus',
                dataType: 'json',
                data: { UserID: learnerId, IsDeactive: status},
                success: function (result) {
                    if (result.success == 1)
                        learnerTable.ajax.reload(null, false);
                    else
                        alert('Failed to update active status of the user.')
                },
                error: function (status) {
                    console.log(status);
                }
            });
        }
    }

    //function to show training report for a learner
    function showTrainingReport(btn) {
        var learnerTable = $('#registeredLearnerList').DataTable();
        var learnerEmail = learnerTable.row(btn.closest('tr')).data()["EmailId"];
        var learnerEmpNo = learnerTable.row(btn.closest('tr')).data()["EmployeeNumber"];
        var trainee = learnerEmail;

        if (learnerEmail == null || learnerEmail == '')
            trainee = learnerEmpNo;

        //redirecting to learner training report page
        window.location.href = hdnBaseUrl + "Report/TrainingCard?trainee=" + trainee;
    }

    return {
        init: init,
        deleteLearner: deleteLearner,
        updateActiveStatus: updateActiveStatus,
        renderLocationDropDown: renderLocationDropDown,
        resetLearnerPassword: resetLearnerPassword,
        resendLearnerActivationEmail: resendLearnerActivationEmail,
        showTrainingReport: showTrainingReport
    }
})();


var updateLearnerHandler = (function () {

    $modal = $('#updateLearnerInfoModal');
    $title = $('#updateInfoTitle')
    $userId = $('#txtUpdateUserId');
    $firstName = $('#txtUpdateFirstName');
    $lastName = $('#txtUpdateLastName');
    $email = $('#txtUpdateEmail');
    $location = $('#updateLocationDDL');
    $department = $('#updateDepartmentDDL');
    $empno = $('#txtUpdateUserId');
    $alert = $('#editLearnerError');
    $updateInfo = $('#btnUpdateLearnerInfo');

    var learnerId = 0;

    //function to update user info
    function updateLearner(btn) {
        learnerId = btn.id.split('-').pop();
        $.ajax({
            type: 'get',
            url: 'GetLearnerInfo',
            dataType: 'json',
            data: { UserID: learnerId },
            success: function (info) {
                populateData(info);
            },
            error: function (status) {
                console.log(status);
            }
        });
    }

    //to render info in the pop up
    function populateData(info) {
        UTILS.Alert.hide($alert);
        // render drop downs
        renderLocations(info.locationList);
        renderDepartments(info.departmentList);

        $title.html('Update - ' + info.learnerInfo.FirstName + ' ' + info.learnerInfo.LastName);
        $userId.val(info.learnerInfo.EmployeeNumber);
        $firstName.val(info.learnerInfo.FirstName);
        $lastName.val(info.learnerInfo.LastName);
        $email.val(info.learnerInfo.EmailId);
        $location.val(info.learnerInfo.LocationID);
        $department.val(info.learnerInfo.DepartmentID);
        $empno.val(info.learnerInfo.EmployeeNumber);
        $modal.modal('show');
    }

    function renderLocations(locationList) {
        $location.empty();
        $location.append($('<option/>', { value: '0', text: 'Select' }));
        $.each(locationList, function (index, item) {
            $location.append($('<option/>', {
                value: item.LocationId,
                text: item.LocationName
            }))
        });
    }

    function renderDepartments(departmentList) {
        $department.empty();
        $department.append($('<option/>', { value: '0', text: 'Select' }));
        $.each(departmentList, function (index, item) {
            $department.append($('<option/>', {
                value: item.DepartmentId,
                text: item.DepartmentName
            }))
        });
    }

    $updateInfo.click(function (e) {
        e.preventDefault();
        var learnerTable = $('#registeredLearnerList').DataTable();
        var learner = {
            UserID: learnerId,
            EmployeeNumber: $userId.val(),
            FirstName: $firstName.val(),
            LastName: $lastName.val(),
            EmailId: $email.val(),
            LocationID: $location.val(),
            DepartmentID: $department.val(),
            EmployeeNumber: $empno.val()
        }
        $.ajax({
            type: 'post',
            url: 'UpdateLearnerInfo',
            dataType: 'json',
            data: learner,
            success: function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'User Info updated successfully !');
                    learnerTable.ajax.reload(null, false);
                }
                else if (result.success == 2) {
                    UTILS.Alert.show($alert, 'error', 'Email address is registered with another user. Please try another valid email address');
                }
                else {
                    UTILS.Alert.show($alert, 'error','Failed to update the user.')
                }
            },
            error: function (status) {
                console.log(status);
                UTILS.Alert.show($alert, 'error', 'Something went wrong. Please try agin later');
            }
        });
    });

    return {
        updateLearner: updateLearner
    }
})();