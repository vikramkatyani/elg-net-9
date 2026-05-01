$(function () {
    if (currentpageId == "allocate_ra") {
        UTILS.activateNavigationLink('manageRALink');
        UTILS.activateMenuNavigationLink('menu-allocate-ra');
        allocateModuleHandler.init(2);
    }
    else {
        UTILS.activateNavigationLink('moduleLink');
        UTILS.activateMenuNavigationLink('menu-allocate-module');
        allocateModuleHandler.init(1);
    }
    $('[data-toggle="tooltip"]').tooltip();
});


var allocateModuleHandler = (function () {
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchAllocateModule')
    var $clearSearchBtn = $('#clearSearchAllocateModule')
    var $allocateAllBtn = $('#btnAllocateToSelected')
    var $selectAllChkBox = $('#chk_slc_all_user_ma');
    var $alert = $('#divMessage_AllocateModule');
    var $reportContainer = $('#div-report-container');

    var selectedModule = 0;
    var selUsers = [];
    var unSelUsers = [];
    var selectAllUsers = false;

    function showDefaultMessage() {
        $reportContainer.hide();
        $('#allocateModuleLearnerList').DataTable().destroy();
        var message = ' ';
        if (currentpageId == "allocate_ra") {
            message = '<div><b>How to use this page:</b><ul>' +
                '<li>This tool allows you to assign a risk assessment to specific individuals or departments within your organization.</li>' +
                '<li>Select a <b>Risk Assessment</b> from the dropdown to begin.</li>' +
                '<li>Use the <b>User</b> field to search by name or email.</li>' +
                '<li>Apply additional filters to narrow your search.</li>' +
                '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to view users who do not yet have a license for the selected assessment.</li>' +
                '<li>Select one or more users using the checkboxes in the first column.</li>' +
                '<li>Click <b>Allocate to Selected</b> to assign the risk assessment to those users.</li>' +
                '<li>Click <i class="fa fa-times me-1"></i> <b>Clear</b> to reset all filters and start over.</li>' +
                '</ul></div>';
        }
            
        else {
            message = '<div><b>How to use this page:</b><ul>' +
        '<li>This tool allows you to assign a course to specific individuals or departments within your organization.</li>' +
        '<li>Select a <b>Course</b> from the dropdown to begin.</li>' +
        '<li>Use the <b>User</b> field to search by name or email.</li>' +
        '<li>Apply additional filters to narrow your search.</li>' +
        '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to view users who do not yet have a license for the selected course.</li>' +
        '<li>Select one or more users using the checkboxes in the first column.</li>' +
        '<li>Click <b>Allocate to Selected</b> to assign the course to those users.</li>' +
        '<li>Click <i class="fa fa-times me-1"></i> <b>Clear</b> to reset all filters and start over.</li>' +
        '</ul></div>';
        }
        UTILS.Alert.show($alert, 'default', message);
    }

    function resetSelections() {
        selUsers = [];
        unSelUsers = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);
        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick", "allocateModuleHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some learners");
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $reportContainer.show();

        //reset select users if course is changed
        if (selectedModule != $ddlCourse.val()) {
            resetSelections()
        }
        selectedModule = $ddlCourse.val();
        $('#allocateModuleLearnerList').DataTable().destroy();
        $('#allocateModuleLearnerList').DataTable({
            lengthChange: false,
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": true,
            "stateSave": true,
            "ajax": {
                "url": hdnBaseUrl + "CourseManagement/LoadLearnerDataToAllocateModule",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "drawCallback": function (settings) {
                allocateModuleHandler.selectAllRows();
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
                targets: [0], 
                render: function (a, b, data, d) {
                    return `<input type="checkbox" class="chk-user" id="chk-user-${data.UserID}" value="${data.UserID}">`;
                }
            }, {
                // render learner name
                targets: [1], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            },
            //{
            //    targets: [5],
            //    orderable: false,
            //    searchable: false,
            //    className: "text-center align-middle",
            //    render: function (a, b, data, d) {
            //        const userId = data["UserID"];
            //        const isDeactive = data["IsDeactive"];
            //        const isExpired = data["IsCourseExpired"] == 1 || data["IsCourseExpired"] === '1' || data["IsCourseExpired"] === true || data["IsCourseExpired"] === 'true';

            //        let btn = `
            //                <div class="d-flex justify-content-center align-items-center">
            //                    <div class="dropdown">
            //                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
            //                                id="actionDropdown-${userId}" data-bs-toggle="dropdown" aria-expanded="false"
            //                                style="width: 2.5rem; height: 2.5rem;">
            //                            <i class="fa fa-ellipsis-v"></i>
            //                        </button>
            //                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${userId}">`;

            //                            if (isDeactive == 1) {
            //                                btn += `
            //                        <li>
            //                            <a class="dropdown-item" href="#" id="assign-user-${userId}"
            //                               onclick="allocateModuleHandler.allocateModuleAccess(this)">
            //                                <i class="fa fa-fw fa-sync me-2"></i>ReAssign Access
            //                            </a>
            //                        </li>`;
            //                            } else if (isExpired) {
            //                                btn += `
            //                        <li>
            //                            <a class="dropdown-item" href="#" id="assign-user-${userId}"
            //                               onclick="allocateModuleHandler.assignModuleLicense(this)">
            //                                <i class="fa fa-fw fa-plus-circle me-2"></i>ReAssign Licence
            //                            </a>
            //                        </li>`;
            //                            } else {
            //                                btn += `
            //                        <li>
            //                            <a class="dropdown-item" href="#" id="assign-user-${userId}"
            //                               onclick="allocateModuleHandler.assignModuleLicense(this)">
            //                                <i class="fa fa-fw fa-plus-circle me-2"></i>Assign
            //                            </a>
            //                        </li>`;
            //                            }

            //                            btn += `
            //                        </ul>
            //                    </div>
            //                </div>`;

            //        return btn;
            //    }
                //}
                {
                    targets: [5],
                    orderable: false,
                    searchable: false,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        const userId = data["UserID"];
                        const isDeactive = data["IsDeactive"];
                        const isExpired = data["IsCourseExpired"] == 1 || data["IsCourseExpired"] === '1' || data["IsCourseExpired"] === true || data["IsCourseExpired"] === 'true';

                        let iconClass = "fa-plus-circle";
                        let btnText = "Assign";
                        let actionFn = "assignModuleLicense";

                        if (isDeactive == 1) {
                            iconClass = "fa-sync";
                            btnText = "ReAssign Access";
                            actionFn = "allocateModuleAccess";
                        } else if (isExpired) {
                            iconClass = "fa-plus-circle";
                            btnText = "ReAssign Licence";
                            actionFn = "assignModuleLicense";
                        }

                        return `
                            <button type="button" data-toggle="tooltip" title = "${btnText}"
                                    class="btn btn-sm  border-0 p-2 rounded-circle"
                                    id="assign-user-${userId}"
                                    onclick="allocateModuleHandler.${actionFn}(this)">
                                <i class="fa fa-fw ${iconClass} me-1"></i>
                            </button>
                        `;
                    }
                }
        ],
            'order': [[1, 'asc']]
        });
    });

        // function to check learner checkbox click
    $('#allocateModuleLearnerList').on('draw.dt', function () {
        $('#allocateModuleLearnerList tbody').off('change', 'input.chk-user'); // remove any duplicate bindings
        $('#allocateModuleLearnerList tbody').on('change', 'input[type="checkbox"]', function () {
            debugger;
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
                // enable allocate to all button
                $allocateAllBtn.removeClass('disabled');
                $allocateAllBtn.attr("onclick", "allocateModuleHandler.assignToSelected()");
                $allocateAllBtn.attr("title", "Allocate to all selected learners");

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
        resetSelections();
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
        renderDepartmentDropDown(0);
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

        //populate all departments if location is not selected
        if (locationId == 0) {
            UTILS.data.getCompanyDepartments(function (data) {
                if (data && data.departmentList != null) {
                    $.each(data.departmentList, function (index, item) {
                        $ddlDep.append($('<option/>', {
                            value: item.DepartmentId,
                            text: item.DepartmentName
                        }))
                    });
                }
            })
        } else {
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
    }

    //function to re-assign module access to single learner and refresh table
    function allocateModuleAccess(btn) {
        if (confirm("Are you sure you want to re-assign access to this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'CourseManagement/ReassignLearnerModuleAccess'
            var data = {
                LearnerID: learnerId,
                Course: $ddlCourse.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Access re-assigned successfully.')
                    $('#allocateModuleLearnerList').DataTable().draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to re-assign access.')
            }, function (status) {
                console.log(status);
                });
        }
    }

    //function to allocate module to single learner and refresh table
    function assignModuleLicense(btn) {
        if (confirm("Are you sure you want to assign course to this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'CourseManagement/AllocateModuleLicenseToLearner'
            var data = {
                LearnerID: learnerId,
                Course: $ddlCourse.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Course assigned successfully.')
                    $('#allocateModuleLearnerList').DataTable().draw();
                }
                else if (result.success == 2) {
                    UTILS.Alert.show($alert, 'error', 'Failed to allocate course. Not enough licences available.')
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to assign course.')
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
            $allocateAllBtn.removeClass('disabled');
            $allocateAllBtn.attr("onclick", "allocateModuleHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Allocate to all selected learners");
        } else {
            $allocateAllBtn.addClass('disabled');
            $allocateAllBtn.removeAttr("onclick", "allocateModuleHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Select some learners");
        }
        allocateModuleHandler.selectAllRows();
    });

    

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = $('#allocateModuleLearnerList').DataTable().rows({ 'search': 'applied' }).nodes();

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
    function assignToSelected() {
        if (selUsers.length > 0 || selectAllUsers) {
            if (confirm("Are you sure you want to allocate licences to all selected users?")) {
                // allocate functionality
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + "CourseManagement/AllocateModuleLicenseToLearner_Multiple";
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
                        UTILS.Alert.show($alert, 'success', 'Course allocated successfully.')
                        resetSelections()
                        $('#allocateModuleLearnerList').DataTable().draw();
                    }
                    else if (result.success == 2) {
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate course. Not enough licences available.')
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate course.')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to allocate course. Please try again later');
                    UTILS.resetButton($allocateAllBtn);
                })
                
            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select learner(s) to allocate course');
            resetSelections()
            ////disable allocate to all buton
            //$allocateAllBtn.addClass('disabled');
            //$allocateAllBtn.removeAttr("onclick", "allocateModuleHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some learners");
        }
        
    }

    return {
        init: init,
        assignModuleLicense: assignModuleLicense,
        allocateModuleAccess: allocateModuleAccess,
        renderLocationDropDown: renderLocationDropDown,
        renderDepartmentDropDown: renderDepartmentDropDown,
        selectAllRows: selectAllRows,
        assignToSelected: assignToSelected
    }
})();