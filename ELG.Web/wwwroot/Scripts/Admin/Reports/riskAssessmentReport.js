$(function () {
    $('#txtRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-ra-report');

    riskAssessmentReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var riskAssessmentReportHandler = (function () {

    var $learner = $('#txtLearner')
    var $ddlLoc = $('#ddlLocation')
    var $ddlDep = $('#ddlDepartment')
    var $ddlCourse = $('#ddlCourse')
    var $ddlIssue = $('#ddlIssue')
    var $ddlRAStatus = $('#ddlRAStatus')
    var $ddlSignedOff = $('#ddlSignedOff')
    var $txtRptFrom = $('#txtRptFrom')
    var $txtRptTo = $('#txtRptTo')
    var $searchBtn = $('#searchRiskAssessmentBtn');
    var $clearSearchBtn = $('#clearSearchRAReport');
    var $downloadBtn = $('#downloadRAReport');

    var $raRespModal = $('#riskAssessmentResponseModal');
    var $userName = $('#spnRAUserName');
    var $courseName = $('#spnRACourseName');
    var $container = $('#divRAResponseHolder');
    var $adminNote = $('#txtadminNotes');
    var $saveAdminNoteBtn = $('#btnUpdateAssessmentAdminNotes');
    var $updateBtn = $('#btnUpdateRiskAssessmentResponse');
    var $printRABtn = $('#btnPrintRiskAssessmentResponse');
    var $alert = $('#divMessageRAResp');
    var raid = 0;
    var groupId = 0;

    var template = "";
    template += '<div class="row col-md-12 {{class_issue_state}}">'
    template += '   <div class="col-md-3">'
    template += '       <p>{{ra_question}}</p>'
    template += '   </div>'
    template += '   <div class="col-md-1 text-center">'
    template += '       <p>{{ra_response}}{{evidence_link}}</p>'
    template += '   </div >'
    template += '   <div class="col-md-3 text-center">'
    template += '       <p>{{ra_response_notes}}</p>'
    template += '   </div>'
    template += '   {{follow-up-template}}'
    template += '</div >'

    var groupTemplate = '<div class="row"><div class="col-md-12 mb-2 text-gray-900"<b>Group: {{ra_group_name}}</b></div></div>';

    var $reportContainer = $("#div-report-container");
    function showDefaultMessage() {
        $reportContainer.hide();
        $('#registeredAdminLearnerList').DataTable().destroy();
        var message = '<div ><b>How to use this page:</b><ul>' +
            '<li>Use the input field to search for a user by name or email.</li>' +
            '<li>Select filters such as location, department, risk assessment, status, issue, and sign-off to narrow your results.</li>' +
            '<li>Use the date fields to filter assessments completed within a specific range.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> Search to view matching records.</li>' +
            '<li>Click <i class="fa fa-download me-1"></i> Download to export the filtered results to Excel.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alert, 'default', message);
    }

    //function to save admin notes
    $saveAdminNoteBtn.click(function (e) {
        UTILS.disableButton($saveAdminNoteBtn);
        e.preventDefault
        if (confirm('Are you sure you want to save Admin Notes?')) {
            var url = hdnBaseUrl + "Report/SaveAdminRANotes";
            var fileRAANUpload = $("#upload-ra-admin-note-pic").get(0);
            //var filesRAAN = fileRAANUpload.files;
            var raANImageData = new FormData();
            //if (filesRAAN.length > 0)
            //    raANImageData.append(filesRAAN[0].name, filesRAAN[0]);
            raANImageData.append("ra_id", raid);
            raANImageData.append("adminComments", $adminNote.val());

            $.ajax({
                url: url,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: raANImageData,
                success: function (result) {
                    //var obj = $.parseJSON(e);
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Admin Notes saved successfully.')
                    }
                    else {
                        UTILS.Alert.show($alert, 'error', 'Failed to update Admin Notes.')
                    }
                    UTILS.resetButton($saveAdminNoteBtn);
                },
                error: function (e) {
                    UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
                    UTILS.resetButton($saveAdminNoteBtn);
                }
            });

            //var data = {
            //    ra_id: raid,
            //    adminComments: $adminNote.val()
            //}

            //UTILS.makeAjaxCall(url, data, function (result) {
            //    if (result.success == 1) {
            //        UTILS.Alert.show($alert, 'success', 'Admin Notes saved successfully.')
            //    }
            //    else {
            //        UTILS.Alert.show($alert, 'error', 'Failed to update Admin Notes.')
            //    }
            //    UTILS.resetButton($saveAdminNoteBtn);
            //}, function (err) {
            //    console.log(err);
            //})
        }
    })

    //function to sign off RA
    $updateBtn.click(function (e) {
        UTILS.disableButton($updateBtn);
        e.preventDefault
        if (confirm('Are you sure you want to sign-off the risk assessment')) {
            var url = hdnBaseUrl + "Report/UpdateLearnerRAStatus";

            var fileRAANUpload = $("#upload-ra-admin-note-pic").get(0);
            //var filesRAAN = fileRAANUpload.files;

            var raANImageData = new FormData();
            //if (filesRAAN.length > 0)
            //    raANImageData.append(filesRAAN[0].name, filesRAAN[0]);
            raANImageData.append("ra_id", raid);
            raANImageData.append("adminComments", $adminNote.val());

            //var data = {
            //    ra_id: raid,
            //    adminComments: $adminNote.val()
            //}
            $.ajax({
                url: url,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: raANImageData,
                success: function (result) {
                    //var obj = $.parseJSON(e);
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Risk assessment signed off successfully.')
                        $updateBtn.hide();
                        $saveAdminNoteBtn.hide();
                        $adminNote.attr('disabled', 'disabled');
                        $('#riskAssessmentReport').DataTable().draw();
                    }
                    else {
                        UTILS.Alert.show($alert, 'error', 'Failed to sign-off the risk assessment.')
                    }
                    UTILS.resetButton($updateBtn);
                },
                error: function (e) {
                    UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
                    UTILS.resetButton($updateBtn);
                }
            });

            //UTILS.makeAjaxCall(url, data, function (result) {
            //    if (result.success == 1) {
            //        UTILS.Alert.show($alert, 'success', 'Risk assessment signed off successfully.')
            //        $updateBtn.hide();
            //        $saveAdminNoteBtn.hide();
            //        $adminNote.attr('disabled', 'disabled');
            //        raReport.draw();
            //    }
            //    else {
            //        UTILS.Alert.show($alert, 'error', 'Failed to sign-off the risk assessment.')
            //    }
            //    UTILS.resetButton($updateBtn);
            //}, function (err) {
            //    console.log(err);
            //})
        }
    })

    //function to open modal for risk assessment responses
    function showRAResponsePopUP(btn) {
        var raReport = $('#riskAssessmentReport').DataTable();
        UTILS.Alert.hide($alert)
        groupId = 0;
        raid = btn.id.split('-').pop();
        var learner = (raReport.row(btn.closest('tr')).data()["FirstName"]) + " " + (raReport.row(btn.closest('tr')).data()["LastName"]);
        var course = raReport.row(btn.closest('tr')).data()["CourseName"];
        $userName.html(learner);
        $courseName.html(course);
        $container.html('<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>');
        $raRespModal.modal('show');
        getGetLearnerRiskAssessmentResponses();

    }

    function getGetLearnerRiskAssessmentResponses() {
        var url = hdnBaseUrl + "Report/GetLearnerRiskAssessmentResponses"
        var data = {
            riskAssId: raid
        }
        UTILS.makeAjaxCall(url, data, function (data) {
            $container.html('');
            if (data != null && data.response != null && data.response != undefined && data.response.length > 0) {
                $('#spnRALocation').html(data.location);
                $('#spnRACompletionDate').html(data.completionDate);
                var imageURL = data.response[0].AdminNoteImagePath;
                $adminNote.val(data.response[0].AdminNotes);
                data.response.forEach(renderTemplate)

                if (data.response[0].DateSignedOff != null && data.response[0].DateSignedOff != '') {
                    //$('#divChkSignOff').hide();
                    $('#spnSignOffDate').html('Signed Off on: ' + data.response[0].DateSignedOff);
                    $('#spnSignOffBy').html('Signed Off By: ' + data.response[0].SignedOfBy_FirstName + ' ' + data.response[0].SignedOfBy_LastName);
                    $updateBtn.hide();
                    $saveAdminNoteBtn.hide();
                    $adminNote.attr('disabled', 'disabled');
                    //embed uploaded image 
                    if (imageURL != null && imageURL != '') {
                        var raImageTemplate = '<a href="' + imageURL + '" target="_blank" title="Admin notes">View Image</a>';
                        $("#ra-admin-note-picture").html(raImageTemplate);
                    }
                } else {

                    var raImageTemplate = '<input type="file" accept="image/*" id="upload-ra-admin-note-pic" class="">';
                    if (imageURL != null && imageURL != '') {
                        raImageTemplate = '<a href="' + imageURL + '" target="_blank" title="Admin notes">View Image</a>'+raImageTemplate;
                    }
                    $("#ra-admin-note-picture").html(raImageTemplate);

                    //$('#divChkSignOff').show();
                    $('#spnSignOffDate').hide();
                    $('#spnSignOffBy').hide();
                    $updateBtn.show();
                    $saveAdminNoteBtn.show();
                    $adminNote.removeAttr('disabled');
                }
            }
        }, function (err) {
            console.log(err);
        });
    }

    function renderTemplate(item, index) {
        var currentResp = template;
        var currentGroup = groupTemplate;
        var followUpTemplate = '';
        var evidenceTemplate = '';

        if (groupId != item.GroupId) {
            groupId = item.GroupId
            currentGroup = currentGroup.replace('{{ra_group_name}}', item.GroupName);
            $container.append(currentGroup)
        }

        if (item.IsIssue) {
            currentResp = currentResp.replace('{{class_issue_state}}', "table-danger");

            if (item.FollowedUpOn == null || item.FollowedUpOn == '') {
                followUpTemplate += '   <div class="col-md-3 text-center">'
                followUpTemplate += '       <input class="form-control" type= "text" id="txt-feedback-' + item.ResponseId + '" placeholder="Feedback" />'
                followUpTemplate += '   </div>'
                followUpTemplate += '   <div class="col-md-2 text-center">'
                followUpTemplate += '       <button type="button" id="btn-followed-up-' + item.ResponseId + '" class="btn btn-dark" onclick="riskAssessmentReportHandler.markFollowedUp(this)"><span>Followed Up</span></button>'
                followUpTemplate += '   </div>'
                //currentResp = currentResp.replace('{{feedback_btn_state}}', " onclick='riskAssessmentReportHandler.markFollowedUp(this)' ");
            } else {
                //currentResp = currentResp.replace('/{{ra_resp_feedback}}/g', "disabled");
                //currentResp = currentResp.replace('/{{ra_resp_feedback}}/g', 'value = "disabled {{' + item.FollowUpFeedback + '}}"');
                followUpTemplate += '   <div class="col-md-3 text-center">'
                followUpTemplate += '       <span>' + item.FollowUpFeedback + '</span>'
                followUpTemplate += '   </div>'
                followUpTemplate += '   <div class="col-md-2 text-center">'
                followUpTemplate += '       <button type="button" id="btn-followed-up-' + item.ResponseId + '" class="btn btn-dark" disabled><span>Followed Up</span></button>'
                followUpTemplate += '   </div >'
            }


            if (item.Evidence != null && item.Evidence != '') {
                evidenceTemplate = ' <a href="' + item.Evidence + '" target="_blank">Evidence</a>';
            } 

        } else {
            currentResp = currentResp.replace('{{class_issue_state}}', "");
        }
        currentResp = currentResp.replace(/{{follow-up-template}}/g, followUpTemplate);

        currentResp = currentResp.replace(/{{ra_question}}/g, item.Question);
        currentResp = currentResp.replace(/{{ra_response}}/g, item.Response);
        currentResp = currentResp.replace(/{{evidence_link}}/g, evidenceTemplate);
        currentResp = currentResp.replace(/{{ra_response_notes}}/g, item.ResponseNote);
        $container.append(currentResp);
    }


    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        $ddlIssue.val('1');
        $ddlSignedOff.val('2');
        $('#riskAssessmentReport').DataTable().draw();
    }

    // function to initialise report page
    // bind drop down in search area
    function init() {

        $.urlParam('filter')
        renderCourseDropDown();
        renderLocationDropDown();
        showDefaultMessage();
    }

    //function to render list of all courses in organisation
    function renderCourseDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllRiskAssessments(function (data) {
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
           SearchText : $learner.val(),
           Location : $ddlLoc.val(),
           Department : $ddlDep.val(),
           Course : $ddlCourse.val(),
           RAStatus : $ddlRAStatus.val(),
           Issue : $ddlIssue.val(),
           SignedOff : $ddlSignedOff.val(),
           From : $txtRptFrom.val(),
           To : $txtRptTo.val(),
        }

        var path = 'DownloadRiskAssessmentReport?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        $reportContainer.show();
        UTILS.Alert.hide($alert);
        $('#riskAssessmentReport').DataTable().destroy();
        $('#riskAssessmentReport').DataTable({
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": false,
            "ajax": {
                "url": "LoadRiskAssessmentReport",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                    data.RAStatus = $ddlRAStatus.val();
                    data.Issue = $ddlIssue.val();
                    data.SignedOff = $ddlSignedOff.val();
                    data.From = $txtRptFrom.val();
                    data.To = $txtRptTo.val();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "createdRow": function (row, data, dataIndex) {
                if (data["Issue"] > 0) {
                    $(row).addClass('table-danger');
                }
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "Location", "name": "l.strLocation", "autoWidth": true },
                { "data": "Department", "name": "d.strDepartment", "autoWidth": true },
                { "data": "CourseName", "name": "co.strCourse", "autoWidth": true },
                { "data": "AssignedOnDate", "name": "pd.dateAssignedOn", "autoWidth": true },
                { "data": "Issue", "name": "ra.intIssueCount", "autoWidth": true },
                { "data": "SignedOffDate", "name": "ra.datSignoff", "autoWidth": true },
                { "data": "RAStatus", "name": "ra.datCompleted", "autoWidth": true },
                { "data": "CompletionDate", "name": "ra.datCompleted", "autoWidth": true }
            ],
            columnDefs: [{
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                // render action buttons in the last column
                targets: [9], render: function (a, b, data, d) {
                    return '<button type="button" id="ra-' + data["RiskAssessmentId"] + '" class="btn btn-sm btn-dark mb-1" onclick="riskAssessmentReportHandler.showRAResponsePopUP(this)"><span>Action</span></button> '
                        + '<div>' + data["CompletionDate"] + '</div>'
                }
            }]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $ddlRAStatus.val('0');
        $ddlIssue.val('0');
        $ddlSignedOff.val('0');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        showDefaultMessage();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });


    ////funtion to print individual ra report
    //$printRABtn.click(function (e) {
    //    e.preventDefault();
    //    populatePrintElement();
    //});

    //// function to create prinatable content for ra individual report
    //function populatePrintElement() {
    //    $("#div_txtadminNotes").html($adminNote.val());
    //    var bodyHtml = $("#raIndividualReportBody").html();
    //    var $printSection = $("#individualRAPrintSection");
    //    $printSection.html('');
    //    $printSection.html(bodyHtml);
    //    $printSection.find("#txtadminNotes").remove();
    //    window.print();
    //}

    $printRABtn.on('click', function (e) {
        e.preventDefault();
        printRAReportBodyOnly();
    });

    function printRAReportBodyOnly() {
        const content = document.querySelector('#raIndividualReportBody');
        if (!content) return;

        // Create a root-level print container
        let printContainer = document.getElementById('printRAContainer');
        if (!printContainer) {
            printContainer = document.createElement('div');
            printContainer.id = 'printRAContainer';
            document.body.appendChild(printContainer);
        }

        // Copy content into the print container
        printContainer.innerHTML = content.outerHTML;

        // Temporarily set the container's id to match the CSS selector
        printContainer.id = 'raIndividualReportBody';

        // Trigger print
        window.print();

        // Restore id and cleanup
        printContainer.id = 'printRAContainer';
        printContainer.innerHTML = '';
    }
    //// Print button click
    //$printRABtn.click(function (e) {
    //    e.preventDefault();
    //    printRAReport();
    //});

    //// Function to print only modal body content
    //function printRAReport() {
    //    var bodyHtml = $("#raIndividualReportBody").html();

    //    // Open a new window
    //    var printWindow = window.open('', '', 'height=600,width=800');

    //    // Write only the modal body content into it
    //    printWindow.document.write('<html><head><title>Risk Assessment</title>');
    //    // optional: include your CSS for styling
    //    printWindow.document.write('<link rel="stylesheet" href="/path/to/bootstrap.css">');
    //    printWindow.document.write('</head><body>');
    //    printWindow.document.write(bodyHtml);
    //    printWindow.document.write('</body></html>');

    //    printWindow.document.close();
    //    printWindow.focus();

    //    // Trigger print
    //    printWindow.print();
    //    //printWindow.close();
    //}
    //function to mark issue as followed up
    function markFollowedUp(btn) {
        if (confirm("Are you sure you want to mark this as Followed Up?")) {
            var respId = btn.id.split('-').pop();
            var feedBack = $("#txt-feedback-" + respId).val();

            var url = hdnBaseUrl + "Report/UpdateLearnerRAIssueFollowedUp"
            var data = {
                respId: respId,
                feedBack: feedBack
            }

            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    getGetLearnerRiskAssessmentResponses();
                }
                else {
                    alert('Failed to update follow up status.');
                }
            }, function (err) {
                alert('Failed to update follow up status.');
                console.log(err);
            })
        }

    }

    return {
        markFollowedUp: markFollowedUp,
        showRAResponsePopUP: showRAResponsePopUP,
        init: init
    }
})();