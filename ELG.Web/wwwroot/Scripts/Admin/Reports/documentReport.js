$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-doc-report');
    documnetReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var documnetReportHandler = (function () {

    var $alert = $('#message_document_report');
    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlStatus = $('#ddlStatus')
    var $ddlIssedStatus = $('#ddlIssedStatus')
    var $category = $('#ddlCategory')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchDocumentReportBtn');
    var $clearSearchBtn = $('#clearSearchDocumentReport');
    var $downloadBtn = $('#downloadDocumentReport');
    var $testReportContainer = $('#document-report-container');

    // function to initialise report page
    // bind drop down in search area
    function init() {
        showDefaultMessage();
        renderLocationDropDown();
        renderCategoryDropDown();
    }
    function showDefaultMessage() {
        var message = '<div > <b>How to use this page:</b> <ul>  <li>Select filter criteria using the dropdowns and input fields.</li>  <li>Click <i class="fa fa-search me-1"></i> Search to view document report that match your selected filters.</li> <li>Click <i class="fa fa-download me-1"></i> Download to export the filtered report data to Excel.</li> <li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li> </ul></div>';
        UTILS.Alert.show($alert, 'default', message);
        $testReportContainer.hide();
    }

    //function to render list of all documnet category in organisation
    function renderCategoryDropDown() {
        $category.empty();
        $category.append($('<option/>', { value: '0', text: 'Select All' }));


        UTILS.data.getAllDocumentCategory(function (data) {
            if (data && data.catList != null) {
                $.each(data.catList, function (index, item) {
                    $category.append($('<option/>', {
                        value: item.CategoryId,
                        text: item.CategoryName
                    }))
                });
            }
        })
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

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText : $learner.val(),
            Location : $ddlLoc.val(),
            Department : $ddlDep.val(),
            Category: $category.val(),
            Status : $ddlStatus.val(),
            AssignmentStatus: $ddlIssedStatus.val(),
            FromDate : $txtRptFrom.val(),
            ToDate : $txtRptTo.val(),
        }

        var path = 'DownloadDocumentReport?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search

    //apply filters for search
    $searchBtn.on('click', function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $testReportContainer.show();
        let userRole = 0;
        
        // Check if DataTable exists before destroying
        if ($.fn.DataTable.isDataTable('#orgDocumentReport')) {
            $('#orgDocumentReport').DataTable().destroy();
        }
        
        var table = $('#orgDocumentReport').DataTable({
            lengthChange: false,
            "processing": true,
            "scrollX": true, 
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadDocumentReport",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Category = $category.val();
                    data.Status = $ddlStatus.val();
                    data.AssignmentStatus = $ddlIssedStatus.val();
                    data.From = $txtRptFrom.val();
                    data.To = $txtRptTo.val();
                },
                "dataSrc": function (json) {
                    userRole = json.UserRole; // capture role from server
                    // Hide Action column if not role 1
                    if (userRole !== 1) {
                        table.column(10).visible(false);
                    } else {
                        table.column(10).visible(true);
                    }
                    return json.data;
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "Location", "name": "l.strLocation", "autoWidth": true },
                { "data": "Department", "name": "d.strDepartment", "autoWidth": true },
                { "data": "CategoryName", "name": "dm.TX_NAME", "autoWidth": true },
                { "data": "FileName", "name": "m.TX_FILENAME", "autoWidth": true },
                { "data": "DocumentAssignmentStatus", "name": "map.intRecordId", "autoWidth": true },
                { "data": "ViewedOnDate", "name": "vd.dtOfViewed", "autoWidth": true },
                { "data": "DocumentStatus", "name": "vd.strUserStatus", "autoWidth": true },
                { "data": "StatusUpdatedOn", "name": "vd.statusUpdatedOn", "autoWidth": true },
                { "data": null, "orderable": false, "searchable": false }
            ],
            columnDefs: [{
                // render action buttons in the last column
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            },
                {
                    targets: [10],
                    render: function (a, b, data, d) {
                        if (data.DocumentAssignmentStatus === "Yes") {
                            return '<button type="button"  id="edit-doc-' + data["DocID"] + '" class="btn btn-primary" onclick="editUserDocHandler.showEditLearnerDocPopUP(this)"><i class="fa fa-fw fa-edit"></i></button>';
                        } else {
                            return '<button disabled type="button" id="edit-doc-' + data["DocID"] + '" class="btn btn-primary" data-toggle="tooltip" title="Document not issued"><i class="fa fa-fw fa-edit"></i></button>';
                        }
                    }
                }
            ]
        });
    });

    //clear search filters
    $clearSearchBtn.on('click',function (e) {
        e.preventDefault();
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $category.val('0');
        $ddlStatus.val('0');
        $ddlIssedStatus.val('-1');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        showDefaultMessage();
        
        // Safely draw DataTable if initialized
        const tableElement = $('#orgDocumentReport');
        if (tableElement.length && $.fn.DataTable.isDataTable(tableElement)) {
            tableElement.DataTable().draw();
        }
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    return {
        init: init
    }
})();

var editUserDocHandler = (function () {

    var $modal = $('#updateLearnerDocInfoModal');
    var $updateDocInfoTitle = $('#updateLearnerDocInfoModalLabel');
    var $docStatus = $('#ddlDocStatus');
    var $viewedOn = $('#txtViewedOn');
    var $updatedOn = $('#txtStatusUpdatedOn');
    var $updateRecordMessage = $('#editLearnerDocError');
    var $updateRecordBtn = $('#btnUpdateLearnerDocInfo');

    var selectedUserid = 0;
    var selectedDocid = 0;

    function showEditLearnerDocPopUP(btn) {
        UTILS.Alert.hide($updateRecordMessage)
        var rowData = $('#orgDocumentReport').DataTable().row(btn.closest('tr')).data();
        selectedUserid = rowData["UserID"];
        selectedDocid = rowData["DocID"];
        var documentName = rowData["FileName"];
        var learner = rowData["FirstName"] + " " + rowData["LastName"];
        var status = rowData["DocumentStatus"];
        var viewedOnDate = rowData["ViewedOnDate"];
        var statusUpdatedOn = rowData["StatusUpdatedOn"];
        $updateDocInfoTitle.html('Update - ' + documentName + ' for: ' + learner);
        $docStatus.val(status);
        $updatedOn.val(statusUpdatedOn);
        $viewedOn.val(viewedOnDate);

        $updatedOn.datepicker({ dateFormat: 'dd-M-yy' });
        $viewedOn.datepicker({ dateFormat: 'dd-M-yy' });
        $modal.modal('show');
    }
    //function to update learning record
    $updateRecordBtn.on('click',function (e) {
        UTILS.disableButton($updateRecordBtn);
        e.preventDefault();
        if (confirm('Are you sure you want to update this record?')) {
            var url = hdnBaseUrl + "Report/UpdateLearnerDocRecord";
            var data = {
                UserID: selectedUserid,
                DocID: selectedDocid,
                DocumentStatus: $docStatus.val(),
                ViewedOnDate: $viewedOn.val(),
                StatusUpdatedOn: $updatedOn.val()
            }

            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($updateRecordMessage, 'success', 'Record updated successfully.')
                    UTILS.resetButton($updateRecordBtn);
                    $('#orgDocumentReport').DataTable().draw();
                }
                else {
                    UTILS.Alert.show($updateRecordMessage, 'error', 'Failed to update record.')
                    UTILS.resetButton($updateRecordBtn);
                }
            }, function (err) {
                console.log(err);
                UTILS.Alert.show($updateRecordMessage, 'error', 'Failed to update record. Please try again later.')
                UTILS.resetButton($updateRecordBtn);
            })
        }
    });

    return {
        showEditLearnerDocPopUP: showEditLearnerDocPopUP
    }
})();