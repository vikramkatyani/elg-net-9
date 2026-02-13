$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-configure-module');
    refreshTrainingManualHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});


var refreshTrainingManualHandler = (function () {
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $searchBtn = $('#searchAllocatedModule')
    var $clearSearchBtn = $('#clearSearchAllocateModule')
    var $allocateAllBtn = $('#btnResetForSelected')
    var $selectAllChkBox = $('#chk_slc_all_user_rtm');
    var $alert = $('#divMessage_LearnerAssignedModule');
    var $reportContainer = $("#div-report-container")

    var selectedModule = 0;
    var selRecords = [];
    var unSelRecords = [];
    var selectAllRecords = false;

    function resetSelections() {
        selRecords = [];
        unSelRecords = [];
        selectAllRecords = false;
        $selectAllChkBox.prop('checked', false);
        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick");
        $allocateAllBtn.attr("title", "Select training records to reset");
    }
    function showDefaultMessage() {
        $reportContainer.hide();
        $('#learnerAssignedModuleList').DataTable().destroy();
        var message = '<div ><b>How to use this page:</b><ul>' +
            '<li>Use the <b>User</b> field to search by name or email.</li>' +
            '<li>Apply additional filters to narrow your search.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> <b>Search</b> to view assigned training records.</li>' +
            '<li>Select one or more users using the checkboxes in the first column.</li>' +
            '<li>Click <b>Reset Training</b> to reset the selected users’ training records.</li>' +
            '<li>To reset filters and start over, click the <i class="fa fa-times me-1"></i> <b>Clear</b> button.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alert, 'default', message);
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
        $('#learnerAssignedModuleList').DataTable().destroy();
        $('#learnerAssignedModuleList').DataTable({
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
                url: hdnBaseUrl + "CourseManagement/LoadLearnerAssignedModuleData",
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
                refreshTrainingManualHandler.selectAllRows();
            },
            columns: [
                { data: "UserID", name: "c.UserID", width: "5%" },
                { data: "FirstName", name: "c.strFirstName", width: "20%" },
                { data: "EmailId", name: "c.strEmail", width: "20%" },
                { data: "Location", name: "l.strLocation", width: "15%" },
                { data: "Department", name: "d.strDepartment", width: "15%" },
                { data: "CourseName", name: "co.strCourse", width: "15%" },
                { data: "AssignedOn", name: "pd.dateAssignedOn", width: "10%" },
                { data: "CourseStatus", name: "pd.strStatus", width: "10%" },
                { data: null, orderable: false, searchable: false, width: "5%" }
            ],
            columnDefs: [
                {
                    targets: 0,
                    orderable: false,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        return `<input type="checkbox" class="chk-user" id="chk-user-${data["ProgressRecordID"]}" name="chk-user-${data["ProgressRecordID"]}" value="${data["ProgressRecordID"]}">`;
                    }
                },
                {
                    targets: 1,
                    render: function (a, b, data, d) {
                        return `<span>${data["FirstName"]} ${data["LastName"]}</span>`;
                    }
                },
                {
                    targets: 7,
                    render: function (a, b, data, d) {
                        if (data["CourseStatus"] === "completed" || data["CourseStatus"] === "passed") {
                            return `<span>${data["CourseStatus"]}</span><br/><small class="text-muted">${data["CompletionDate"]}</small>`;
                        } else {
                            return `<span>${data["CourseStatus"]}</span>`;
                        }
                    }
                },
                //{
                //    targets: 8,
                //    className: "text-center align-middle",
                //    render: function (a, b, data, d) {
                //        const recordId = data["ProgressRecordID"];
                //        return `
                //<div class="d-flex justify-content-center align-items-center">
                //    <div class="dropdown">
                //        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                //                id="actionDropdown-${recordId}" data-bs-toggle="dropdown" aria-expanded="false"
                //                style="width: 2.5rem; height: 2.5rem;">
                //            <i class="fa fa-ellipsis-v"></i>
                //        </button>
                //        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${recordId}">
                //            <li>
                //                <a class="dropdown-item" href="#" id="assign-user-${recordId}"
                //                   onclick="refreshTrainingManualHandler.resetLearnerCourseRecord(this)">
                //                    <i class="fa fa-fw fa-sync me-2"></i>Reset
                //                </a>
                //            </li>
                //        </ul>
                //    </div>
                //</div>`;
                //    }
                //}
                {
                    targets: 8,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        const recordId = data["ProgressRecordID"];

                        return `
                            <button type="button"  data-toggle="tooltip" title = "Reset"
                                    class="btn btn-sm  border-0 p-2 rounded-circle"
                                    id="assign-user-${recordId}"
                                    onclick="refreshTrainingManualHandler.resetLearnerCourseRecord(this)">
                                <i class="fa fa-fw fa-refresh me-1"></i> 
                            </button>
                        `;
                    }
                }
            ],
            order: [[1, 'asc']]
        });
    });

    // function to check learner checkbox click - bind after each table draw
    $('#learnerAssignedModuleList').on('draw.dt', function () {
        $('#learnerAssignedModuleList tbody').off('change', 'input.chk-user'); // remove any duplicate bindings
        $('#learnerAssignedModuleList tbody').on('change', 'input[type="checkbox"]', function () {
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
                selRecords.push(id);
                // enable reset button
                $allocateAllBtn.removeClass('disabled');
                $allocateAllBtn.attr("onclick", "refreshTrainingManualHandler.resetForSelected()");
                $allocateAllBtn.attr("title", "Reset all selected training records");

                // remove id if exist in unselected array
                if (unSelRecords.length > 0) {
                    for (i = 0; i < unSelRecords.length; i++) {
                        if (unSelRecords[i] == id) {
                            unSelRecords.splice(i, 1);
                            break;
                        }
                    }
                }
            }
            else {

                //push in unselected array
                unSelRecords.push(id);

                // remove id if exist in selected array
                if (selRecords.length > 0) {
                    for (i = 0; i < selRecords.length; i++) {
                        if (selRecords[i] == id) {
                            selRecords.splice(i, 1);
                            break;
                        }
                    }
                }
                
                // Disable button if no records selected
                if (selRecords.length === 0 && !selectAllRecords) {
                    $allocateAllBtn.addClass('disabled');
                    $allocateAllBtn.removeAttr("onclick");
                    $allocateAllBtn.attr("title", "Select records to reset");
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
    function init() {
        showDefaultMessage();
        renderCourseDropDown();
        renderLocationDropDown();
        renderDepartmentDropDown(0);
    }

    //function to render list of all locations in organisation
    function renderCourseDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select Course' }));
        UTILS.data.getAllCourses(function (data) {
            populateList(data);
        })
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

    ////function to re-assign module access to single learner and refresh table
    //function allocateModuleAccess(btn) {
    //    if (confirm("Are you sure you want to re-assign access to this user?")) {
    //        var learnerId = btn.id.split('-').pop();
    //        var url = hdnBaseUrl + 'CourseManagement/ReassignLearnerModuleAccess'
    //        var data = {
    //            LearnerID: learnerId,
    //            Course: $ddlCourse.val()
    //        }
    //        UTILS.makeAjaxCall(url, data, function (result) {
    //            if (result.success == 1) {
    //                UTILS.Alert.show($alert, 'success', 'Access re-assigned successfully.')
    //                learnerTable.draw();
    //            }
    //            else
    //                UTILS.Alert.show($alert, 'error', 'Failed to re-assign access.')
    //        }, function (status) {
    //            console.log(status);
    //            });
    //    }
    //}

    //function to allocate module to single learner and refresh table
    function resetLearnerCourseRecord(btn) {
        if (confirm("Are you sure you want to reset training for this user?")) {
            var recordID = btn.id.split('-').pop();
            var url = hdnBaseUrl + 'CourseManagement/RefreshLearnerModuleProgress'
            var data = {
                LearnerID: parseInt(recordID)
            }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Training reset successfully.')
                    $('#learnerAssignedModuleList').DataTable().draw();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to reset training.')
            }, function (status) {
                console.log(status);
            });
        }
    }

    // function to select all checkbox click
    $selectAllChkBox.on('click', function () {
        //set variable value same as select all checkbox value
        selectAllRecords = this.checked;

        if (this.checked) {
            selRecords = [];
            unSelRecords = [];
            $allocateAllBtn.removeClass('disabled');
            $allocateAllBtn.attr("onclick", "refreshTrainingManualHandler.resetForSelected()");
            $allocateAllBtn.attr("title", "Reset all selected training records");
        } else {
            $allocateAllBtn.addClass('disabled');
            $allocateAllBtn.removeAttr("onclick");
            $allocateAllBtn.attr("title", "Select records to reset");
        }
        refreshTrainingManualHandler.selectAllRows();
    });

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = $('#learnerAssignedModuleList').DataTable().rows({ 'search': 'applied' }).nodes();

        // Check/uncheck checkboxes for all rows in the table
        $('input[type="checkbox"]', rows).prop('checked', $selectAllChkBox.prop('checked'));

        //check each learner and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-user').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selRecords.length > 0) {
                for (i = 0; i < selRecords.length; i++) {
                    if (selRecords[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unSelRecords.length > 0) {
                for (j = 0; j < unSelRecords.length; j++) {
                    if (unSelRecords[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }
    
    // function to bulk reset to selected button
    function resetForSelected() {
        if (selRecords.length > 0 || selectAllRecords) {
            if (confirm("Are you sure you want to reset all selected training records?")) {
                // allocate functionality
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + "CourseManagement/RefreshLearnerModuleProgress_Multiple";
                var data = {
                    SearchCriteria: {
                    SearchText: $learner.val(),
                        Location: parseInt($ddlLoc.val()),
                        Department: parseInt($ddlDep.val()),
                        Course: parseInt($ddlCourse.val())
                    },
                    AllSelected: selectAllRecords,
                    SelectedRecordList: selRecords.map(id => parseInt(id)),
                    UnselectedRecordList: unSelRecords.map(id => parseInt(id))
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Training reset successfully.')
                        resetSelections()
                        $('#learnerAssignedModuleList').DataTable().draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to reset training.')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, 'error', 'Failed to reset training. Please try again later');
                    UTILS.resetButton($allocateAllBtn);
                })
                
            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select training records to reset');
            resetSelections()
            ////disable allocate to all buton
            //$allocateAllBtn.addClass('disabled');
            //$allocateAllBtn.removeAttr("onclick", "refreshTrainingManualHandler.resetForSelected()");
            //$allocateAllBtn.attr("title", "Select some learners");
        }
        
    }

    return {
        init: init,
        resetLearnerCourseRecord: resetLearnerCourseRecord,
        renderLocationDropDown: renderLocationDropDown,
        renderDepartmentDropDown: renderDepartmentDropDown,
        selectAllRows: selectAllRows,
        resetForSelected: resetForSelected
    }
})();