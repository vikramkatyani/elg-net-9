$(function () {
    UTILS.activateNavigationLink('collapseDocManage');
    UTILS.activateMenuNavigationLink('menu-document-access');
    $('[data-toggle="tooltip"]').tooltip();
    allocateCategoryLocationHandler.init();
    $(".assign-sel-row").hide();
    $(".type-table-container").hide();
    $('#applyAssignmentMethod').hide();
});

var editCatID = 0;

var allocateCategoryLocationHandler = (function () {


    var $initialInfoMessage = "";
    var $locSelInfoMessage = "";
    var $compSelInfoMessage = "";

    $initialInfoMessage = "Simply select your category, then decide if you wish to assign the category to a specific location or your entire workforce.<br/> ";
    $locSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Location:</b> this will include ALL staff throughout that location and any departments within it. Click on \"Continue\" button and select location from the table below to perform this action.";
    $compSelInfoMessage = "<span class='fa fa-info-circle'></span> <b>Entire Company:</b> this will include ALL staff throughout your entire company structure. Click on the \"Entire Company\" button to perform this action.";
   
    var $selAssignMethod = $("#ddlMethod");
    var $infoAlert = $('#divCatAllocateInfoMessage');
    var $ddlLoc = $('#ddlLocation');
    var $searchBtn = $('#searchCategoryLocation');
    var $applyAssignmentMethodBtn = $('#applyAssignmentMethod');
    var $clearSearchBtn = $('#clearSearchCategoryLocation');
    var $alert = $('#divMessage_allocateCat');

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
            case 2: $infoAlert.html($locSelInfoMessage);
                $allLocationTableContainer.show();
                $applyAssignmentMethodBtn.show();
                allLocationsTable.draw();
                break;
            case 4: $infoAlert.html($compSelInfoMessage);
                $('#filter-assignby-org').show();
                break;
            default: $infoAlert.html($initialInfoMessage);
                break;
        }
    });

    //apply assign method
    $applyAssignmentMethodBtn.click(function (e) {
        e.preventDefault();
        var selMethod = $selAssignMethod.val();
        switch (parseInt(selMethod)) {
            case 2: allLocationsTable.draw();
                UTILS.Alert.hide($alert);
                break;
        }
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        selectedLocation = $ddlLoc.val();
        if (selectedLocation != 0 && editCatID != 0)
            allLocationsTable.draw();
        else
            UTILS.Alert.show($alert,'warning', 'Please select a Document Category and a Location to map.')
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert)
        $ddlLoc.val('0');
        //$ddlDocument.val('0');
        //selectedDocument = 0;
        resetSelections();
        allLocationsTable.draw();
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
        $allocateAllBtn.removeAttr("onclick", "allocateCategoryLocationHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some depatments");

        $allocateAllLocationBtn.addClass('disabled');
        $allocateAllLocationBtn.removeAttr("onclick", "allocateCategoryLocationHandler.assignToSelectedLocations()");
        $allocateAllLocationBtn.attr("title", "Select some locations");

        $allocateAllDeptBtn.addClass('disabled');
        $allocateAllDeptBtn.removeAttr("onclick", "allocateCategoryLocationHandler.assignToSelectedDepartments()");
        $allocateAllDeptBtn.attr("title", "Select some depatments");
    }
            
    var allLocationsTable = $('#allocateCatAllLocationList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": hdnBaseUrl + "Document/LoadAllLocationsForCategoryAllocation",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.Course = editCatID;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "LocationId", "name": "l.intLocationId", "autoWidth": true },
            { "data": "LocationName", "name": "l.strLocation", "autoWidth": true }
        ],
        "drawCallback": function (settings) {
            allocateCategoryLocationHandler.selectAllLocationRows();
        },
        columnDefs: [{
            // render checkbox in first column
            orderable: false,
            targets: [0], render: function (a, b, data, d) {
                return '<input type="checkbox" class="chk-location" id="chk-location-' + data["LocationId"] + '" name="chk-location-' + data["LocationId"] + '" value="' + $('<div/>').text(data).html() + '">';
            }
        },
        {
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                if (data["Assigned"]) {
                    return '<button type="button"id="remove-all-loc-' + data["LocationId"] + '" class="btn btn-sm btn-secondary mb-1" onclick="allocateCategoryLocationHandler.removeCatAllocationFromLocationInOrganisation(this)"><i class="fa fa-fw fa-times"></i><span>Remove Allocation</span></button> '
                } else {
                    return '<button type="button"id="assign-all-loc-' + data["LocationId"] + '" class="btn btn-sm btn-dark mb-1" onclick="allocateCategoryLocationHandler.assignToEntireLocation(this)"><i class="fa fa-fw fa-plus-circle"></i><span>Allocate</span></button> '
                }
            }
        }],
        'order': [[1, 'asc']]
    });
    
    //allocate to entire organisation
    $allocateEntireOrgBtn.click(function (e) {
        e.preventDefault();
        assignToEntireOrg();
    });

    function init() {
        UTILS.Alert.hide($alert)
        //renderDocumentDropDown();
        renderLocationDropDown();
        editCatID = $.urlParam('id');
        var catName = decodeURIComponent($.urlParam('category'));
        $("#spnUpdateCategoryName").html(catName);
    }

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
    
    // remove auto assingment from location
    function removeCatAllocationFromLocationInOrganisation(btn) {
        if (confirm("Are you sure you want to remove document allocation from this Location?")) {
            var locationId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/RemoveCategoryAllocationFromLocationInOrg'
            var data = {
                Document: editCatID,
                Locations: locationId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document category allocation removed from the location successfully.')
                    allLocationsTable.draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to remove document category allocation from the location.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    // function to set auto allocation for all locations in the organisation
    function assignToEntireOrg() {
        if (parseInt(editCatID) > 0 ) {
            if (confirm("Are you sure you want to allocate document category for entire company?")) {
                UTILS.disableButton($allocateEntireOrgBtn);
                var url = hdnBaseUrl + 'Document/SetDocumentCategoryAllocationForEntireOrg'
                var data = {
                    Course: editCatID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document category allocated successfully.')
                        resetSelections();
                        //allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category.')
                    UTILS.resetButton($allocateEntireOrgBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category. Please try again later')
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
        if (parseInt(editCatID) > 0 ) {
            if (confirm("Are you sure you want to allocate category to selected location?")) {
                var url = hdnBaseUrl + 'Document/SetCategoryAllocationForLocationInOrganisation'
                var data = {
                    allSelected: false,
                    selectedLocationList: [locationId],
                    unselectedLocationList: [],
                    Document: editCatID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document Category allocated successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category.')
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category. Please try again later')
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select a document category');
            resetSelections();
        }

    }
    
    // function to select all checkbox click - for location table
    $selectAllLocationChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllLocations = this.checked;

        if (this.checked) {
            selEntireLocations = [];
            unselEntireLocations = [];
            $allocateAllLocationBtn.removeClass('disabled');
            $allocateAllLocationBtn.attr("onclick", "allocateCategoryLocationHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Set auto to all selected locations");
        } else {
            $allocateAllLocationBtn.addClass('disabled');
            $allocateAllLocationBtn.removeAttr("onclick", "allocateCategoryLocationHandler.assignToSelectedLocations()");
            $allocateAllLocationBtn.attr("title", "Select some locations");
        }
        allocateCategoryLocationHandler.selectAllLocationRows();
    });

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
            $allocateAllLocationBtn.attr("onclick", "allocateCategoryLocationHandler.assignToSelectedLocations()");
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
    // function to bulk assign from assign to selected button for locations
    function assignToSelectedLocations() {
        if (selEntireLocations.length > 0 || selectAllLocations) {
            if (confirm("Are you sure you want to allocate document for selected locations?")) {
                UTILS.disableButton($allocateAllLocationBtn);
                var url = hdnBaseUrl + 'Document/SetCategoryAllocationForMultipleLocationInOrganisation'
                var data = {
                    allSelected: selectAllLocations,
                    selectedLocationList: selEntireLocations,
                    unselectedLocationList: unselEntireLocations,
                    Document: editCatID
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document Category allocated successfully.')
                        resetSelections();
                        allLocationsTable.draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category.')
                    UTILS.resetButton($allocateAllLocationBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to allocate document category. Please try again later')
                        UTILS.resetButton($allocateAllLocationBtn);
                });

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select location(s) to allocate document category');
            resetSelections()
            ////disable allocate to all buton
            //$allocateAllBtn.addClass('disabled');
            //$allocateAllBtn.removeAttr("onclick", "allocateCategoryLocationHandler.assignToSelected()");
            //$allocateAllBtn.attr("title", "Select some departments");
        }

    }
    
    return {
        init: init,
        assignToEntireOrg: assignToEntireOrg,
        selectAllLocationRows: selectAllLocationRows,
        removeCatAllocationFromLocationInOrganisation: removeCatAllocationFromLocationInOrganisation,
        assignToSelectedLocations: assignToSelectedLocations,
        assignToEntireLocation: assignToEntireLocation
    }
})();