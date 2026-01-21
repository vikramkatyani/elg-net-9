$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtUpdateRecordCompletionDate').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-learning-report');
    learningProgressReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var learningProgressReportHandler = (function () {

    var $alert = $('#message_test_report');
    var $testReportContainer = $('#test-report-container')
    var $learner = $('#txtLearner')
    var $learnerStatus = $('#ddlUserStatus')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $ddlAccessStatus = $('#ddlAccessStatus')
    var $status = $('#ddlStatus')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchLearnerProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $downloadBtn = $('#downloadProgressReport');

    var $updateProgressModal = $('#updateLearningRecordModal');
    var $updateProgressTitle = $('#updateLearningRecordModalLabel');
    var $recordStatus = $('#ddlLearningRecordStatus');
    var $recordScore = $('#txtUpdateRecordScore');
    var $recordCompletionDate = $('#txtUpdateRecordCompletionDate');
    var $updateRecordBtn = $('#btnUpdateLearningRecord');
    var $updateRecordMessage = $('#updateLearningRecordMessage');

    var recId = 0;

    //function to open update record Modal
    function showUpdateLearningRecordPopUP(btn) {
        UTILS.Alert.hide($updateRecordMessage)

        recId = btn.id.split('-').pop();
        var rowData = $('#learningProgressReport').DataTable().row(btn.closest('tr')).data();
        var module = rowData["CourseName"];
        var learner = rowData["FirstName"] +" "+ rowData["LastName"] ;
        var score = rowData["Score"];
        var status = rowData["CourseStatus"];
        var completionDate = rowData["CompletionDate"];
        $updateProgressTitle.html('Update - ' + module + ' for: ' + learner);
        $recordStatus.val(status);
        $recordScore.val(score);
        $recordCompletionDate.val(completionDate);

        $updateProgressModal.modal('show');
    }

    //function to update learning record
    $updateRecordBtn.click(function (e) {
        UTILS.disableButton($updateRecordBtn);
        e.preventDefault();
        if (confirm('Are you sure you want to update this record?')) {
            var url = hdnBaseUrl + "Report/UpdateLearningProgress";
            var data = {
                RecordId: recId,
                Score: $recordScore.val(),
                CourseStatus: $recordStatus.val(),
                CompletionDate: $recordCompletionDate.val()
            }

            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($updateRecordMessage, 'success', 'Record updated successfully.')
                    UTILS.resetButton($updateRecordBtn);
                    $('#learningProgressReport').DataTable().draw();
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

    //$.urlParam = function (name) {
    //    var trainee = '';
    //    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
    //        .exec(window.location.href);
    //    if (results == null) {
    //        return 0;
    //    }
    //    $learner.val(decodeURI(results[1]));
    //    $('#learningProgressReport').DataTable().draw();
    //}


    function showDefaultMessage() {
        var message = '<div > <b>How to use this page:</b> <ul>  <li>Select filter criteria such as user, course, status, or date range using the dropdowns and input fields.</li>  <li>Click <i class="fa fa-search me-1"></i> Search to view test results that match your selected filters.</li> <li>Click <i class="fa fa-download me-1"></i> Download to export the filtered report data to Excel.</li> <li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li> </ul></div>';
        UTILS.Alert.show($alert, 'default', message);
        $testReportContainer.hide();
    }

    // function to initialise report page
    // bind drop down in search area
    function init() {
        showDefaultMessage();
        //$.urlParam('trainee');
        $txtRptFrom.val("");
        $txtRptTo.val("");
        $txtRptFrom.attr("disabled", "disabled");
        $txtRptTo.attr("disabled", "disabled");

        renderCourseDropDown();
        renderLocationDropDown();
        applyFiltersInUrl();
    }

    function getQueryParams() {
        const params = new URLSearchParams(window.location.search);
        let queryObject = {};

        params.forEach((value, key) => {
            queryObject[key] = value;
        });

        return queryObject;
    }

    function applyFiltersInUrl() {
        // Usage Example
        const queryParams = getQueryParams();
        $learner.val(queryParams.trainee ? decodeURI(queryParams.trainee) : '');
        $ddlCourse.val(queryParams.course ? decodeURI(queryParams.course) : '-1');

        if (queryParams.trainee || queryParams.course) {
            $searchBtn.click();            
        }

    }

    //function to render list of all locations in organisation
    function renderCourseDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllCourses(function (data) {
            if (data && data.courseList != null) {
                $.each(data.courseList, function (index, item) {
                    $ddlCourse.append($('<option/>', {
                        value: item.CourseId,
                        text: item.CourseName
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
            SearchText: $learner.val(),
            UserStatus: $learnerStatus.val(),
            Location:$ddlLoc.val(),
            Department: $ddlDep.val(),
            Course: $ddlCourse.val(),
            AccessStatus: $ddlAccessStatus.val(),
            Status: $status.val(),
            From: $txtRptFrom.val(),
            To: $txtRptTo.val(),
        }

        var path = 'DownloadLearningProgress?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alert);
        $testReportContainer.show();
        //progressReport.draw();
        $('#learningProgressReport').DataTable().destroy();
        $('#learningProgressReport').DataTable({
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
                "url": "LoadLearningProgress",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val().replace(/'/g, "''");
                    data.UserStatus = $learnerStatus.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                    data.AccessStatus = $ddlAccessStatus.val(),
                        data.Status = $status.val();
                    data.From = $txtRptFrom.val();
                    data.To = $txtRptTo.val();
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
                { "data": "CourseName", "name": "co.strCourse", "autoWidth": true },
                { "data": "AssignedOn", "name": "pd.dateAssignedOn", "autoWidth": true },
                { "data": "LastAccessedDate", "name": "pd.dateLastStarted", "autoWidth": true },
                { "data": "CourseStatus", "name": "pd.strStatus", "autoWidth": true },
                { "data": "Score", "name": "pd.intScore", "autoWidth": true },
                { "data": "CompletionDate", "name": "pd.dateCompletedOn", "autoWidth": true }
            ],
            columnDefs: [{
                // render learner name
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                // render learner name
                targets: [7], render: function (a, b, data, d) {
                    if (data["CourseStatus"] == 'incomplete')
                        return "<span>In-progress</span>";
                    else
                        return "<span>" + data["CourseStatus"] + "</span>";
                }
                }, {
                    targets: [10], // Adjust if needed
                    orderable: false,
                    searchable: false,
                    className: "text-center align-middle", // Center the ellipsis icon
                    render: function (a, b, data, d) {
                        const recordId = data["RecordId"];
                        const courseStatus = data["CourseStatus"];

                        let btn = `
        <div class="d-flex justify-content-center align-items-center">
            <div class="dropdown">
                <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                        id="actionDropdown-${recordId}" data-bs-toggle="dropdown" aria-expanded="false"
                        style="width: 2.5rem; height: 2.5rem;">
                    <i class="fa fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${recordId}">
                    <li>
                        <a class="dropdown-item" href="#" id="update-record-${recordId}"
                           onclick="learningProgressReportHandler.showUpdateLearningRecordPopUP(this)">
                            <i class="fa fa-fw fa-edit me-2"></i>Edit
                        </a>
                    </li>`;

                        if (courseStatus === 'passed') {
                            btn += `
                    <li>
                        <a class="dropdown-item" href="#" id="cert-user-${recordId}"
                           onclick="learningProgressReportHandler.createCertificate(this)">
                            <i class="fa fa-fw fa-certificate me-2"></i>Certificate
                        </a>
                    </li>`;
                        } else {
                            btn += `
                    <li>
                        <a class="dropdown-item disabled" href="#" id="cert-user-${recordId}">
                            <i class="fa fa-fw fa-certificate me-2"></i>Certificate
                        </a>
                    </li>`;
                        }

                        btn += `
                    <li>
                        <a class="dropdown-item" href="#" id="history-user-${recordId}"
                           onclick="learningProgressReportHandler.viewProgressHistory(this)">
                            <i class="fa fa-fw fa-clock-rotate-left me-2"></i>History
                        </a>
                    </li>
                </ul>
            </div>
        </div>`;

                        return btn;
                    }
                }]
        });
    });

    //apply filters for search
    $status.change(function () {
        if ($status.val() != "passed" && $status.val() != "failed" )
        {
            $txtRptFrom.val("");
            $txtRptTo.val("");
            $txtRptFrom.attr("disabled", "disabled");
            $txtRptTo.attr("disabled", "disabled");
        }
        else {
            $txtRptFrom.val("");
            $txtRptTo.val("");
            $txtRptFrom.removeAttr("disabled");
            $txtRptTo.removeAttr("disabled");
        }
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $learnerStatus.val('1');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $ddlAccessStatus.val('0'),
        $status.val('0');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        showDefaultMessage();
        $('#learningProgressReport').DataTable().destroy();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //var progressReport = 

    function createCertificate(btn) {
        var recordId = btn.id.split("-").pop();
        var url = hdnBaseUrl + "Certificate/GetCertificate/"+recordId;
        //var selectedRowData = progressReport.row(btn.closest('tr')).data();
        //var recordId = selectedRowData["RecordId"];
        //var userName = selectedRowData["FirstName"] + " " + selectedRowData["LastName"];
        //var completionDate = selectedRowData["CompletionDate"];
        //var courseName = selectedRowData["CourseName"];
        //var score = selectedRowData["Score"];
        //var data = {
        //    RecordId: recordId,
        //    CourseName: courseName,
        //    Score: score,
        //    CompletionDate: completionDate,
        //    user: userName
        //}
        window.open(url, '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes');
        //$.ajax({
        //    type: "POST",
        //    url: url,
        //    data: data,
        //    async: false,
        //    dataType: "application/pdf",
        //    success: function (data, textStatus, jqXHR) {
        //        window.open(escape(data), "Title", "");
        //    }
        //});
        //UTILS.makeAjaxCall(url, data, function (file) {
        //    var base64EncodedPDF = file;
        //    var dataURI = "data:application/pdf;base64," + base64EncodedPDF;
        //    window.open(dataURI, '_blank');
        //})

    }
    function viewProgressHistory(btn) {
        var selectedRowData = $('#learningProgressReport').DataTable().row(btn.closest('tr')).data();
        var em = selectedRowData["EmailId"];
        var c = selectedRowData["Course"];
        var dataURI = hdnBaseUrl + `Report/HistoricRecords?trainee=${em}&course=${c}`;
        window.open(dataURI, '_blank');
    }
    return {
        init: init,
        createCertificate: createCertificate,
        showUpdateLearningRecordPopUP: showUpdateLearningRecordPopUP,
        viewProgressHistory: viewProgressHistory
    }
})();