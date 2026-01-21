document.addEventListener('DOMContentLoaded', function () {
    UTILS.activateNavigationLink('raLink');
    UTILS.activateMenuNavigationLink('menu-manage-ra');
    document.querySelectorAll('[data-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));
});

var raReportHandler = (function () {
    const courseInput = document.querySelector('#txtRACourse');
    const searchBtn = document.querySelector('#searchRACourse');
    const clearSearchBtn = document.querySelector('#clearSearchRACourse');
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

    var raImageTemplate = '<div class="card ra-issue-evidence" id="evidence-container-{{que_id}}"><div class="card-body"><div class="row mb-4"><div class="ra-picture-container"><div class="ra-picture"><img src="" class="picture-src" id="{{que_id}}" title=""><input type="file" accept="image/*" id="ra-pic-{{que_id}}" class="">     </div><a href="#" style="display:none" id="btn-remove-pic-{{que_id}}" onclick="raReportHandler.removeRaImage()">&times; Remove</a></div></div><div class="row"> <div class="picture-msg alert alert-info"><h6>Upload evidence image (optional)</h6> <h6>Supported image formats are .jpg, .jpeg, .png and .gif and size should not exceed 10mb.</h6> </div></div> </div></div>';

    let raQuestions = [];
    let queTab = 0;
    const divQueTab = document.querySelector("#divQuestionTab");
    const alertElement = document.querySelector('#raErrorMessage');
    const finishPage = document.querySelector("#divRAFinishTab");
    let courseId = 0;

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
            "url": hdnBaseUrl + "LoadRAData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = courseInput.value;
            },
            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
                window.location.reload();
            }
        },
        "columns": [
            { "data": "CourseName", "name": "CourseName", "autoWidth": true },
            { "data": "RACompletedOn", "name": "RACompletedOn", "autoWidth": true },
            { "data": "IssueCount", "name": "IssueCount", "autoWidth": true },
            { "data": "RASignOffDate", "name": "RASignOffDate", "autoWidth": true }
        ],
        columnDefs: [ {
            // render action buttons in the last column
            targets: [4], render: function (a, b, data, d) {
                let btn = ' '

                if (data["RASignOffDate"] !== null && data["RASignOffDate"] !== '') {
                    btn = btn + '<button type="button" id="ra-course-' + data["CourseId"] + '" class="btn btn-sm btn-primary mb-1" disabled><i class="fa fa-fw fa-play"></i> <span>Launch</span></button> '
               } else { 
                    btn = btn + '<button type="button" id="ra-course-' + data["CourseId"] + '" class="btn btn-sm btn-primary mb-1" onclick="raReportHandler.showRAPopUP(this)"><i class="fa fa-fw fa-play"></i> <span>Launch</span></button> '
                }

                if (data["RACompletedOn"] !== null && data["RACompletedOn"] !== '') {
                    btn = btn + '<button type="button" id="ra-review-' + data["CourseId"] + '" class="btn btn-sm btn-primary mb-1" onclick="raReportHandler.showRAReviewPopUP(this)"><i class="fa fa-fw fa-eye"></i> <span>Review</span></button> '
                } else {
                    btn = btn + '<button type="button" id="ra-review-' + data["CourseId"] + '" class="btn btn-sm btn-primary mb-1 disabled"><i class="fa fa-fw fa-eye"></i> <span>Review</span></button> '
                }

                return btn
            }
        }],
    });

    const raResponseTable = new DataTable('#reviewRAResponseList', {
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
            "url": hdnBaseUrl + "GetRiskAssessmentResponses",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.RACourseID= courseId;
            },
            "dataSrc": function (json) {
                if (json.data && json.data.length > 0) {
                    console.log('First row of reviewRAResponseList data:', json.data[0]);
                }
                return json.data;
            },
            "error": function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
                window.location.reload();
            }
        },
        "createdRow": function (row, data, dataIndex) {
            if (data["Issue"] === true) {
                row.classList.add('table-danger');
            }
        },
        "columns": [
            { "data": "Question", "name": "Question", "autoWidth": true },
            { "data": "Answer", "name": "Answer", "autoWidth": true }
        ]
    });


    function showRAReviewPopUP(btn) {
        courseId = btn.id.split('-').pop();
        new bootstrap.Modal(reviewModal).show();
        raResponseTable.draw();
    }

    //apply filters for search
    searchBtn.addEventListener('click', function (e) {
        e.preventDefault();
        raTable.draw();
    });

    //clear search filters
    clearSearchBtn.addEventListener('click', function (e) {
        e.preventDefault();
        courseInput.value = '';
        raTable.draw();
    });

    //function to open modal for risk assessment 
    function showRAPopUP(btn) {
        queTab = 0;
        courseId = btn.id.split('-').pop();
        raQuestions = [];
        startRAPage.style.display = 'none';
        finishPage.style.display = 'none';
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        divQueTab.style.display = 'block';
        const raTitle = raTable.row(btn.closest('tr')).data()["CourseName"];
        modalTitle.innerHTML = raTitle;

        new bootstrap.Modal(modal, {
            backdrop: 'static',
            keyboard: true
        }).show();
    }

    modal.addEventListener('show.bs.modal', function (e) {
        UTILS.Alert.hide(alertElement);
        renderRiskAssessmentQuestions();
    });

    function renderRiskAssessmentQuestions() {
        const url = hdnBaseUrl + "GetRiskAssessmentQuestion";
        const data = {
            CourseId: courseId
        }
        UTILS.makeAsyncAjaxCall(url, data, function (res) {
            if (res.quesList.length > 0) {
                raQuestions = res.quesList;
                loader.style.display = 'none';
                startRAPage.style.display = 'block';
            }
            else {
                UTILS.Alert.show(alertElement, 'warning', "No questions found.");
            }
        }, function (err) {
            console.log(err);
        });
    }

    //start RA and Render questions
    startRABtn.addEventListener('click', function (e) {
        e.preventDefault();
        startRAPage.style.display = 'none';
        renderQuestion();
    })

    function renderQuestion() {
        UTILS.Alert.hide(alertElement);
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
                UTILS.Alert.show(alertElement, 'error', "Please select an option before submit.");
                return false;
            }

            //check if ra evidence is to be uploaded
            const fileUpload = document.querySelector("#ra-pic-" + raQuestions[queTab - 1].QuestionId);
            const files = fileUpload.files;

            if (files.length > 0) {
                // Create FormData object  
                const raEvidenceData = new FormData();
                raEvidenceData.append(files[0].name, files[0]);
                raEvidenceData.append('AnswerId', answerId);

                //upload ra evidence
                fetch(hdnBaseUrl + "UploadEvidence", {
                    method: "POST",
                    body: raEvidenceData
                })
                .then(response => response.json())
                .then(result => {
                    if (result.status <= 0) {
                        UTILS.Alert.show(alertElement, "error", "Failed to upload evidence image.")
                        return false;
                    }
                })
                .catch(err => {
                    console.log(err);
                    UTILS.Alert.show(alertElement, "error", "Failed to upload evidence image. Please try again later");
                    return false;
                });
            }

            //submit answer
            submitAnswer(answerValue, answerNote, answerId);

            // last answer submit
            if (queTab === raQuestions.length) {
                //hide questions page
                divQueTab.style.display = 'none';
                document.querySelectorAll(".submit-btn").forEach(btn => btn.classList.add('disabled'));

                const url = hdnBaseUrl + "SetRACompleted";
                const data = {
                    CourseId: courseId
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
            const url = hdnBaseUrl + "StartRiskAssessment";
            const data = {
                CourseId: courseId
            }
            UTILS.makeAjaxCall(url, data, function (res) { }, function (err) { })
        }
        loader.style.display = 'block';
        divQueTab.innerHTML = '';
        const quesInfo = raQuestions[queTab];

        let questionLine = queTemplate;  
        let txtArea = noteTemplate
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

        txtArea = noteTemplate.replace("{{que_id}}", quesInfo.QuestionId);
        divQueTab.innerHTML += txtArea;

        let raEvidence = raImageTemplate
        raEvidence = raEvidence.replace(/{{que_id}}/g, quesInfo.QuestionId); 
        divQueTab.innerHTML += raEvidence;

        submitBtn = submitBtn.replace("{{que_id}}", quesInfo.QuestionId);
        divQueTab.innerHTML += submitBtn;
        loader.style.display = 'none';
        document.querySelectorAll('.ra-issue-evidence').forEach(el => el.style.display = 'none');
        queTab++
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

        const url = hdnBaseUrl + "SaveRAResponse";
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
        checkResponseIssue: checkResponseIssue
    }
})();
