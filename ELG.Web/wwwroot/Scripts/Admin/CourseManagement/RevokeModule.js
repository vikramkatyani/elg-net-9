$(function () {
    if (currentpageId == "suspend_ra_access") {
        UTILS.activateNavigationLink('manageRALink');
        UTILS.activateMenuNavigationLink('menu-revoke-ra');
        revokeModuleHandler.init(2);
    }
    else {
        UTILS.activateNavigationLink('moduleLink');
        UTILS.activateMenuNavigationLink('menu-revoke-module');
        revokeModuleHandler.init(1);
    }
    $('[data-toggle="tooltip"]').tooltip();
});


var revokeModuleHandler = (function () {
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchRevokeModule')
    var $clearSearchBtn = $('#clearSearchRevokeModule')
    var $revokeAllBtn = $('#btnRevokeFromSelected')
    var $selectAllChkBox = $('#chk_slc_all_user_ra');
    var $alert = $('#divMessage_RevokeModule');
    var selectedModule = 0;

    var selUsers = [];
    var unSelUsers = [];
    var selectAllUsers = false;

    var $reportContainer = $('#div-report-container');

    var dataURL = hdnBaseUrl + "CourseManagement/LoadLearnerDataToRevokeModule";
    if (currentpageId == "suspend_ra_access")
        dataURL = hdnBaseUrl + "CourseManagement/LoadLearnerDataToRevokeRA";

    function resetSelections() {
        selUsers = [];
        unSelUsers = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);
        $revokeAllBtn.addClass('disabled');
        $revokeAllBtn.removeAttr("onclick", "revokeModuleHandler.revokeFromSelected()");
        $revokeAllBtn.attr("title", "Select some learners");
    }

    function showDefaultMessage() {
        $reportContainer.hide();
        $('#revokeModuleLearnerList').DataTable().destroy();
        var message = ' ';
        if (currentpageId == "suspend_ra_access") {
            message = '<div ><b>How to use this page:</b><ul>' +
                '<li>Select a <b>Risk Assessment</b> from the dropdown to begin.</li>' +
                '<li>Use the <b>User</b> field to search by name or email.</li>' +
                '<li>Apply additional filters to narrow your search.</li>' +
                '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to view users who do not yet have a license for the selected assessment.</li>' +
                '<li>Select one or more users using the checkboxes in the first column.</li>' +
                '<li>Click <b>Suspend from Selected</b> to revoke access of the risk assessment from those users.</li>' +
                '<li>Click <i class="fa fa-times me-1"></i> <b>Clear</b> to reset all filters and start over.</li>' +
                '</ul></div>';
        }

        else {
            message = '<div ><b>How to use this page:</b><ul>' +
                '<li>Select a <b>Course</b> from the dropdown to begin.</li>' +
                '<li>Use the <b>User</b> field to search by name or email.</li>' +
                '<li>Apply additional filters to narrow your search.</li>' +
                '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to view users who do not yet have a license for the selected course.</li>' +
                '<li>Select one or more users using the checkboxes in the first column.</li>' +
                '<li>Click <b>Suspend from Selected</b> to revoke access of the course from those users.</li>' +
                '<li>Click <i class="fa fa-times me-1"></i> <b>Clear</b> to reset all filters and start over.</li>' +
                '</ul></div>';
        }
        UTILS.Alert.show($alert, 'default', message);
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $reportContainer.show();
        //reset select users if course is changed
        if (selectedModule != $ddlCourse.val()) {
            resetSelections()
        }
        selectedModule = $ddlCourse.val();
        $('#revokeModuleLearnerList').DataTable().destroy();
        $('#revokeModuleLearnerList').DataTable({
            lengthChange: false,
            processing: true,
            serverSide: true,
            filter: false,
            orderMulti: true,
            stateSave: true,
            language: {
                processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
                emptyTable: "No record(s) found."
            },
            ajax: {
                url: dataURL,
                type: "POST",
                datatype: "json",
                data: function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                },
                error: function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong. Please try again later.");
                }
            },
            drawCallback: function (settings) {
                revokeModuleHandler.selectAllRows();
            },
            columns: [
                { data: "UserID", name: "c.UserID", width: "5%" },
                { data: "FirstName", name: "c.strFirstName", width: "25%" },
                { data: "EmailId", name: "c.strEmail", width: "25%" },
                { data: "Location", name: "l.strLocation", width: "20%" },
                { data: "Department", name: "l.strDepartment", width: "20%" },
                { data: null, orderable: false, searchable: false, width: "5%" }
            ],
            columnDefs: [
                {
                    targets: 0,
                    orderable: false,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        return `<input type="checkbox" class="chk-user" id="chk-user-${data["UserID"]}" name="chk-user-${data["UserID"]}" value="${data["UserID"]}">`;
                    }
                },
                {
                    targets: 1,
                    render: function (a, b, data, d) {
                        return `<span>${data["FirstName"]} ${data["LastName"]}</span>`;
                    }
                },
                //{
                //    targets: 5,
                //    className: "text-center align-middle",
                //    render: function (a, b, data, d) {
                //        const userId = data["UserID"];
                //        let btn = `
                //<div class="d-flex justify-content-center align-items-center">
                //    <div class="dropdown">
                //        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                //                id="actionDropdown-${userId}" data-bs-toggle="dropdown" aria-expanded="false"
                //                style="width: 2.5rem; height: 2.5rem;">
                //            <i class="fa fa-ellipsis-v"></i>
                //        </button>
                //        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${userId}">
                //            <li>
                //                <a class="dropdown-item" href="#" id="revoke-user-${userId}"
                //                   onclick="revokeModuleHandler.revokeModule(this)">
                //                    <i class="fa fa-fw fa-times me-2"></i>Suspend
                //                </a>
                //            </li>`;

                //        if (currentpageId === "suspend_ra_access" && data["IsRASignedOff"]) {
                //            btn += `
                //            <li>
                //                <a class="dropdown-item" href="#" id="retake-user-${userId}"
                //                   onclick="revokeModuleHandler.retakeRA(this)">
                //                    <i class="fa fa-fw fa-sync me-2"></i>Retake
                //                </a>
                //            </li>`;
                //        }

                //        btn += `</ul></div></div>`;
                //        return btn;
                //    }
                //}
                {
                    targets: 5,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        const userId = data["UserID"];
                        const isRASignedOff = data["IsRASignedOff"];
                        const isRetakeVisible = currentpageId === "suspend_ra_access" && isRASignedOff;

                        let iconClass = "fa-times";
                        let btnText = "Suspend";
                        let actionFn = "revokeModule";

                        if (isRetakeVisible) {
                            iconClass = "fa-sync";
                            btnText = "Retake";
                            actionFn = "retakeRA";
                        }

                        return `
                        <button type="button" data-toggle="tooltip" title = "${btnText}"
                                class="btn btn-sm border-0 p-2 rounded-circle"
                                id="${actionFn}-user-${userId}"
                                onclick="revokeModuleHandler.${actionFn}(this)">
                            <i class="fa fa-fw ${iconClass} me-1"></i>
                        </button>
                    `;
                    }
                }
            ],
            order: [[1, 'asc']]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        selectedModule = 0;
        resetSelections()
        showDefaultMessage();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    // function to initialise report page
    // bind drop down in search area
    function init(pageId) {
        UTILS.Alert.hide($alert)
        renderCourseDropDown(pageId);
        renderLocationDropDown();
        showDefaultMessage();
    }

    //function to render list of all locations in organisation
    function renderCourseDropDown(pageId) {
        $ddlCourse.empty();
        if (pageId == 1) {
            $ddlCourse.append($('<option/>', { value: '0', text: 'Select Course' }));
            UTILS.data.getAllCourses(function (data) {
                populateList(data);
            })
        }
        else if (pageId == 2) {
            $ddlCourse.append($('<option/>', { value: '0', text: 'Select Risk Assessment' }));
            UTILS.data.getAllRiskAssessments(function (data) {
                populateList(data);
            })
        }
    }

    function populateList(data) {
        if (data && data.courseList != null) {
            $.each(data.courseList, function (index, item) {
                $ddlCourse.append($('<option/>', {
                    value: item.CourseId,
                    text: item.CourseName
                }))
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

    //function to delete learner and refresh table
    function revokeModule(btn) {
        if (confirm("Are you sure you want to suspend course access from this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'CourseManagement/RevokeLearnerModuleAccess'
            var data = {
                LearnerID: learnerId,
                Course: $ddlCourse.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert,'success','Access suspended successfully.')
                    $('#revokeModuleLearnerList').DataTable().row($(btn).parents('tr')).remove().draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to suspend access.')
            }, function (status) {
                console.log(status);
                UTILS.Alert.show($alert, 'error', 'Failed to suspend access. Please try again later');
            });
        }
    }


    //function to retake risk assessment
    function retakeRA(btn) {
        if (confirm("Are you sure you want to assign new risk assessment to this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'CourseManagement/RetakeLearnerRAAccess'
            var data = {
                LearnerID: learnerId,
                Course: $ddlCourse.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert,'success','New risk assessment assigned successfully.')
                    $('#revokeModuleLearnerList').DataTable().row($(btn).parents('tr')).remove().draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to assign new risk assessment.')
            }, function (status) {
                console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to assign new risk assessment. Please try again later');
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
            $revokeAllBtn.attr("onclick", "revokeModuleHandler.revokeFromSelected()");
            $revokeAllBtn.attr("title", "Suspend access from selected learners");
        } else {
            $revokeAllBtn.addClass('disabled');
            $revokeAllBtn.removeAttr("onclick", "revokeModuleHandler.revokeFromSelected()");
            $revokeAllBtn.attr("title", "Select some learners");
        }
        revokeModuleHandler.selectAllRows();
    });

    // function to check learner checkbox click
    $('#revokeModuleLearnerList tbody').on('change', 'input[type="checkbox"]', function () {
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
            // enable revoke to all button
            $revokeAllBtn.removeClass('disabled');
            $revokeAllBtn.attr("onclick", "revokeModuleHandler.revokeFromSelected()");
            $revokeAllBtn.attr("title", "Suspend from all selected learners");

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
        var rows = $('#revokeModuleLearnerList').DataTable().rows({ 'search': 'applied' }).nodes();

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
    function revokeFromSelected() {
        if (selUsers.length > 0 || selectAllUsers) {
            if (confirm("Are you sure you want to suspend access from all selected users?")) {
                UTILS.disableButton($revokeAllBtn);
                var url = hdnBaseUrl + 'CourseManagement/RevokeLearnerModuleAccess_Multiple'
                var data = {
                    allSelected: selectAllUsers,
                    selectedUserList: selUsers,
                    unselectedUserList: unSelUsers,
                    SearchText: $learner.val(),
                    Location: $ddlLoc.val(),
                    Department: $ddlDep.val(),
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Access suspended successfully.')
                        resetSelections();
                        $('#revokeModuleLearnerList').DataTable().draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to suspend access.')
                    UTILS.resetButton($revokeAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to suspend access. Please try again later');
                    UTILS.resetButton($revokeAllBtn);
                });
            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select learner(s) to suspend course access');

            //disable revoke from all buton
            $revokeAllBtn.addClass('disabled');
            $revokeAllBtn.removeAttr("onclick", "revokeModuleHandler.revokeFromSelected()");
            $revokeAllBtn.attr("title", "Select some learners");
        }

    }

    return {
        init: init,
        revokeModule: revokeModule,
        renderLocationDropDown: renderLocationDropDown,
        selectAllRows: selectAllRows,
        revokeFromSelected: revokeFromSelected,
        retakeRA: retakeRA
    }
})();