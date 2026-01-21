$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-document-accesss');
        allocateIndividalDocumentGroupHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});


var allocateIndividalDocumentGroupHandler = (function () {
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $searchBtn = $('#searchAllocateDocument')
    var $clearSearchBtn = $('#clearSearchAllocateDocument')
    var $allocateAllBtn = $('#btnAllocateToSelected')
    var $selectAllChkBox = $('#chk_slc_all_user_dia');
    var $alert = $('#divMessage_AllocateDocument');
    var $reportContainer = $("#div-report-container");
    function showDefaultMessage() {
        $reportContainer.hide();
        $('#registeredAdminLearnerList').DataTable().destroy();
        var message = '<div > <b>How to use this page:</b> <ul>' +
            '<li>Use the input field to search for a learner by name or email.</li>' +
            '<li>Select a location and department from the dropdowns to narrow your search.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> Search to view matching learners.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li>' +
            '<li>Use the checkboxes to select one or more learners from the list.</li>' +
            '<li>Click <b>Allocate to Selected</b> to assign the document group to selected learners.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alert, 'default', message);
    }

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        return (results[1]);
    }

    var selectedGroupId = 0;
    var selUsers = [];
    var unSelUsers = [];
    var selectAllUsers = false;

    function resetSelections() {
        selUsers = [];
        unSelUsers = [];
        selectAllUsers = false;
        $selectAllChkBox.prop('checked', false);
        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick", "allocateIndividalDocumentGroupHandler.assignToSelected()");
        $allocateAllBtn.attr("title", "Select some learners");
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $reportContainer.show();
        $('#allocateIndividualDocumentLearnerList').DataTable().destroy();
        $('#allocateIndividualDocumentLearnerList').DataTable({
            processing: true,
            serverSide: true,
            filter: false,
            orderMulti: true,
            stateSave: true,
            lengthChange: false,
            language: {
                processing: `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>`,
                emptyTable: "No record(s) found."
            },
            ajax: {
                url: hdnBaseUrl + "Document/LoadLearnerDataToAllocateIndividalDocumentGroup",
                type: "POST",
                datatype: "json",
                data: function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $.urlParam('id');
                },
                error: function (xhr, error, code) {
                    console.error(xhr);
                    alert("Oops! Something went wrong. Please try again later.");
                }
            },
            drawCallback: function () {
                allocateIndividalDocumentGroupHandler.selectAllRows();

                // Re-initialize tooltips
                const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                tooltipTriggerList.map(function (tooltipTriggerEl) {
                    return new bootstrap.Tooltip(tooltipTriggerEl);
                });
            },
            columns: [
                { data: "UserID", name: "c.UserID", width: "5%" },
                { data: "FirstName", name: "c.strFirstName", width: "20%" },
                { data: "EmailId", name: "c.strEmail", width: "25%" },
                { data: "Location", name: "l.strLocation", width: "20%" },
                { data: "Department", name: "d.strDepartment", width: "20%" },
                { data: null, orderable: false, searchable: false, width: "10%" }
            ],
            columnDefs: [
                {
                    targets: 0,
                    orderable: false,
                    className: "text-center",
                    render: function (data, type, row) {
                        const safeId = $('<div/>').text(data).html();
                        return `<input type="checkbox" class="form-check-input chk-user" id="chk-user-${row.UserID}" name="chk-user-${row.UserID}" value="${safeId}">`;
                    }
                },
                {
                    targets: 1,
                    render: function (data, type, row) {
                        return `<span>${row.FirstName} ${row.LastName}</span>`;
                    }
                },
                {
                    targets: 5,
                    className: "text-center",
                    render: function (data, type, row) {
                        const id = row.UserID;
                        const isExpired = row.IsCourseExpired === true || row.IsCourseExpired === 'true' || row.IsCourseExpired === 1 || row.IsCourseExpired === '1';

                        return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            <li>
                                <a class="dropdown-item" href="#" id="assign-access-${id}"
                                    onclick="allocateIndividalDocumentGroupHandler.assignIndividualDocumentGroupAccess(this)">
                                    <i class="fa fa-plus-circle me-2"></i>Assign Access
                                </a>
                            </li>
                        </ul>
                    </div>
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
    function init() {
        UTILS.Alert.hide($alert)
        selectedGroupId = $.urlParam('id');
        var docName = decodeURIComponent($.urlParam('doc'));
        $("#spnUpdateDocumentName").html(docName);
        renderLocationDropDown();
        renderDepartmentDropDown(0);
        showDefaultMessage();
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

    //function to allocate Document to single learner and refresh table
    function assignIndividualDocumentGroupAccess(btn) {
        var grpName = decodeURIComponent($.urlParam('group'));
        if (confirm("Are you sure you want to assign " + grpName +" to this user?")) {
            var learnerId = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'Document/AllocateIndividualDocumentGroupToLearner?groupID=' + selectedGroupId
            var data = {
                LearnerID: learnerId
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Document group assigned successfully.')
                    $('#allocateIndividualDocumentLearnerList').DataTable().draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to assign document group.')
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
            $allocateAllBtn.attr("onclick", "allocateIndividalDocumentGroupHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Allocate to all selected learners");
        } else {
            $allocateAllBtn.addClass('disabled');
            $allocateAllBtn.removeAttr("onclick", "allocateIndividalDocumentGroupHandler.assignToSelected()");
            $allocateAllBtn.attr("title", "Select some learners");
        }
        allocateIndividalDocumentGroupHandler.selectAllRows();
    });

    // function to check learner checkbox click
    $('#allocateIndividualDocumentLearnerList').on('draw.dt', function () {
        $('#allocateIndividualDocumentLearnerList tbody').on('change', 'input[type="checkbox"]', function () {
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
                $allocateAllBtn.attr("onclick", "allocateIndividalDocumentGroupHandler.assignToSelected()");
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

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = $('#allocateIndividualDocumentLearnerList').DataTable().rows({ 'search': 'applied' }).nodes();

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

            var groupName = decodeURIComponent($.urlParam('group'));
            if (confirm("Are you sure you want to assign " + groupName+" access to all selected users?")) {
                // allocate functionality
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + "Document/AllocateIndividualDocumentGroupToLearner_Multiple?groupID=" + selectedGroupId;
                var data = {
                    allSelected: selectAllUsers,
                    selectedUserList: selUsers,
                    unselectedUserList: unSelUsers,
                    SearchText: $learner.val(),
                    Location: $ddlLoc.val(),
                    Department: $ddlDep.val()
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Document assigned successfully.')
                        resetSelections()
                        $('#allocateIndividualDocumentLearnerList').DataTable().draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to assign documents.')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to assign document. Please try again later');
                    UTILS.resetButton($allocateAllBtn);
                })
                
            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select learner(s) to allocate course');
            resetSelections();
        }
        
    }

    return {
        init: init,
        assignIndividualDocumentAccess: assignIndividualDocumentAccess,
        renderLocationDropDown: renderLocationDropDown,
        renderDepartmentDropDown: renderDepartmentDropDown,
        selectAllRows: selectAllRows,
        assignToSelected: assignToSelected
    }
})();