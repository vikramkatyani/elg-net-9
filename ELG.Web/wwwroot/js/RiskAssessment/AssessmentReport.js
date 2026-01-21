document.addEventListener('DOMContentLoaded', function () {
    UTILS.activateNavigationLink('myCourseLink');
    UTILS.activateMenuNavigationLink('menu-my-courses');
    document.querySelectorAll('[data-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));
});

var raReportHandler = (function () {
    const newRAButton = document.querySelector('#btnCreateNewRARecord');
    const modal = document.querySelector('#riskAssessmentModal');
    const modalTitle = document.querySelector('#riskAssessmentModal .modal-header');
    const container = document.querySelector("#divriskAssessmentContainer");
    const loader = document.querySelector("#divriskAssessmentContainer #loader");
    const startRAPage = document.querySelector("#divriskAssessmentContainer #divStartRAMessage");
    const startRABtn = document.querySelector("#btnStartRA");
    const reviewModal = document.querySelector('#riskAssessmentReviewModal');

    var queTemplate = '<div class="mb-4"><p>{{group_name}}</p><h4>{{ra_question}}</h4><div>';
    var optionTemplate = '<div> <h5><label for="radio-{{option_id}}"><input class="rb-option" type="radio"  name="radio-{{que_id}}" value="{{q_option_id}}" issue="{{opt_issue}}" onchange="raReportHandler.checkResponseIssue()"> {{option_text}}</label></h5> </div>';
    var submitButtonTemplate = '<div class="text-center"><button type="button" class="btn btn-primary mb-1 submit-btn" id="btn-{{que_id}}" onclick="raReportHandler.renderQuestion()">Submit</button></div>';
    var noteTemplate = '<span>Enter any other comments here</span> <textarea  class="form-control mb-4" id="note-{{que_id}}"></textarea>';

    // Table templates for all-at-once rendering
    // Use a class and allow unique ids for each table
    function getTableHeaderTemplate(tableId) {
        return '<table class="table table-striped riskAssessmentTable" id="' + tableId + '">\n<thead>\n<tr><th>Risk Factors</th><th>Things to consider</th><th>Response</th></tr>\n</thead>\n<tbody>';
    }
    var tableRowStartTemplate = '<tr data-question-id="{{que_id}}" data-answer-id="{{answer_id}}">';
    var tableRowColsTemplate = '<td class="align-middle">{{risk_factor}}</td><td class="align-middle">{{consider_text}}</td><td class="align-middle">{{options_html}}</td>';
    var tableRowEndTemplate = '</tr>';
    var tableFooterTemplate = '</tbody></table>';

    var raImageTemplate = '<div class="card ra-issue-evidence" id="evidence-container-{{que_id}}"><div class="card-body"><div class="row mb-4"><div class="ra-picture-container"><div class="ra-picture"><img src="" class="picture-src" id="{{que_id}}" title=""><input type="file" accept="image/*" id="ra-pic-{{que_id}}" class="">     </div><a href="#" style="display:none" id="btn-remove-pic-{{que_id}}" onclick="raReportHandler.removeRaImage()">&times; Remove</a></div></div><div class="row"> <div class="picture-msg alert alert-info"><h6>Upload evidence image (optional)</h6> <h6>Supported image formats are .jpg, .jpeg, .png and .gif and size should not exceed 10mb.</h6> </div></div> </div></div>';

    let raQuestions = [];
    let queTab = 0;
    const divQueTab = document.querySelector("#divQuestionTab");
    const alertElement = document.querySelector('#raErrorMessage');
    const finishPage = document.querySelector("#divRAFinishTab");

    let launchedRiskAssessment = 0;

    const ids = getIdsFromUrl();
    function getIdsFromUrl() {
        const parts = window.location.pathname.split('/').filter(p => p.length);
        const idx = parts.findIndex(p => p.toLowerCase() === 'loadrareport');
        if (idx >= 0 && parts.length > idx + 2) {
            return {
                courseId: decodeURIComponent(parts[idx + 1]),
                subModuleId: decodeURIComponent(parts[idx + 2])
            };
        }
        return { courseId: null, subModuleId: null };
    }

    let raid = 0;

    const raTable = new DataTable('#tableRACourseList', {
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "ajax": {
            "url": hdnBaseUrl +"LoadRAAttemptData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.RACourseID = ids.courseId;
                data.RASubModuleID = ids.subModuleId;
            },
            "dataSrc": function (json) {
                // Check condition
                if (json.incompleteRA > 0 || json.TotalCount <= 0) {
                    // Disable the button
                    $('#btnCreateNewRARecord').css('cursor', 'not-allowed');

                    // Remove any click handlers so it cannot be triggered
                    $('#btnCreateNewRARecord').off('click');
                } else {
                    // Enable the button again if condition is not met
                    $('#btnCreateNewRARecord').prop('disabled', false);

                    // Rebind click handler if needed
                    $('#btnCreateNewRARecord').off('click').on('click', function (e) {
                        if (confirm("Are you sure you want to create a new Risk Assessment record?")) {
                            raid = 0;
                            showRAPopUP(e.currentTarget, 'new');
                        }
                    });
                }
                // Normalize server data keys to the frontend's expected lowerCamel names
                var rows = Array.isArray(json.data) ? json.data : [];
                return rows.map(function (item) {
                    return {
                        locationName: item.locationName ?? item.LocationName ?? item.location ?? item.Location ?? '',
                        courseName: item.courseName ?? item.CourseName ?? item.course ?? item.Course ?? '',
                        raCompletedOn: item.raCompletedOn ?? item.RACompletedOn ?? item.completedOn ?? item.CompletedOn ?? '',
                        issueCount: item.issueCount ?? item.IssueCount ?? item.issue ?? item.Issue ?? 0,
                        raSignOffDate: item.raSignOffDate ?? item.RASignOffDate ?? item.signOffDate ?? item.SignOffDate ?? '',
                        raResultID: item.raResultID ?? item.RAResultID ?? item.resultID ?? item.ResultID ?? ''
                    };
                });
            },

            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
                window.location.reload();
            }
        },
        "columns": [
            { "data": "locationName", "name": "locationName", "autoWidth": true },
            { "data": "courseName", "name": "courseName", "autoWidth": true },
            { "data": "raCompletedOn", "name": "raCompletedOn", "autoWidth": true },
            { "data": "issueCount", "name": "issueCount", "autoWidth": true },
            { "data": "raSignOffDate", "name": "raSignOffDate", "autoWidth": true }
        ],
        columnDefs: [ {
            // render action buttons in the last column
            targets: [5], render: function (a, b, data, d) {
                let btn = ' '

                // Show only one button at a time: Review if completed, Launch if not
                if (data["raCompletedOn"] !== null && data["raCompletedOn"] !== '') {
                    // Completed - show Review button only
                    btn = btn + '<button type="button" id="ra-review-' + data["raResultID"] + '" class="btn btn-sm btn-primary mb-1" onclick="raReportHandler.showRAReviewPopUP(this)"><i class="fa fa-fw fa-eye"></i> <span>Review</span></button> '
                } else {
                    // Not completed - show Launch button only
                    btn = btn + '<button type="button" id="ra-' + data["raResultID"] + '" class="btn btn-sm btn-primary mb-1" onclick="raReportHandler.showRAPopUP(this, \'edit\')"><i class="fa fa-fw fa-play"></i> <span>Launch</span></button> '
                }

                return btn
            }
        }],
    });


    function showRAReviewPopUP(btn) {
        raRID = btn.id.split('-').pop();
        new bootstrap.Modal(reviewModal).show();

        const raResponseTable = new DataTable('#reviewRAResponseList', {

            destroy: true,

            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "ordering": false,
            "paging": false,
            "bInfo": false,
            "ajax": {
                "url": hdnBaseUrl + "GetLearnerSubModuleRAResponses",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.RAResultID = raRID;
                },
                "error": function (xhr, error, code) {
                    alert("Oops! Something went wrong please try again later.");
                    window.location.reload();
                }
            },
            "createdRow": function (row, data, dataIndex) {
                if (data["Issue"] === true || data["issue"] === true) {
                    row.classList.add('table-danger');
                }
            },
            "columns": [
                { "data": "Question", "name": "Question", "autoWidth": true },
                { "data": "Answer", "name": "Answer", "autoWidth": true }
            ]
        });
    }

    //function to open modal for risk assessment 
    function showRAPopUP(btn, mode) {
        queTab = 0;
        if(mode == 'edit')
            raid = btn.id.split('-').pop();

        courseId = ids.courseId;  
        raQuestions = [];
        startRAPage.style.display = 'none';
        finishPage.style.display = 'none';
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        divQueTab.style.display = 'block';

        new bootstrap.Modal(modal, {
            backdrop: 'static',
            keyboard: true
        }).show();
    }

    modal.addEventListener('show.bs.modal', function (e) {
        // Ensure jQuery object for alert utils to avoid type errors
        UTILS.Alert.hide($(alertElement));
        renderRiskAssessmentQuestions();
    });

    function renderRiskAssessmentQuestions() {
        const url = hdnBaseUrl + "GetSubModuleRiskAssessmentQuestion";
        const data = {
            // Use PascalCase to match server-side models reliably
            RiskAssessmentResultID: raid,
            SubModuleId: ids.subModuleId,
            CourseId: ids.courseId
        };
        UTILS.makeAsyncAjaxCall(url, data, function (res) {
            if (res && Array.isArray(res.quesList) && res.quesList.length > 0) {
                raQuestions = res.quesList;
                loader.style.display = 'none';
                startRAPage.style.display = 'block';
                launchedRiskAssessment = res.rarid;
            } else {
                loader.style.display = 'none';
                startRAPage.style.display = 'none';
                UTILS.Alert.show($(alertElement), 'warning', "No questions found.");
            }
        }, function (err) {
            loader.style.display = 'none';
            startRAPage.style.display = 'none';
            UTILS.Alert.show($(alertElement), 'error', "Unable to load assessment questions.");
            console.log(err);
        });
    }

    //start RA and Render questions
    startRABtn.addEventListener('click', function (e) {
        e.preventDefault();
        startRAPage.style.display = 'none';
        renderGroupedTables();
    })

    function renderQuestion() {
        UTILS.Alert.hide($(alertElement));
        document.querySelectorAll(".submit-btn").forEach(btn => btn.classList.remove('disabled'));

        let answerValue = "";
        let answerNote = "";
        let answerId = "";

        if (queTab > 0 && queTab <= raQuestions.length) {
            //check if any option is selected
            const checkedRadio = document.querySelector("input[type='radio']:checked");
            answerValue = checkedRadio ? checkedRadio.value : undefined;
            const noteEl = document.querySelector("#note-" + raQuestions[queTab-1].QuestionId);
            answerNote = noteEl ? noteEl.value : "";
            answerId = raQuestions[queTab-1].AnswerID
            if (answerValue === undefined || answerValue === null || answerValue === "") {
                UTILS.Alert.show($(alertElement), 'error', "Please select an option before submit.");
                return false;
            }

            //submit answer
            submitAnswer(answerValue, answerNote, answerId);

            // last answer submit
            if (queTab === raQuestions.length) {
                //hide questions page
                divQueTab.style.display = 'none';
                document.querySelectorAll(".submit-btn").forEach(btn => btn.classList.add('disabled'));

                const url = hdnBaseUrl + "SetSubModuleRACompleted";
                const data = {
                    RiskAssessmentResultID: launchedRiskAssessment
                }
                UTILS.makeAjaxCall(url, data, function (res) { }, function (err) { })

                //render finish page
                finishPage.style.display = 'block';

                //refresh table
                raTable.draw();
                return false;
            }
        }
        else {
            // start risk assessment
            const url = hdnBaseUrl + "StartSubModuleRiskAssessment";
            const data = {
                RiskAssessmentResultID: launchedRiskAssessment
            }
            UTILS.makeAjaxCall(url, data, function (res) { }, function (err) { })
        }
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        const quesInfo = raQuestions[queTab];

        let questionLine = queTemplate;  
        let submitBtn = submitButtonTemplate;

        questionLine = questionLine.replace("{{group_name}}", quesInfo.GroupText)
        questionLine = questionLine.replace("{{ra_question}}", quesInfo.QuestionText)
        divQueTab.innerHTML += questionLine;

        if (quesInfo.Options.length > 0) {
            for (let i = 0; i < quesInfo.Options.length; i++) {
                let optionLine = optionTemplate;
                optionLine = optionLine.replace("{{option_id}}", quesInfo.Options[i].QuestionOptionId);
                optionLine = optionLine.replace("{{q_option_id}}", quesInfo.Options[i].QuestionOptionId);
                optionLine = optionLine.replace("{{que_id}}", quesInfo.QuestionId);
                optionLine = optionLine.replace("{{option_text}}", quesInfo.Options[i].OptionText);
                optionLine = optionLine.replace("{{opt_issue}}", quesInfo.Options[i].Issue);
                divQueTab.innerHTML += optionLine;
            }
        }

        submitBtn = submitBtn.replace("{{que_id}}", quesInfo.QuestionId);
        divQueTab.innerHTML += submitBtn;
        loader.style.display = 'none';
        document.querySelectorAll('.ra-issue-evidence').forEach(el => el.style.display = 'none');
        queTab++
    }

    // Start SubModule Risk Assessment session
    function startSubModuleRA(locationVal) {
        const url = hdnBaseUrl + "StartSubModuleRiskAssessment";
        const data = {
            RiskAssessmentResultID: launchedRiskAssessment,
            StrLocationName: locationVal || ''
        }
        UTILS.makeAjaxCall(url, data, function (res) { }, function (err) { })
    }

    // Render all questions at once in a tabular view
    function renderAllQuestionsTable() {
        UTILS.Alert.hide(alertElement);
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        divQueTab.style.display = 'block';

        if (!raQuestions || raQuestions.length === 0) {
            loader.style.display = 'none';
            UTILS.Alert.show(alertElement, 'warning', "No questions found.");
            return;
        }

        var html = '';
        html += getTableHeaderTemplate('riskAssessmentTable-main');

        for (let i = 0; i < raQuestions.length; i++) {
            const quesInfo = raQuestions[i];
            let rowStart = tableRowStartTemplate
                .replace("{{que_id}}", quesInfo.QuestionId)
                .replace("{{answer_id}}", quesInfo.AnswerID || 0);

            let optionsHtml = '';
            if (quesInfo.Options && quesInfo.Options.length > 0) {
                for (let j = 0; j < quesInfo.Options.length; j++) {
                    const opt = quesInfo.Options[j];
                    optionsHtml += '<div class="form-check">'
                        + '<input class="form-check-input rb-option" type="radio" name="radio-' + quesInfo.QuestionId + '" value="' + opt.QuestionOptionId + '" issue="' + opt.Issue + '" onchange="raReportHandler.onOptionChange(this)">' 
                        + '<label class="form-check-label">' + opt.OptionText + '</label>'
                        + '</div>';
                }
            }

            let rowCols = tableRowColsTemplate
                .replace("{{risk_factor}}", quesInfo.GroupText)
                .replace("{{consider_text}}", quesInfo.QuestionText)
                .replace("{{options_html}}", optionsHtml);

            html += rowStart + rowCols + tableRowEndTemplate;
        }

        html += tableFooterTemplate;

        // Single submit button
        html += '<div class="text-center mt-3"><button type="button" class="btn btn-primary" id="btnSubmitAllRAResponses" onclick="raReportHandler.submitAllResponses()">Submit</button></div>';

        divQueTab.innerHTML = html;
        loader.style.display = 'none';
    }

    // Render questions grouped by GroupText into separate tables
    function renderGroupedTables() {
        UTILS.Alert.hide(alertElement);
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        divQueTab.style.display = 'block';

        if (!raQuestions || raQuestions.length === 0) {
            loader.style.display = 'none';
            UTILS.Alert.show(alertElement, 'warning', "No questions found.");
            return;
        }

        // Group by GroupText
        const groups = {};
        raQuestions.forEach(q => {
            const key = q.GroupText || 'General';
            if (!groups[key]) groups[key] = [];
            groups[key].push(q);
        });

        let html = '';
        // Add Location/Site input at the start
        html += '<div class="mb-4">';
        html += '<label for="txtRALocation" class="form-label fw-bold">Location / Site <span class="text-danger">*</span></label>';
        html += '<input type="text" class="form-control" id="txtRALocation" placeholder="Enter location or site" />';
        html += '</div>';

        let tableIndex = 1;
        Object.keys(groups).forEach(groupName => {
            const list = groups[groupName];
            // Section header styled band
            html += '<div class="bg-primary text-white p-2 mb-2">' + groupName + '</div>';
            const tableId = 'riskAssessmentTable-' + tableIndex;
            html += getTableHeaderTemplate(tableId);

            for (let i = 0; i < list.length; i++) {
                const quesInfo = list[i];
                let rowStart = tableRowStartTemplate
                    .replace("{{que_id}}", quesInfo.QuestionId)
                    .replace("{{answer_id}}", quesInfo.AnswerID || 0);

                let optionsHtml = '';
                if (quesInfo.Options && quesInfo.Options.length > 0) {
                    for (let j = 0; j < quesInfo.Options.length; j++) {
                        const opt = quesInfo.Options[j];
                        optionsHtml += '<div class="form-check">'
                            + '<input class="form-check-input rb-option" type="radio" name="radio-' + quesInfo.QuestionId + '" value="' + opt.QuestionOptionId + '" issue="' + opt.Issue + '" onchange="raReportHandler.onOptionChange(this)">' 
                            + '<label class="form-check-label">' + opt.OptionText + '</label>'
                            + '</div>';
                    }
                }

                let rowCols = tableRowColsTemplate
                    .replace("{{risk_factor}}", quesInfo.QuestionText || '')
                    .replace("{{consider_text}}", (quesInfo.Instructions || quesInfo.StrAdditionalText || ''))
                    .replace("{{options_html}}", optionsHtml);

                html += rowStart + rowCols + tableRowEndTemplate;
            }

            html += tableFooterTemplate;
            tableIndex++;
        });

        // Single submit button
        html += '<div class="text-center mt-3"><button type="button" class="btn btn-primary" id="btnSubmitAllRAResponses" onclick="raReportHandler.submitAllResponses()">Submit</button></div>';

        divQueTab.innerHTML = html;
        loader.style.display = 'none';
    }

    // Option change handler to toggle danger row highlight
    function onOptionChange(el) {
        const isIssue = (el.getAttribute('issue') === 'true');
        const row = el.closest('tr');
        if (!row) return;
        if (isIssue) {
            row.classList.add('table-danger');
        } else {
            row.classList.remove('table-danger');
        }
    }

    // Validate and submit all responses at once
    function submitAllResponses() {
        UTILS.Alert.hide(alertElement);
        const locationInput = document.querySelector('#txtRALocation');
        const location = locationInput ? locationInput.value.trim() : '';
        
        if (!location) {
            UTILS.Alert.show(alertElement, 'error', 'Please enter Location / Site before submitting.');
            if (locationInput) locationInput.focus();
            return;
        }

        // Ensure start call carries location
        startSubModuleRA(location);

        const tables = document.querySelectorAll('.riskAssessmentTable');
        if (!tables || tables.length === 0) {
            UTILS.Alert.show(alertElement, 'error', 'Unable to submit. Table(s) not found.');
            return;
        }

        const unanswered = [];
        const submissions = [];

        tables.forEach(table => {
            const rows = Array.from(table.querySelectorAll('tbody tr'));
            rows.forEach(row => {
                const qid = row.getAttribute('data-question-id');
                const ansId = parseInt(row.getAttribute('data-answer-id') || '0');
                const checked = table.querySelector('input[name="radio-' + qid + '"]:checked');
                if (!checked) {
                    unanswered.push(row);
                } else {
                    const payload = {
                        AnswerId: ansId,
                        OptionId: parseInt(checked.value),
                        IssueText: ''
                    };
                    submissions.push(payload);
                }
            });
        });

        if (unanswered.length > 0) {
            UTILS.Alert.show(alertElement, 'error', 'Please respond to all questions before submitting.');
            unanswered[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        // Submit all responses then mark RA completed
        const saveUrl = hdnBaseUrl + 'SaveSubModuleRAResponse';
        const completeUrl = hdnBaseUrl + 'SetSubModuleRACompleted';

        const savePromises = submissions.map(s => fetch(saveUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(s)
        }).then(r => r.json()));

        Promise.all(savePromises)
            .then(() => fetch(completeUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ RiskAssessmentResultID: launchedRiskAssessment, StrLocationName: location })
            }).then(r => r.json()))
            .then(() => {
                divQueTab.style.display = 'none';
                finishPage.style.display = 'block';
                if (typeof raTable !== 'undefined') {
                    raTable.draw();
                }
            })
            .catch(err => {
                console.error(err);
                UTILS.Alert.show(alertElement, 'error', 'Failed to submit responses. Please try again later.');
            });
    }

    function checkResponseIssue() {
        const isIssue = event.target.getAttribute("issue");
        const val = event.target.getAttribute('name').split('-').pop();
        const evidenceContainer = document.querySelector("#evidence-container-"+val);
        if (isIssue === "true") {
            UTILS.Alert.show(alertElement, 'success', 'This issue is recorded and will be reported to your manager.');
            if (evidenceContainer) evidenceContainer.style.display = 'block';
        } else {
            UTILS.Alert.hide(alertElement);
            if (evidenceContainer) evidenceContainer.style.display = 'none';
        }
    };

    //submit Answer
    function submitAnswer(ans, note, ansID) {

        const url = hdnBaseUrl + "SaveSubModuleRAResponse";
        const data = {
            AnswerId: ansID,
            OptionId: ans,
            IssueText: note
        }
        UTILS.makeAjaxCall(url, data, function (res) { }, function (err) { })
    }

    return {
        showRAPopUP: showRAPopUP,
        renderQuestion: renderQuestion,
        showRAReviewPopUP: showRAReviewPopUP,
        checkResponseIssue: checkResponseIssue,
        onOptionChange: onOptionChange,
        submitAllResponses: submitAllResponses,
        renderGroupedTables: renderGroupedTables
    }
})();
