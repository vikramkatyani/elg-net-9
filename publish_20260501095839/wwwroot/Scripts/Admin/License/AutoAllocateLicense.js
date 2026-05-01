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
    $(".assign-sel-row").hide();
    $(".type-table-container").hide();
    $('#applyAssignmentMethod').hide();
});

var autoAllocateModuleHandler = (function () {


    var $initialInfoMessage = "";
    var $advancedSelInfoMessage = "";
    var $locSelInfoMessage = "";
    var $depSelInfoMessage = "";
    var $compSelInfoMessage = "";
    var $noteAutoLicence = ""

    if (currentpageId == "ra_auto_allocate") {
        $initialInfoMessage = "Simply select your Risk Assessment, then decide if you wish to assign the risk assessment to a specific location, department name or your entire workforce.<br /><b>Note:</b> Auto Allocation works for all future users only. For any pre-registered users requiring the same risk assessment/s please use the Hand-Picked Enrollment option.";
        $advancedSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Advanced:</b> Select a Location from the dropdown and click on \"Search\" button to allocate to specific departments within this location only.";
        $locSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Location:</b> this will include ALL staff throughout that location and any departments within it. Click on \"Continue\" button and select location from the table below to perform this action.";
        $depSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Department:</b> this will include ALL departments with the same name throughout your entire company structure regardless of their location. Click on \"Continue\" button and select department from the table below to perform this action.";
        $compSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Entire Company:</b> this will include ALL staff throughout your entire company structure. Click on the \"Entire Company\" button to perform this action.";
        $noteAutoLicence = "<br /><b>Note:</b> Auto Allocation works for all future users only. For any pre-registered users requiring the same course/s please use the Hand-Picked Enrollment option."
    }
    else {
        $initialInfoMessage = "Simply select your course, then decide if you wish to assign the course to a specific location, department name or your entire workforce.<br/> <b>Note:</b> Auto Allocation works for all future users only. For any pre-registered users requiring the same course/s please use the Hand-Picked Enrollment option.";
        $advancedSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Advanced:</b> Select a Location from the dropdown and click on \"Search\" button to allocate to specific departments within this location only.";
        $locSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Location:</b> this will include ALL staff throughout that location and any departments within it. Click on \"Continue\" button and select location from the table below to perform this action.";
        $depSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Department:</b> this will include ALL departments with the same name throughout your entire company structure regardless of their location. Click on \"Continue\" button and select department from the table below to perform this action.";
        $compSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Entire Company:</b> this will include ALL staff throughout your entire company structure. Click on the \"Entire Company\" button to perform this action.";
        $noteAutoLicence = "<br /><b>Note:</b> Auto Allocation works for all future users only. For any pre-registered users requiring the same course/s please use the Hand-Picked Enrollment option."
    }

    var $selAssignMethod = $("#ddlMethod");
    var $infoAlert = $('#divLicAutoAllocateInfoMessage');
    var $ddlDepartment = $('#ddlDepartment')
    var $ddlLoc = $('#ddlLocation')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchDepartmentModule')
    var $applyAssignmentMethodBtn = $('#applyAssignmentMethod')
    var $clearSearchBtn = $('#clearSearchDepartmentModule')
    var $alert = $('#divMessage_AutoAllocateModule');

    var $allocateEntireDepartmentBtn = $('#btnAssignToEntireDepartment')
    var $allocateEntireOrgBtn = $('#btnAssignToEntireOrg')
    var $allocateAllBtn = $('#btnAllocateToSelected')
    var $allocateAllLocationBtn = $('#btnAllocateToSelectedLocations')
    var $allocateAllDeptBtn = $('#btnAllocateToSelectedDepartments')
    var $selectAllChkBox = $('#chk_slc_all_departments');
    var $selectAllDeptChkBox = $('#chk_slc_all_company_departments');
    var $selectAllLocationChkBox = $('#chk_slc_all_locations');
    var $departmentTableContainer = $('#autoassigndepartments_tbl_container');
    var $allLocationTableContainer = $('#autoAssignAllLocations_tbl_container');
    var $alldepartmentTableContainer = $('#autoAssignAllDepartments_tbl_container');

    var selDepartments = [];
    var unselDepartments = [];

    var selEntireDepartments = [];
    var unselEntireDepartments = [];
    var selEntireLocations = [];
    var unselEntireLocations = [];

    var selectAllUsers = false;
    var selectAllLocations = false;
    var selectAllDepartments = false;

    var selectedModule = 0;
    var selectedLocation = 0;

    //function to display/ hide as per selection assignment method
    $selAssignMethod.change(function (e) {
        e.preventDefault();
        var selMethod = $selAssignMethod.val();
        $(".assign-sel-row").hide();
        $(".type-table-container").hide();
        $applyAssignmentMethodBtn.hide();
        UTILS.Alert.hide($alert);
       // $clearSearchBtn.click();
        switch (parseInt(selMethod)) {
            case 1: $infoAlert.html($advancedSelInfoMessage + $noteAutoLicence);
                $('#filter-assignby-location').show();
                $departmentTableContainer.show();
                break;
            case 2: $infoAlert.html($locSelInfoMessage + $noteAutoLicence);
                $allLocationTableContainer.show();
                $applyAssignmentMethodBtn.show();
                allLocationsTable.draw();
                break;
            case 3: $infoAlert.html($depSelInfoMessage + $noteAutoLicence);
                $alldepartmentTableContainer.show();
                $applyAssignmentMethodBtn.show();
                allDepartmentsTable.draw();
                break;
            case 4: $infoAlert.html($compSelInfoMessage + $noteAutoLicence);
                $('#filter-assignby-org').show();
                break;
            default: $infoAlert.html($initialInfoMessage);
                break;
        }
    });

    //apply assign method
    $applyAssignmentMethodBtn.click(function (e) {
        e.preventDefault();
        if ($ddlCourse.val() <= 0) {
            UTILS.Alert.show($alert, 'warning', 'Please select a Course to set auto license allocation.');
            return false;
        }
        var selMethod = $selAssignMethod.val();
        switch (parseInt(selMethod)) {
            case 2: allLocationsTable.draw();
                UTILS.Alert.hide($alert);
                break;
            case 3: allDepartmentsTable.draw();
                UTILS.Alert.hide($alert);
                break;
        }
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
            UTILS.Alert.show($alert,'warning', 'Please select a Course and a Location to set auto license allocation for a department.')
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

    function resetSelections() {
        selDepartments = [];
        unselDepartments = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);

        selEntireLocations = [];
        unselEntireLocations = [];
        selectAllLocations = false;
        $selectAllLocationChkBox.prop('checked', false);

        selEntireDepartments = [];
        unselEntireDepartments = [];
        selectAllDepartments = false;
        $selectAllDeptChkBox.prop('checked', false);

        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some depatments");

        $allocateAllLocationBtn.addClass('disabled');
        $allocateAllLocationBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelectedLocations()");
        $allocateAllLocationBtn.attr("title", "Select some locations");

        $allocateAllDeptBtn.addClass('disabled');
        $allocateAllDeptBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelectedDepartments()");
        $allocateAllDeptBtn.attr("title", "Select some depatments");
    }

    var departmentTable = $('#autoAllocateModuleDepartmentList').DataTable({
        lengthChange: false,
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Licenses/LoadDepartmentForLicenseAutoAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Location = $ddlLoc.val();
                data.Course = $ddlCourse.val();
            },
            error: function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong. Please try again later.");
            }
        },
        columns: [
            { data: "DepartmentId", name: "d.DepartmentId", width: "5%" },
            { data: "DepartmentName", name: "d.strDepartment", width: "75%" },
            { data: null, orderable: false, searchable: false, width: "20%" }
        ],
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    return `<input type="checkbox" class="chk-department" id="chk-department-${data["DepartmentId"]}" name="chk-department-${data["DepartmentId"]}" value="${data["DepartmentId"]}">`;
                }
            },
            //{
            //    targets: 2,
            //    className: "text-center align-middle",
            //    render: function (a, b, data, d) {
            //        const depId = data["DepartmentId"];
            //        const isAssigned = data["Assigned"];

            //        return `
            //    <div class="d-flex justify-content-center align-items-center">
            //        <div class="dropdown">
            //            <button class="btn btn-sm  border-0 p-2 rounded-circle" type="button"
            //                    id="actionDropdown-${depId}" data-bs-toggle="dropdown" aria-expanded="false"
            //                    style="width: 2.5rem; height: 2.5rem;">
            //                <i class="fa fa-ellipsis-v"></i>
            //            </button>
            //            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${depId}">
            //                <li>
            //                    <a class="dropdown-item" href="#" id="${isAssigned ? `remove-dep-${depId}` : `assign-dep-${depId}`}"
            //                       onclick="autoAllocateModuleHandler.${isAssigned ? 'removeAutoAssignmemt' : 'setForAutoAssignment'}(this)">
            //                        <i class="fa fa-fw ${isAssigned ? 'fa-times' : 'fa-plus-circle'} me-2"></i>
            //                        ${isAssigned ? 'Remove from Auto' : 'Set for Auto'}
            //                    </a>
            //                </li>
            //            </ul>
            //        </div>
            //    </div>`;
            //    }
            //}
            {
                targets: 2,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    const depId = data["DepartmentId"];
                    const isAssigned = data["Assigned"];
                    const iconClass = isAssigned ? "fa-times" : "fa-plus-circle";
                    const btnText = isAssigned ? "Remove from Auto" : "Set for Auto";
                    const actionFn = isAssigned ? "removeAutoAssignmemt" : "setForAutoAssignment";

                    return `
            <button type="button" data-toggle="tooltip" title = "${btnText}"
                    class="btn btn-sm  border-0 p-2 rounded-circle"
                    id="${isAssigned ? `remove-dep-${depId}` : `assign-dep-${depId}`}"
                    onclick="autoAllocateModuleHandler.${actionFn}(this)">
                <i class="fa fa-fw ${iconClass} me-1"></i> 
            </button>
        `;
                }
            }
        ],
        drawCallback: function (settings) {
            autoAllocateModuleHandler.selectAllRows();
        },
        order: [[1, 'asc']]
    });
        
    var allLocationsTable = $('#autoAllocateModuleAllLocationList').DataTable({
        lengthChange: false,
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Licenses/LoadAllLocationsForLicenseAutoAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Course = $ddlCourse.val();
            },
            error: function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong. Please try again later.");
            }
        },
        columns: [
            { data: "LocationId", name: "l.intLocationId", width: "5%" },
            { data: "LocationName", name: "l.strLocation", width: "75%" },
            { data: null, orderable: false, searchable: false, width: "20%" }
        ],
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    return `<input type="checkbox" class="chk-location" id="chk-location-${data["LocationId"]}" name="chk-location-${data["LocationId"]}" value="${data["LocationId"]}">`;
                }
            },
            //{
            //    targets: 2,
            //    className: "text-center align-middle",
            //    render: function (a, b, data, d) {
            //        const locId = data["LocationId"];
            //        const isAssigned = data["Assigned"];

            //        return `
            //    <div class="d-flex justify-content-center align-items-center">
            //        <div class="dropdown">
            //            <button class="btn btn-sm  border-0 p-2 rounded-circle" type="button"
            //                    id="actionDropdown-${locId}" data-bs-toggle="dropdown" aria-expanded="false"
            //                    style="width: 2.5rem; height: 2.5rem;">
            //                <i class="fa fa-ellipsis-v"></i>
            //            </button>
            //            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${locId}">
            //                <li>
            //                    <a class="dropdown-item" href="#" id="${isAssigned ? `remove-all-loc-${locId}` : `assign-all-loc-${locId}`}"
            //                       onclick="autoAllocateModuleHandler.${isAssigned ? 'removeAutoAssignmemtFromLocationInOrganisation' : 'assignToEntireLocation'}(this)">
            //                        <i class="fa fa-fw ${isAssigned ? 'fa-times' : 'fa-plus-circle'} me-2"></i>
            //                        ${isAssigned ? 'Remove from Auto' : 'Set for Auto'}
            //                    </a>
            //                </li>
            //            </ul>
            //        </div>
            //    </div>`;
            //    }
            //}
            {
                targets: 2,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    const locId = data["LocationId"];
                    const isAssigned = data["Assigned"];
                    const iconClass = isAssigned ? "fa-times" : "fa-plus-circle";
                    const btnText = isAssigned ? "Remove from Auto" : "Set for Auto";
                    const actionFn = isAssigned ? "removeAutoAssignmemtFromLocationInOrganisation" : "assignToEntireLocation";

                    return `
            <button type="button"  data-toggle="tooltip" title = "${btnText}"
                    class="btn btn-sm  border-0 p-2 rounded-circle"
                    id="${isAssigned ? `remove-all-loc-${locId}` : `assign-all-loc-${locId}`}"
                    onclick="autoAllocateModuleHandler.${actionFn}(this)">
                <i class="fa fa-fw ${iconClass} me-1"></i> 
            </button>
        `;
                }
            }

        ],
        drawCallback: function (settings) {
            autoAllocateModuleHandler.selectAllLocationRows();
        },
        order: [[1, 'asc']]
    });
    
    var allDepartmentsTable = $('#autoAllocateModuleAllDepartmentList').DataTable({
        lengthChange: false,
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Licenses/LoadAllDepartmentForLicenseAutoAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Course = $ddlCourse.val();
            },
            error: function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong. Please try again later.");
            }
        },
        columns: [
            { data: "DepartmentId", name: "d.intDepartmentID", width: "5%" },
            { data: "DepartmentName", name: "d.strDepartment", width: "75%" },
            { data: null, orderable: false, searchable: false, width: "20%" }
        ],
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    return `<input type="checkbox" class="chk-dep" id="chk-dep-${data["DepartmentId"]}" name="chk-dep-${data["DepartmentId"]}" value="${data["DepartmentId"]}">`;
                }
            },
            //{
            //    targets: 2,
            //    className: "text-center align-middle",
            //    render: function (a, b, data, d) {
            //        const depId = data["DepartmentId"];
            //        const isAssigned = data["Assigned"];

            //        return `
            //    <div class="d-flex justify-content-center align-items-center">
            //        <div class="dropdown">
            //            <button class="btn btn-sm  border-0 p-2 rounded-circle" type="button"
            //                    id="actionDropdown-${depId}" data-bs-toggle="dropdown" aria-expanded="false"
            //                    style="width: 2.5rem; height: 2.5rem;">
            //                <i class="fa fa-ellipsis-v"></i>
            //            </button>
            //            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${depId}">
            //                <li>
            //                    <a class="dropdown-item" href="#" id="${isAssigned ? `remove-all-dep-${depId}` : `assign-all-dep-${depId}`}"
            //                       onclick="autoAllocateModuleHandler.${isAssigned ? 'removeAutoAssignmemtFromDepartmentInOrganisation' : 'assignToEntireDepartment'}(this)">
            //                        <i class="fa fa-fw ${isAssigned ? 'fa-times' : 'fa-plus-circle'} me-2"></i>
            //                        ${isAssigned ? 'Remove from Auto' : 'Set for Auto'}
            //                    </a>
            //                </li>
            //            </ul>
            //        </div>
            //    </div>`;
            //    }
            //}
            {
                targets: 2,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    const depId = data["DepartmentId"];
                    const isAssigned = data["Assigned"];
                    const iconClass = isAssigned ? "fa-times" : "fa-plus-circle";
                    const btnText = isAssigned ? "Remove from Auto" : "Set for Auto";
                    const actionFn = isAssigned ? "removeAutoAssignmemtFromDepartmentInOrganisation" : "assignToEntireDepartment";

                    return `
            <button type="button"  data-toggle="tooltip" title = "${btnText}"
                    class="btn btn-sm border-0 p-2 rounded-circle"
                    id="${isAssigned ? `remove-all-dep-${depId}` : `assign-all-dep-${depId}`}"
                    onclick="autoAllocateModuleHandler.${actionFn}(this)">
                <i class="fa fa-fw ${iconClass} me-1"></i>
            </button>
        `;
                }
            }
        ],
        drawCallback: function (settings) {
            autoAllocateModuleHandler.selectAllDepartmentRows();
        },
        order: [[1, 'asc']]
    });


    //allocate to entire organisation
    $allocateEntireOrgBtn.click(function (e) {
        e.preventDefault();
        assignToEntireOrg();
    });

    ////allocate to entire department
    //$allocateEntireDepartmentBtn.click(function (e) {
    //    e.preventDefault();
    //    var selectedDepartment = $ddlDepartment.val();
    //    if (selectedDepartment > 0)
    //        assignToEntireDepartment();
    //    else {
    //        UTILS.Alert.show($alert, 'warning', 'Please select a department to set auto license allocation for a department.');
    //        return false;
    //    }
    //});


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

    //function to remove department from auto allocation of licenses --advanced option
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

    // remove auto assingment from location
    function removeAutoAssignmemtFromLocationInOrganisation(btn) {
        if (confirm("Are you sure you want to remove auto assignment of license(s) to future user(s) from this Location?")) {
            var locationId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Licenses/RemoveLicenseAutoAllocationFromLocationInOrg'
            var data = {
                Course: $ddlCourse.val(),
                Locations: locationId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Auto assignment of licenses removed from the location successfully.')
                    allLocationsTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove auto assignment of licenses from the location.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to remove all department in all location of an organisation from auto allocation of licenses
    function removeAutoAssignmemtFromDepartmentInOrganisation(btn) {
        if (confirm("Are you sure you want to remove auto assignment of license(s) to future user(s) from this department?")) {
            var depId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Licenses/RemoveLicenseAutoAllocationForDepartmentInOrg'
            var data = {
                Course: $ddlCourse.val(),
                Departments: depId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Auto assignment of licenses removed from the department successfully.')
                    allDepartmentsTable.draw();
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

    // function to set auto allocation for all locations in the organisation
    function assignToEntireOrg() {
        if (parseInt($ddlCourse.val()) > 0 ) {
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
    function assignToEntireLocation(btn) {
        var locationId = btn.id.split('-').pop();
        if (parseInt($ddlCourse.val()) > 0 ) {
            if (confirm("Are you sure you want to set auto allocation for selected location?")) {
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForLocationInOrganisation'
                var data = {
                    allSelected: false,
                    selectedLocationList: [locationId],
                    unselectedLocationList: [],
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later')
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a course to auto allocate licences');
            resetSelections();
        }

    }

    // function to set auto allocation for selected department in the organisation
    function assignToEntireDepartment(btn) {
        var departmentId = btn.id.split('-').pop();
        if (parseInt($ddlCourse.val()) > 0 ) {
            if (confirm("Are you sure you want to set auto allocation for selected department in entire company?")) {
                UTILS.disableButton($allocateEntireDepartmentBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForEntireOrgDepartment'
                var data = {
                    Course: $ddlCourse.val(),
                    Department: departmentId
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        allDepartmentsTable.draw();
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

    // function to select all checkbox click - for location table
    $selectAllLocationChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllLocations = this.checked;

        if (this.checked) {
            selEntireLocations = [];
            unselEntireLocations = [];
            $allocateAllLocationBtn.removeClass('disabled');
            $allocateAllLocationBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Set auto to all selected locations");
        } else {
            $allocateAllLocationBtn.addClass('disabled');
            $allocateAllLocationBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Select some locations");
        }
        autoAllocateModuleHandler.selectAllLocationRows();
    });

    // function to select all checkbox click - for department table
    $selectAllDeptChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllDepartments = this.checked;

        if (this.checked) {
            selEntireDepartments = [];
            unselEntireDepartments = [];
            $allocateAllDeptBtn.removeClass('disabled');
            $allocateAllDeptBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelectedDepartments()");
            $allocateAllDeptBtn.attr("title", "Set auto to all selected locations");
        } else {
            $allocateAllDeptBtn.addClass('disabled');
            $allocateAllDeptBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelectedDepartments()");
            $allocateAllDeptBtn.attr("title", "Select some departments");
        }
        autoAllocateModuleHandler.selectAllDepartmentRows();
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

    // function to render rows on page change -- location table
    function selectAllLocationRows() {
        // Get all rows with search applied
        var rows = allLocationsTable.rows({ 'search': 'applied' }).nodes();

        // Check/uncheck checkboxes for all rows in the table
        $('input[type="checkbox"]', rows).prop('checked', $selectAllLocationChkBox.prop('checked'));

        //check each location and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-location').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selEntireLocations.length > 0) {
                for (i = 0; i < selEntireLocations.length; i++) {
                    if (selEntireLocations[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unselEntireLocations.length > 0) {
                for (j = 0; j < unselEntireLocations.length; j++) {
                    if (unselEntireLocations[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }

    // function to render rows on page change -- location table
    function selectAllDepartmentRows() {
        // Get all rows with search applied
        var rows = allDepartmentsTable.rows({ 'search': 'applied' }).nodes();

        // Check/uncheck checkboxes for all rows in the table
        $('input[type="checkbox"]', rows).prop('checked', $selectAllDeptChkBox.prop('checked'));

        //check each location and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-dep').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selEntireDepartments.length > 0) {
                for (i = 0; i < selEntireDepartments.length; i++) {
                    if (selEntireDepartments[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unselEntireDepartments.length > 0) {
                for (j = 0; j < unselEntireDepartments.length; j++) {
                    if (unselEntireDepartments[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }

    // function to check department checkbox click
    $('#autoAllocateModuleDepartmentList').on('draw.dt', function () {
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
    });

    // function to check all locations checkbox click
    $('#autoAllocateModuleAllLocationList').on('draw.dt', function () {
        $('#autoAllocateModuleAllLocationList tbody').on('change', 'input[type="checkbox"]', function () {
            // If checkbox is not checked
            if (!this.checked) {
                var el = $selectAllLocationChkBox.get(0);
                // If "Select all" control is checked and has 'indeterminate' property
                if (el && el.checked && ('indeterminate' in el)) {
                    el.indeterminate = true;
                }
            }

            // push values in selected or unselected arrays respectively to maintain state
            var id = this.id.split('-').pop();

            //push in selected array
            if (this.checked) {
                selEntireLocations.push(id);
                // enable allocate to all button
                $allocateAllLocationBtn.removeClass('disabled');
                $allocateAllLocationBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelectedLocations()");
                $allocateAllLocationBtn.attr("title", "Allocate to all selected locations");

                // remove id if exist in unselected array
                if (unselEntireLocations.length > 0) {
                    for (i = 0; i < unselEntireLocations.length; i++) {
                        if (unselEntireLocations[i] == id) {
                            unselEntireLocations.splice(i, 1);
                            break;
                        }
                    }
                }
            }
            else {

                //push in unselected array
                unselEntireLocations.push(id);

                // remove id if exist in selected array
                if (selEntireLocations.length > 0) {
                    for (i = 0; i < selEntireLocations.length; i++) {
                        if (selEntireLocations[i] == id) {
                            selEntireLocations.splice(i, 1);
                            break;
                        }
                    }
                }
            }
        });
    });

    // function to check all departments checkbox click
    $('#autoAllocateModuleAllDepartmentList').on('draw.dt', function () {
        $('#autoAllocateModuleAllDepartmentList tbody').on('change', 'input[type="checkbox"]', function () {
            // If checkbox is not checked
            if (!this.checked) {
                var el = $selectAllDeptChkBox.get(0);
                // If "Select all" control is checked and has 'indeterminate' property
                if (el && el.checked && ('indeterminate' in el)) {
                    el.indeterminate = true;
                }
            }

            // push values in selected or unselected arrays respectively to maintain state
            var id = this.id.split('-').pop();

            //push in selected array
            if (this.checked) {
                selEntireDepartments.push(id);
                // enable allocate to all button
                $allocateAllDeptBtn.removeClass('disabled');
                $allocateAllDeptBtn.attr("onclick", "autoAllocateModuleHandler.assignToSelectedDepartments()");
                $allocateAllDeptBtn.attr("title", "Allocate to all selected departments");

                // remove id if exist in unselected array
                if (unselEntireDepartments.length > 0) {
                    for (i = 0; i < unselEntireDepartments.length; i++) {
                        if (unselEntireDepartments[i] == id) {
                            unselEntireDepartments.splice(i, 1);
                            break;
                        }
                    }
                }
            }
            else {

                //push in unselected array
                unselEntireDepartments.push(id);

                // remove id if exist in selected array
                if (selEntireDepartments.length > 0) {
                    for (i = 0; i < selEntireDepartments.length; i++) {
                        if (selEntireDepartments[i] == id) {
                            selEntireDepartments.splice(i, 1);
                            break;
                        }
                    }
                }
            }
        });
    });

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

    // function to bulk assign from assign to selected button for locations
    function assignToSelectedLocations() {
        if (selEntireLocations.length > 0 || selectAllLocations) {
            if (confirm("Are you sure you want to set auto allocation for selected locations?")) {
                UTILS.disableButton($allocateAllLocationBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForLocationInOrganisation'
                var data = {
                    allSelected: selectAllLocations,
                    selectedLocationList: selEntireLocations,
                    unselectedLocationList: unselEntireLocations,
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                    UTILS.resetButton($allocateAllLocationBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later')
                        UTILS.resetButton($allocateAllLocationBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select location(s) to auto allocate course');
            resetSelections()
            ////disable allocate to all buton
            //$allocateAllBtn.addClass('disabled');
            //$allocateAllBtn.removeAttr("onclick", "autoAllocateModuleHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }

    // function to bulk assign from assign to selected button for departments
    function assignToSelectedDepartments() {
        if (selEntireDepartments.length > 0 || selectAllDepartments) {
            if (confirm("Are you sure you want to set auto allocation for selected departments?")) {
                UTILS.disableButton($allocateAllDeptBtn);
                var url = hdnBaseUrl + 'Licenses/SetLicenseAutoAllocationForAllDepartmentsInOrganisation'
                var data = {
                    allSelected: selectAllDepartments,
                    selectedDepartmentList: selEntireDepartments,
                    unselectedDepartmentList: unselEntireDepartments,
                    Course: $ddlCourse.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Auto allocation set successfully.')
                        resetSelections();
                        allDepartmentsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation.')
                    UTILS.resetButton($allocateAllDeptBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to set auto allocation. Please try again later');
                        UTILS.resetButton($allocateAllDeptBtn);
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

    return {
        init: init,
        removeAutoAssignmemtFromLocation: removeAutoAssignmemtFromLocation,
        removeAutoAssignmemt: removeAutoAssignmemt,
        setForAutoAssignment: setForAutoAssignment,
        setForAutoAssignmentToLocation: setForAutoAssignmentToLocation,
        selectAllRows: selectAllRows,
        assignToSelected: assignToSelected,
        assignToEntireOrg: assignToEntireOrg,
        assignToEntireDepartment: assignToEntireDepartment,
        removeAutoAssignmemtFromDepartmentInOrganisation: removeAutoAssignmemtFromDepartmentInOrganisation,
        removeAutoAssignmemtFromLocationInOrganisation: removeAutoAssignmemtFromLocationInOrganisation,
        selectAllLocationRows: selectAllLocationRows,
        selectAllDepartmentRows: selectAllDepartmentRows,
        assignToSelectedLocations: assignToSelectedLocations,
        assignToSelectedDepartments: assignToSelectedDepartments,
        assignToEntireLocation: assignToEntireLocation
    }
})();