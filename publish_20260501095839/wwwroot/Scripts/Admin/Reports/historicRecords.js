$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtUpdateRecordCompletionDate').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-historic-report');
    learningProgressReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var learningProgressReportHandler = (function () {

    var $alert = $('#message_test_report');
    var $testReportContainer = $('#test-report-container')
    var $learner = $('#txtLearner')
    var $ddlCourse = $('#ddlCourse')
    var $status = $('#ddlStatus')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchLearnerProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $downloadBtn = $('#downloadProgressReport');
    var recId = 0;

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
        var message = '<div> ' +
            '<b>How to use this page:</b>' +
            ' <ul> <li>Input User email.</li>' +
            '  <li>Click <i class="fa fa-search me-1"></i> Search to view historic records that match your selected filters.</li> ' +
            '<li>Click <i class="fa fa-download me-1"></i> Download to export the filtered report data to Excel.</li> ' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li> ' +
            '</ul></div>';
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

    //function to render list of all courses in organisation
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

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $learner.val(),
            Course: $ddlCourse.val(),
            Status: $status.val(),
            From: $txtRptFrom.val(),
            To: $txtRptTo.val(),
        }

        var path = 'DownloadHistoricRecords?' + $.param(data);
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
                "url": "LoadHistoricLearningProgress",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val().replace(/'/g, "''");
                    data.Course = $ddlCourse.val();
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
                { "data": "CompletionDate", "name": "pd.dateCompletedOn", "autoWidth": true },
                { "data": "MovedToHistoryOn", "name": "pd.movedToHistoryOn", "autoWidth": true }
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
                    targets: [11], // Adjust if needed
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
                   `;

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
                </ul>
            </div>
        </div>`;

                        return btn;
                    }
                }]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $ddlCourse.val('-1');
        $status.val('-1');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        showDefaultMessage();
    });

    function createCertificate(btn) {
        var recordId = btn.id.split("-").pop();
        var url = hdnBaseUrl + "Certificate/GetCertificate/"+recordId;
        window.open(url, '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes');

    }
    return {
        init: init,
        createCertificate: createCertificate
    }
})();