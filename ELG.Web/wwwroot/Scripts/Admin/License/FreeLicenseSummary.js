
var manageFreeLicenseHandler = (function () {

    var $fl_modal = $('#learnerWithFreeLicenseModal');
    var $learner = $('#txt_lwfl_Learner')
    var $ddlLoc = $('#ddl_lwfl_Location')
    var $ddlDep = $('#ddl_lwfl_Department')
    var $searchBtn = $('#search_lwfl_Learner')
    var $clearsearchBtn = $('#clearSearch_lwfl_Learner')
    var $alert = $("#message_freeLicences");
    var course = 0;

    var $revokeAllBtn = $('#btnFreeFromSelected')
    var $selectAllChkBox = $('#chk_slc_all_user_rl');
    var selUsers = [];
    var unSelUsers = [];
    var selectAllUsers = false;


    function resetSelections() {
        selUsers = [];
        unSelUsers = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);
        $revokeAllBtn.addClass('disabled');
        $revokeAllBtn.removeAttr("onclick", "manageFreeLicenseHandler.freeFromSelected()");
        $revokeAllBtn.attr("title", "Select some learners");
    }

    //populate location list with admin rights
    var freeLicenseTable = $('#learnerListFor_LWFL').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "searching": false,
        "ajax": {
            "url": hdnBaseUrl + "/CourseManagement/LoadFreeLicenseLearnerData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $learner.val();
                data.Location = $ddlLoc.val();
                data.Department = $ddlDep.val();
                data.Course = course;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "drawCallback": function (settings) {
            manageFreeLicenseHandler.selectAllRows();
        },
        "columns": [
            { "data": "UserID", "name": "c.UserID", "autoWidth": true },
            { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
            { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
            { "data": "Location", "name": "l.strLocation", "autoWidth": true },
            { "data": "Department", "name": "d.strDepartment", "autoWidth": true }
        ],
        columnDefs: [{
            // render checkbox in first column
            orderable: false,
            targets: [0], render: function (a, b, data, d) {
                return '<input type="checkbox" class="chk-user" id="chk-user-' + data["UserID"] + '" name="chk-user-' + data["UserID"] + '" value="' + $('<div/>').text(data).html() + '">';
            }
        },{
            // render checkbox in first column
            targets: [1], render: function (a, b, data, d) {
                return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>';
            }
        },
           {
            // render action buttons in the last column
            targets: [5], render: function (a, b, data, d) {
                return '<button type="button" id="free-user-' + data["UserID"] + '" class="btn btn-sm btn-dark mb-1" onclick="manageFreeLicenseHandler.freeupLicense(this)"><i class="fa fa-fw fa-times"></i><span>Recall</span></button> '
            }
        }],
        'order': [[1, 'asc']]
    });

    function showLearnerWithFreeLicenses($btn) {
        course = $btn.id.split('-').pop();
        $fl_modal.modal('show');
        UTILS.Alert.hide($alert);
        freeLicenseTable.draw();
    }

    $searchBtn.click(function () {
        resetSelections();
        freeLicenseTable.draw();
    })

    $clearsearchBtn.click(function () {
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        resetSelections();
        freeLicenseTable.draw();
    })

    function freeupLicense(btn) {
        if (confirm("Are you sure you want to free license from this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Licenses/FreeUpUnusedLicenses'
            var data = {
                userList: learnerId,
                Course: course,
                count:1
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'License removed successfully.')
                    freeLicenseTable.row($(btn).parents('tr')).remove().draw();
                    $('#licenseSummaryDataTable').DataTable().ajax.reload(null, false);
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove license.')
            }, function (status) {
                console.log(status);
            });
        }
    }


    // function to select all checkbox click
    $selectAllChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllUsers = this.checked;

        if (this.checked) {
            selUsers = [];
            unSelUsers = [];
            $revokeAllBtn.removeClass('disabled');
            $revokeAllBtn.attr("onclick", "manageFreeLicenseHandler.freeFromSelected()");
            $revokeAllBtn.attr("title", "Revoke from all selected learners");
        } else {
            $revokeAllBtn.addClass('disabled');
            $revokeAllBtn.removeAttr("onclick", "manageFreeLicenseHandler.freeFromSelected()");
            $revokeAllBtn.attr("title", "Select some learners");
        }
        manageFreeLicenseHandler.selectAllRows();
    });

    // function to check learner checkbox click
    $('#learnerListFor_LWFL tbody').on('change', 'input[type="checkbox"]', function () {
        // If checkbox is not checked
        if (!this.checked) {
            var el = $selectAllChkBox.get(0);
            // If "Select all" control is checked and has 'indeterminate' property
            if (el && el.checked && ('indeterminate' in el)) {
                el.indeterminate = true;
            }
        }

        // push values in selected or unselected arrays respectively to maintain state
        var id = this.id.split('-').pop();

        //push in selected array
        if (this.checked) {
            selUsers.push(id);
            // enable Revoke from all button
            $revokeAllBtn.removeClass('disabled');
            $revokeAllBtn.attr("onclick", "manageFreeLicenseHandler.freeFromSelected()");
            $revokeAllBtn.attr("title", "Revoke from all selected learners");

            // remove id if exist in unselected array
            if (unSelUsers.length > 0) {
                for (i = 0; i < unSelUsers.length; i++) {
                    if (unSelUsers[i] == id) {
                        unSelUsers.splice(i, 1);
                        break;
                    }
                }
            }
        }
        else {

            //push in unselected array
            unSelUsers.push(id);

            // remove id if exist in selected array
            if (selUsers.length > 0) {
                for (i = 0; i < selUsers.length; i++) {
                    if (selUsers[i] == id) {
                        selUsers.splice(i, 1);
                        break;
                    }
                }
            }
        }
    });

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = freeLicenseTable.rows({ 'search': 'applied' }).nodes();

        // Check/uncheck checkboxes for all rows in the table
        $('input[type="checkbox"]', rows).prop('checked', $selectAllChkBox.prop('checked'));

        //check each learner and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-user').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selUsers.length > 0) {
                for (i = 0; i < selUsers.length; i++) {
                    if (selUsers[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unSelUsers.length > 0) {
                for (j = 0; j < unSelUsers.length; j++) {
                    if (unSelUsers[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }

    // function to bulk assign from assign to selected button
    function freeFromSelected() {
        if (selUsers.length > 0 || selectAllUsers) {
            if (confirm("Are you sure you want to revoke from all selected users?")) {

                var url = hdnBaseUrl + 'Licenses/FreeUpUnusedLicensesFromMultiple'
                if (selectAllUsers)
                    selUsers = [0];
                var data = {
                    allSelected: selectAllUsers,
                    selectedUserList: selUsers,
                    unselectedUserList: unSelUsers,
                    SearchText: $learner.val(),
                    Location: $ddlLoc.val(),
                    Department: $ddlDep.val(),
                    Course: course
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'License removed successfully.')
                        resetSelections();
                        freeLicenseTable.draw();
                        $('#licenseSummaryDataTable').DataTable().ajax.reload(null, false);
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to remove license.')
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to remove license. Please try again later')
                });
            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select learner(s) to allocate module');

            resetSelections();
        }

    }

    return {
        showLearnerWithFreeLicenses: showLearnerWithFreeLicenses,
        selectAllRows: selectAllRows,
        freeupLicense: freeupLicense,
        freeFromSelected: freeFromSelected
    }
})();