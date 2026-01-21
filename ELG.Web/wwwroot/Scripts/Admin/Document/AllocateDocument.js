$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-document-accesss');
    $('[data-toggle="tooltip"]').tooltip();
    allocateDocHandler.init();
    $(".assign-sel-row").hide();
    $(".type-table-container").hide();
    $('#applyAssignmentMethod').hide();
});

var editDocumentID = 0;

var allocateDocHandler = (function () {


    var $initialInfoMessage = "";
    var $advancedSelInfoMessage = "";
    var $locSelInfoMessage = "";
    var $depSelInfoMessage = "";
    var $compSelInfoMessage = "";
    var $noteAutoLicence = ""

    $initialInfoMessage = "Start by selecting a document, then choose whether to assign it by location, department, or across your entire organization.<br/>";
    $advancedSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Advanced:</b> Choose a location from the dropdown and click <strong>Search</strong> to assign the document to specific departments within that location only.";
    $locSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Location:</b> This option assigns the document to all staff within the selected location, including all departments it contains. Click <strong>Continue</strong> and select a location from the table below to proceed.";
    $depSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Department:</b> This assigns the document to all departments with the same name across your entire organization, regardless of location. Click <strong>Continue</strong> and choose a department from the table below to proceed.";
    $compSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Entire Company:</b> This will assign the document to all staff across your entire organization. Click the <strong>Entire Company</strong> button to proceed.";
    $noteAutoLicence = ""

    var $selAssignMethod = $("#ddlMethod");
    var $infoAlert = $('#divDocAllocateInfoMessage');
    var $ddlDepartment = $('#ddlDepartment')
    var $ddlLoc = $('#ddlLocation')
    //var $ddlDocument = $('#ddlDocument')
    var $searchBtn = $('#searchDepartmentDoc')
    var $applyAssignmentMethodBtn = $('#applyAssignmentMethod')
    var $clearSearchBtn = $('#clearSearchDepartmentDoc')
    var $alert = $('#divMessage_allocateDoc');

    var $allocateEntireDepartmentBtn = $('#btnAssignToEntireDepartment')
    var $allocateEntireOrgBtn = $('#btnAssignToEntireOrg')
    var $allocateAllBtn = $('#btnAllocateToSelected')
    var $allocateAllLocationBtn = $('#btnAllocateToSelectedLocations')
    var $allocateAllDeptBtn = $('#btnAllocateToSelectedDepartments')
    var $selectAllChkBox = $('#chk_slc_all_departments');
    var $selectAllDeptChkBox = $('#chk_slc_all_company_departments');
    var $selectAllLocationChkBox = $('#chk_slc_all_locations');
    var $departmentTableContainer = $('#allocateDocDepartments_tbl_container');
    var $allLocationTableContainer = $('#allocateDocAllLocations_tbl_container');
    var $alldepartmentTableContainer = $('#allocateDocAllDepartments_tbl_container');

    var selDepartments = [];
    var unselDepartments = [];

    var selEntireDepartments = [];
    var unselEntireDepartments = [];
    var selEntireLocations = [];
    var unselEntireLocations = [];

    var selectAllUsers = false;
    var selectAllLocations = false;
    var selectAllDepartments = false;

    //var selectedDocument = 0;
    var selectedLocation = 0;

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        return (results[1]);
    }

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
        //if ($ddlDocument.val() <= 0) {
        //    UTILS.Alert.show($alert, 'warning', 'Please select a Document to set allocation.');
        //    return false;
        //}
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

        ////reset select users if course is changed
        //if (selectedDocument != $ddlDocument.val() || selectedLocation != $ddlLoc.val()) {
        //    resetSelections()
        //}
        //selectedDocument = $ddlDocument.val();
        selectedLocation = $ddlLoc.val();

        //if (selectedLocation != 0 && selectedDocument != 0)
        if (selectedLocation != 0 && editDocumentID != 0)
            departmentTable.draw();
        else
            UTILS.Alert.show($alert,'warning', 'Please select a Document and a Location to set allocation for a department.')
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $ddlLoc.val('0');
        //$ddlDocument.val('0');
        //selectedDocument = 0;
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
        $allocateAllBtn.removeAttr("onclick", "allocateDocHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some depatments");

        $allocateAllLocationBtn.addClass('disabled');
        $allocateAllLocationBtn.removeAttr("onclick", "allocateDocHandler.assignToSelectedLocations()");
        $allocateAllLocationBtn.attr("title", "Select some locations");

        $allocateAllDeptBtn.addClass('disabled');
        $allocateAllDeptBtn.removeAttr("onclick", "allocateDocHandler.assignToSelectedDepartments()");
        $allocateAllDeptBtn.attr("title", "Select some depatments");
    }

    var departmentTable = $('#allocateDocDepartmentList').DataTable({
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        lengthChange: false,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Document/LoadDepartmentForDocumentAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Location = $ddlLoc.val();
                data.Course = editDocumentID;
            },
            error: function (xhr, error, code) {
                console.error("DataTable error:", code, xhr);
                alert("Oops! Something went wrong. Please try again later.");
            }
        },
        columns: [
            { data: "DepartmentId", name: "d.DepartmentId", orderable: false, width: "5%" },
            { data: "DepartmentName", name: "d.strDepartment", width: "70%" },
            { data: null, orderable: false, searchable: false, width: "25%" }
        ],
        columnDefs: [
            {
                targets: 0,
                className: "text-center align-middle",
                render: function (data, type, row) {
                    return `
                    <input type="checkbox" class="form-check-input chk-department"
                        id="chk-department-${row.DepartmentId}"
                        name="chk-department-${row.DepartmentId}"
                        value="${row.DepartmentId}" />
                `;
                }
            },
            {
                targets: 2,
                className: "text-center align-middle",
                render: function (data, type, row) {
                    const id = row.DepartmentId;
                    const isAssigned = row.Assigned;

                    return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            ${isAssigned
                            ? `<li>
                                    <a class="dropdown-item text-danger" href="#" id="remove-dep-${id}"
                                       onclick="allocateDocHandler.removeDocumentAssignmemt(this)">
                                        <i class="fa fa-times me-2"></i>Remove Allocation
                                    </a>
                                   </li>`
                            : `<li>
                                    <a class="dropdown-item" href="#" id="assign-dep-${id}"
                                       onclick="allocateDocHandler.setForDocAllocation(this)">
                                        <i class="fa fa-plus-circle me-2"></i>Allocate
                                    </a>
                                   </li>`
                        }
                        </ul>
                    </div>
                `;
                }
            }
        ],
        order: [[1, 'asc']],
        drawCallback: function () {
            allocateDocHandler.selectAllRows();

            // Reinitialize tooltips if needed
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    });
        
    var allLocationsTable = $('#allocateDocAllLocationList').DataTable({
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        lengthChange: false,
        language: {
            processing: `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>`,
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Document/LoadAllLocationsForDocumentAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Course = editDocumentID;
            },
            error: function (xhr, error, code) {
                console.error(xhr);
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
                className: "text-center",
                render: function (data, type, row) {
                    const safeId = $('<div/>').text(data).html();
                    return `<input type="checkbox" class="form-check-input chk-location" id="chk-location-${row.LocationId}" name="chk-location-${row.LocationId}" value="${safeId}">`;
                }
            },
            {
                targets: 2,
                className: "text-center",
                render: function (data, type, row) {
                    const id = row.LocationId;
                    const isAssigned = row.Assigned;

                    return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            ${isAssigned
                            ? `<li>
                                    <a class="dropdown-item text-danger" href="#" id="remove-all-loc-${id}"
                                       onclick="allocateDocHandler.removeDocAllocationFromLocationInOrganisation(this)">
                                        <i class="fa fa-times me-2"></i>Remove Allocation
                                    </a>
                                   </li>`
                            : `<li>
                                    <a class="dropdown-item" href="#" id="assign-all-loc-${id}"
                                       onclick="allocateDocHandler.assignToEntireLocation(this)">
                                        <i class="fa fa-plus-circle me-2"></i>Allocate
                                    </a>
                                   </li>`
                        }
                        </ul>
                    </div>
                `;
                }
            }
        ],
        drawCallback: function (settings) {
            allocateDocHandler.selectAllLocationRows();

            // Re-initialize tooltips after draw
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        },
        order: [[1, 'asc']]
    });
    
    var allDepartmentsTable = $('#allocateDocAllDepartmentList').DataTable({
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        lengthChange: false,
        language: {
            processing: `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>`,
            emptyTable: "No record(s) found."
        },
        ajax: {
            url: hdnBaseUrl + "Document/LoadAllDepartmentForDocumentAllocation",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.Course = editDocumentID;
            },
            error: function (xhr, error, code) {
                console.error(xhr);
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
                className: "text-center",
                render: function (data, type, row) {
                    const safeId = $('<div/>').text(data).html();
                    return `<input type="checkbox" class="form-check-input chk-dep" id="chk-dep-${row.DepartmentId}" name="chk-dep-${row.DepartmentId}" value="${safeId}">`;
                }
            },
            {
                targets: 2,
                className: "text-center",
                render: function (data, type, row) {
                    const id = row.DepartmentId;
                    const isAssigned = row.Assigned;

                    return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            ${isAssigned
                            ? `<li>
                                    <a class="dropdown-item text-danger" href="#" id="remove-all-dep-${id}"
                                       onclick="allocateDocHandler.removeDocAssignmemtFromDepartmentInOrganisation(this)">
                                        <i class="fa fa-times me-2"></i>Remove Allocation
                                    </a>
                                   </li>`
                            : `<li>
                                    <a class="dropdown-item" href="#" id="assign-all-dep-${id}"
                                       onclick="allocateDocHandler.assignToEntireDepartment(this)">
                                        <i class="fa fa-plus-circle me-2"></i>Allocate
                                    </a>
                                   </li>`
                        }
                        </ul>
                    </div>
                `;
                }
            }
        ],
        drawCallback: function (settings) {
            allocateDocHandler.selectAllDepartmentRows();

            // Re-initialize tooltips after draw
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
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
    function init() {
        UTILS.Alert.hide($alert)
        //renderDocumentDropDown();
        renderLocationDropDown();
        renderDepartmentDropDown();
        editDocumentID = $.urlParam('id');
        var docName = decodeURIComponent($.urlParam('doc'));
        $("#spnUpdateDocumentName").html(docName);
    }

    ////function to render list of all locations in organisation
    //function renderDocumentDropDown() {
    //    $ddlDocument.empty();

    //    $ddlDocument.append($('<option/>', { value: '0', text: 'Select Document' }));
    //    UTILS.data.getAllDocuments(function (data) {
    //        populateList(data);
    //    })
    //}

    //function populateList(data) {
    //    if (data && data.docList != null) {
    //        $.each(data.docList, function (index, item) {
    //            $ddlDocument.append($('<option/>', {
    //                value: item.DocumentID,
    //                text: item.DocumentName
    //            }))
    //        });
    //    }
    //}

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
    function removeDocumentAssignmemt(btn) {
        if (confirm("Are you sure you want to remove document allocation from this department?")) {
            var departmentId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/RemoveDocumentAllocationForDepartment'
            var data = {
                Document: editDocumentID,
                Location: $ddlLoc.val(),
                Departments: departmentId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocation removed from the department successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove document allocation from the department.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    // remove auto assingment from location
    function removeDocAllocationFromLocationInOrganisation(btn) {
        if (confirm("Are you sure you want to remove document allocation from this Location?")) {
            var locationId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/RemoveDocumentAllocationFromLocationInOrg'
            var data = {
                Document: editDocumentID,
                Locations: locationId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocation removed from the location successfully.')
                    allLocationsTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove document allocation from the location.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to remove all department in all location of an organisation from auto allocation of licenses
    function removeDocAssignmemtFromDepartmentInOrganisation(btn) {
        if (confirm("Are you sure you want to remove document allocation from this department?")) {
            var depId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/RemoveDocumentAllocationForDepartmentInOrg'
            var data = {
                Document: editDocumentID,
                Departments: depId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocation removed from the department successfully.')
                    allDepartmentsTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove document allocation from the department.')
            }, function (status) {
                console.log(status);
            });
        }
    }


    //function to remove all department in location from auto allocation of licenses
    function removeDocumentAssignmemtFromLocation() {
        if (confirm("Are you sure you want to remove document allocaiton from this location?")) {
            var url = hdnBaseUrl + 'Document/RemoveDocumentAllocationFromAllDepartments'
            var data = {
                Document: editDocumentID,
                Location: $ddlLoc.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocation removed from the location successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove document allocation from the location.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to set department for auto allocation of licenses
    function setForDocAllocation(btn) {
        if (confirm("Are you sure you want to allocate document to this department?")) {
            var departmentId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/SetDocumentAllocationForDepartmentInLocation'
            var data = {
                Document: editDocumentID,
                Location: $ddlLoc.val(),
                Departments: departmentId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocated to the department successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    //function to set department for auto allocation of licenses
    function setForDocAllocationToLocation() {
        if (confirm("Are you sure you want to allocate document to this location?")) {
            var url = hdnBaseUrl + 'Document/SetDocumentAllocationForAllDepartments'
            var data = {
                Document: editDocumentID,
                Location: $ddlLoc.val()
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document allocated to the location successfully.')
                    departmentTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    // function to set auto allocation for all locations in the organisation
    function assignToEntireOrg() {
        if (parseInt(editDocumentID) > 0 ) {
            if (confirm("Are you sure you want to allocate document for entire company?")) {
                UTILS.disableButton($allocateEntireOrgBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForEntireOrg'
                var data = {
                    Course: editDocumentID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated successfully.')
                        resetSelections();
                        departmentTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                    UTILS.resetButton($allocateEntireOrgBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later')
                    UTILS.resetButton($allocateEntireOrgBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a document');
            resetSelections();
            UTILS.resetButton($allocateEntireOrgBtn);
        }

    }

    // function to set auto allocation for selected department in the organisation
    function assignToEntireLocation(btn) {
        var locationId = btn.id.split('-').pop();
        if (parseInt(editDocumentID) > 0 ) {
            if (confirm("Are you sure you want to allocate document for selected location?")) {
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForLocationInOrganisation'
                var data = {
                    allSelected: false,
                    selectedLocationList: [locationId],
                    unselectedLocationList: [],
                    Document: editDocumentID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later')
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a document');
            resetSelections();
        }

    }

    // function to set auto allocation for selected department in the organisation
    function assignToEntireDepartment(btn) {
        var departmentId = btn.id.split('-').pop();
        if (parseInt(editDocumentID) > 0 ) {
            if (confirm("Are you sure you want to allocate document for the department in entire company?")) {
                UTILS.disableButton($allocateEntireDepartmentBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForEntireOrgDepartment'
                var data = {
                    Course: editDocumentID,
                    Department: departmentId
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated successfully.')
                        resetSelections();
                        allDepartmentsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                    UTILS.resetButton($allocateEntireDepartmentBtn);
                }, function (status) {
                        console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later')
                        UTILS.resetButton($allocateEntireDepartmentBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a document');
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
            $allocateAllBtn.attr("onclick", "allocateDocHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Set auto to all selected departments");
        } else {
            $allocateAllBtn.addClass('disabled');
            $allocateAllBtn.removeAttr("onclick", "allocateDocHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Select some depatments");
        }
        allocateDocHandler.selectAllRows();
    });

    // function to select all checkbox click - for location table
    $selectAllLocationChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllLocations = this.checked;

        if (this.checked) {
            selEntireLocations = [];
            unselEntireLocations = [];
            $allocateAllLocationBtn.removeClass('disabled');
            $allocateAllLocationBtn.attr("onclick", "allocateDocHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Set auto to all selected locations");
        } else {
            $allocateAllLocationBtn.addClass('disabled');
            $allocateAllLocationBtn.removeAttr("onclick", "allocateDocHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Select some locations");
        }
        allocateDocHandler.selectAllLocationRows();
    });

    // function to select all checkbox click - for department table
    $selectAllDeptChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllDepartments = this.checked;

        if (this.checked) {
            selEntireDepartments = [];
            unselEntireDepartments = [];
            $allocateAllDeptBtn.removeClass('disabled');
            $allocateAllDeptBtn.attr("onclick", "allocateDocHandler.assignToSelectedDepartments()");
            $allocateAllDeptBtn.attr("title", "Set auto to all selected locations");
        } else {
            $allocateAllDeptBtn.addClass('disabled');
            $allocateAllDeptBtn.removeAttr("onclick", "allocateDocHandler.assignToSelectedDepartments()");
            $allocateAllDeptBtn.attr("title", "Select some departments");
        }
        allocateDocHandler.selectAllDepartmentRows();
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
    $('#allocateDocDepartmentList').on('draw.dt', function () {
        $('#allocateDocDepartmentList tbody').on('change', 'input[type="checkbox"]', function () {
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
                $allocateAllBtn.attr("onclick", "allocateDocHandler.assignToSelected()");
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
    $('#allocateDocAllLocationList').on('draw.dt', function () {
        $('#allocateDocAllLocationList tbody').on('change', 'input[type="checkbox"]', function () {
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
                $allocateAllLocationBtn.attr("onclick", "allocateDocHandler.assignToSelectedLocations()");
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
    $('#allocateDocAllDepartmentList').on('draw.dt', function () {
        $('#allocateDocAllDepartmentList tbody').on('change', 'input[type="checkbox"]', function () {
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
                $allocateAllDeptBtn.attr("onclick", "allocateDocHandler.assignToSelectedDepartments()");
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
            if (confirm("Are you sure you want to allocate document for selected departments?")) {
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForAllDepartments'
                var data = {
                    allSelected: selectAllUsers,
                    selectedDepartmentList: selDepartments,
                    unselectedDepartmentList: unselDepartments,
                    Location: $ddlLoc.val(),
                    Document: editDocumentID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated set successfully.')
                        resetSelections();
                        departmentTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later')
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
            //$allocateAllBtn.removeAttr("onclick", "allocateDocHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }

    // function to bulk assign from assign to selected button for locations
    function assignToSelectedLocations() {
        if (selEntireLocations.length > 0 || selectAllLocations) {
            if (confirm("Are you sure you want to allocate document for selected locations?")) {
                UTILS.disableButton($allocateAllLocationBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForMultipleLocationInOrganisation'
                var data = {
                    allSelected: selectAllLocations,
                    selectedLocationList: selEntireLocations,
                    unselectedLocationList: unselEntireLocations,
                    Document: editDocumentID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                    UTILS.resetButton($allocateAllLocationBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later')
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
            //$allocateAllBtn.removeAttr("onclick", "allocateDocHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }

    // function to bulk assign from assign to selected button for departments
    function assignToSelectedDepartments() {
        if (selEntireDepartments.length > 0 || selectAllDepartments) {
            if (confirm("Are you sure you want to allocated document for selected departments?")) {
                UTILS.disableButton($allocateAllDeptBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentAllocationForAllDepartmentsInOrganisation'
                var data = {
                    allSelected: selectAllDepartments,
                    selectedDepartmentList: selEntireDepartments,
                    unselectedDepartmentList: unselEntireDepartments,
                    Document: editDocumentID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document allocated successfully.')
                        resetSelections();
                        allDepartmentsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document.')
                    UTILS.resetButton($allocateAllDeptBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document. Please try again later');
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
            //$allocateAllBtn.removeAttr("onclick", "allocateDocHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }

    return {
        init: init,
        removeDocumentAssignmemtFromLocation: removeDocumentAssignmemtFromLocation,
        removeDocumentAssignmemt: removeDocumentAssignmemt,
        setForDocAllocation: setForDocAllocation,
        setForDocAllocationToLocation: setForDocAllocationToLocation,
        selectAllRows: selectAllRows,
        assignToSelected: assignToSelected,
        assignToEntireOrg: assignToEntireOrg,
        assignToEntireDepartment: assignToEntireDepartment,
        removeDocAssignmemtFromDepartmentInOrganisation: removeDocAssignmemtFromDepartmentInOrganisation,
        removeDocAllocationFromLocationInOrganisation: removeDocAllocationFromLocationInOrganisation,
        selectAllLocationRows: selectAllLocationRows,
        selectAllDepartmentRows: selectAllDepartmentRows,
        assignToSelectedLocations: assignToSelectedLocations,
        assignToSelectedDepartments: assignToSelectedDepartments,
        assignToEntireLocation: assignToEntireLocation
    }
})();