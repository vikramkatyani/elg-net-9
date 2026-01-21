$(function () {
    if (currentpageId == "ra_auto_allocate") {
        UTILS.activateNavigationLink('manageRALink');
        UTILS.activateMenuNavigationLink('menu-auto-ra-allocation');
        autoAllocateModuleHandler.init(2);
    }
    else {
        UTILS.activateNavigationLink('moduleLink');
        UTILS.activateMenuNavigationLink('menu-auto-license-allocation');
        autoAllocateModuleHandler.init(1);
    }
    $('[data-toggle="tooltip"]').tooltip();
});

var autoAllocateModuleHandler = (function () {
    var $ddlDepartment = $('#ddlDepartment')
    var $ddlLoc = $('#ddlLocation')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchDepartmentModule')
    var $clearSearchBtn = $('#clearSearchDepartmentModule')
    var $alert = $('#divMessage_AutoAllocateModule');

    var $allocateEntireDepartmentBtn = $('#btnAssignToEntireDepartment')
    var $allocateEntireOrgBtn = $('#btnAssignToEntireOrg')
    var $allocateAllBtn = $('#btnAllocateToSelected')
    var $selectAllChkBox = $('#chk_slc_all_departments');

    var selDepartments = [];
    var unselDepartments = [];
    var selectAllUsers = false;

    var selectedModule = 0;
    var selectedLocation = 0;

    function resetSelections() {
        selDepartments = [];
        unselDepartments = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);

        //$allocateEntireOrgBtn.addClass('disabled');
        //$allocateEntireOrgBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToEntireOrg()");
        //$allocateEntireOrgBtn.attr("title", "Select a course");

        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some depatments");
    }

    var departmentTable = $('#autoAllocateModuleDepartmentList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": hdnBaseUrl + "Licenses/LoadDepartmentForLicenseAutoAllocation",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.Location = $ddlLoc.val();
                data.Course = $ddlCourse.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "UserID", "name": "c.UserID", "autoWidth": true },
            { "data": "DepartmentName", "name": "d.strDepartment", "autoWidth": true }
        ],
        "drawCallback": function (settings) {
            autoAllocateModuleHandler.selectAllRows();
        },
        columnDefs: [{
            // render checkbox in first column
            orderable: false,
            targets: [0], render: function (a, b, data, d) {
                return '<input type="checkbox" class="chk-department" id="chk-department-' + data["DepartmentId"] + '" name="chk-department-' + data["DepartmentId"] + '" value="' + $('<div/>').text(data).html() + '">';
            }
        },
        {
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                if (data["Assigned"]) {
                    return '<button type="button"id="remove-dep-' + data["DepartmentId"] + '" class="btn btn-sm btn-secondary mb-1" onclick="autoAllocateModuleHandler.removeAutoAssignmemt(this)"><i class="fa fa-fw fa-times"></i><span>Remove from Auto</span></button> '
                } else {
                    return '<button type="button"id="assign-dep-' + data["DepartmentId"] + '" class="btn btn-sm btn-dark mb-1" onclick="autoAllocateModuleHandler.setForAutoAssignment(this)"><i class="fa fa-fw fa-plus-circle"></i><span>Set for Auto</span></button> '
                }
            }
        }],
        'order': [[1, 'asc']]
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)

        //reset select users if course is changed
        if (selectedModule != $ddlCourse.val() || selectedLocation != $ddlLoc.val()) {
            resetSelections()
        }
        selectedModule = $ddlCourse.val();
        selectedLocation = $ddlLoc.val();

        if (selectedLocation != 0 && selectedModule != 0)
            departmentTable.draw();
        else
            UTILS.Alert.show($alert, 'warning', 'Please select a Course and a Location to set auto license allocation for a department.')
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $ddlLoc.val('0');
        $ddlCourse.val('0');
        selectedModule = 0;
        resetSelections();
        learnedepartmentTablerTable.draw();
    });


    //allocate to entire organisation
    $allocateEntireOrgBtn.click(function (e) {
        e.preventDefault();
        assignToEntireOrg();
    });

    //allocate to entire department
    $allocateEntireDepartmentBtn.click(function (e) {
        e.preventDefault();
        var selectedDepartment = $ddlDepartment.val();
        if (selectedDepartment > 0)
            assignToEntireDepartment();
        else {
            UTILS.Alert.show($alert, 'warning', 'Please select a department to set auto license allocation for a department.');
            return false;
        }
    });


    //// Handle click on "Select all" control
    //$('#chk_slc_all_departments').on('click', function () {
    //    // Get all rows with search applied
    //    var rows = departmentTable.rows({ 'search': 'applied' }).nodes();
    //    // Check/uncheck checkboxes for all rows in the table
    //    $('input[type="checkbox"]', rows).prop('checked', this.checked);
    //});

    //// Handle click on checkbox to set state of "Select all" control
    //$('#autoAllocateModuleDepartmentList tbody').on('change', 'input[type="checkbox"]', function () {
    //    // If checkbox is not checked
    //    if (!this.checked) {
    //        var el = $('#chk_slc_all_departments').get(0);
    //        // If "Select all" control is checked and has 'indeterminate' property
    //        if (el && el.checked && ('indeterminate' in el)) {
    //            // Set visual state of "Select all" control
    //            // as 'indeterminate'
    //            el.indeterminate = true;
    //        }
    //    }
    //});

    // function to initialise report page
    // bind drop down in search area
    function init(pageId) {
        UTILS.Alert.hide($alert)
        renderCourseDropDown(pageId);
        renderLocationDropDown();
        renderDepartmentDropDown();
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

    //function to render list of all department in organisation
    function renderDepartmentDropDown() {
        $ddlDepartment.empty();
        $ddlDepartment.append($('<option/>', { value: '0', text: 'Select' }));

        UTILS.data.getCompanyDepartments(function (data) {
            if (data && data.departmentList != null) {
                $.each(data.departmentList, function (index, item) {
                    $ddlDepartment.append($('<option/>', {
                        value: item.DepartmentId,
                        text: item.DepartmentName
                    }))
                });
            }
        })
    }

    //function to remove department from auto allocation of licenses
    function removeAutoAssignmemt(btn) {
        if (confirm("Are you sure you want to remove auto assignment of license(s) to future user(s) from this department?")) {
            var departmentId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Licenses/RemoveLicenseAutoAllocationForDepartment'
            var data = {
                Course: $ddlCourse.val(),
                Location: $ddlLoc.val(),
                Departments: departmentId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Auto assignment of licenses removed from the department successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove auto assignment of licenses from the department.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to remove all department in location from auto allocation of licenses
    function removeAutoAssignmemtFromLocation() {
        if (confirm("Are you sure you want to remove auto assignment of license(s) to future user(s) from this location?")) {
            var url = hdnBaseUrl + 'Licenses/RemoveLicenseAutoAllocationForAllDepartments'
            var data = {
                Course: $ddlCourse.val(),
                Location: $ddlLoc.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Auto assignment of licenses removed from the location successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove auto assignment of licenses from the location.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to set department for auto allocation of licenses
    function setForAutoAssignment(btn) {
        if (confirm("Are you sure you want to set auto assignment of license(s) to future user(s) for this department?")) {
            var departmentId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForDepartment'
            var data = {
                Course: $ddlCourse.val(),
                Location: $ddlLoc.val(),
                Departments: departmentId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Department set for auto assignment successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to set for auto assignment.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to set department for auto allocation of licenses
    function setForAutoAssignmentToLocation() {
        if (confirm("Are you sure you want to set auto assignment of license(s) to future user(s) for this location?")) {
            var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForAllDepartments'
            var data = {
                Course: $ddlCourse.val(),
                Location: $ddlLoc.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Location set for auto assignment successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to set for auto assignment.')
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
            selDepartments = [];
            unselDepartments = [];
            $allocateAllBtn.removeClass('disabled');
            $allocateAllBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Set auto to all selected departments");
        } else {
            $allocateAllBtn.addClass('disabled');
            $allocateAllBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Select some depatments");
        }
        autoAllocateModuleHandler.selectAllRows();
    });

    // function to check department checkbox click
    $('#autoAllocateModuleDepartmentList tbody').on('change', 'input[type="checkbox"]', function () {
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
            selDepartments.push(id);
            // enable allocate to all button
            $allocateAllBtn.removeClass('disabled');
            $allocateAllBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Allocate to all selected departments");

            // remove id if exist in unselected array
            if (unselDepartments.length > 0) {
                for (i = 0; i < unselDepartments.length; i++) {
                    if (unselDepartments[i] == id) {
                        unselDepartments.splice(i, 1);
                        break;
                    }
                }
            }
        }
        else {

            //push in unselected array
            unselDepartments.push(id);

            // remove id if exist in selected array
            if (selDepartments.length > 0) {
                for (i = 0; i < selDepartments.length; i++) {
                    if (selDepartments[i] == id) {
                        selDepartments.splice(i, 1);
                        break;
                    }
                }
            }
        }
    });

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = departmentTable.rows({ 'search': 'applied' }).nodes();

        // Check/uncheck checkboxes for all rows in the table
        $('input[type="checkbox"]', rows).prop('checked', $selectAllChkBox.prop('checked'));

        //check each department and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-department').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selDepartments.length > 0) {
                for (i = 0; i < selDepartments.length; i++) {
                    if (selDepartments[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unselDepartments.length > 0) {
                for (j = 0; j < unselDepartments.length; j++) {
                    if (unselDepartments[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }

    // function to bulk assign from assign to selected button
    function assignToSelected() {
        if (selDepartments.length > 0 || selectAllUsers) {
            if (confirm("Are you sure you want to set auto allocation for selected departments?")) {
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForAllDepartments'
                var data = {
                    allSelected: selectAllUsers,
                    selectedDepartmentList: selDepartments,
                    unselectedDepartmentList: unselDepartments,
                    Location: $ddlLoc.val(),
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        departmentTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later')
                    UTILS.resetButton($allocateAllBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select department(s) to auto allocate course');
            resetSelections()
            ////disable allocate to all buton
            //$allocateAllBtn.addClass('disabled');
            //$allocateAllBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }

    // function to set auto allocation for all locations in the organisation
    function assignToEntireOrg() {
        if (parseInt($ddlCourse.val()) > 0) {
            if (confirm("Are you sure you want to set auto allocation for entire company?")) {
                UTILS.disableButton($allocateEntireOrgBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForEntireOrg'
                var data = {
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        departmentTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                    UTILS.resetButton($allocateEntireOrgBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later')
                    UTILS.resetButton($allocateEntireOrgBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a course to auto allocate licences');
            resetSelections();
            UTILS.resetButton($allocateEntireOrgBtn);
        }

    }

    // function to set auto allocation for selected department in the organisation
    function assignToEntireDepartment() {
        if (parseInt($ddlCourse.val()) > 0) {
            if (confirm("Are you sure you want to set auto allocation for selected department in entire company?")) {
                UTILS.disableButton($allocateEntireDepartmentBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForEntireOrgDepartment'
                var data = {
                    Course: $ddlCourse.val(),
                    Department: $ddlDepartment.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        departmentTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                    UTILS.resetButton($allocateEntireDepartmentBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later')
                    UTILS.resetButton($allocateEntireDepartmentBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a course to auto allocate licences');
            resetSelections();
            UTILS.resetButton($allocateEntireDepartmentBtn);
        }

    }
    return {
        init: init,
        removeAutoAssignmemtFromLocation: removeAutoAssignmemtFromLocation,
        removeAutoAssignmemt: removeAutoAssignmemt,
        setForAutoAssignment: setForAutoAssignment,
        setForAutoAssignmentToLocation: setForAutoAssignmentToLocation,
        selectAllRows: selectAllRows,
        assignToSelected: assignToSelected,
        assignToEntireOrg: assignToEntireOrg,
        assignToEntireDepartment: assignToEntireDepartment
    }
})();