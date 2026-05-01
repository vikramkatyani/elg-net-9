
$(function () {
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-trainingcard-report');
    trainingCardReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var trainingCardReportHandler = (function () {

    var $alert = $('#message_trainingCard_report');
    var $trainingCardHolder = $('#trainingCardHolder');
    var $searchText = $('#txtLearner');
    var $searchBtn = $('#searchLearnerTraining');
    var $printButton = $("#btnPrintTrainingCard")
    var userEmail = '';

    var $traineeName = $('#spn-trainee-name');
    var $traineeEmail = $('#spn-trainee-email');
    var $traineeLoc = $('#spn-trainee-location');
    var $traineeDep = $('#spn-trainee-department');
    var $traineePic = $('#img-trainee-pic');
    var NO_DATA_MESSAGE = "No data found."


    var template = "";


    template += '<div class="card">';
    template += '    <div class="card-content">';
    template += '        <h2>{{course.CourseName}}</h2>';
    template += '        <p class="txt-status"><span class="txt-{{course.ProgressStatus}}">{{course.ProgressStatus}}</span> | Last accessed: {{course.LastAccessedOn}}</p>';
    template += '        <p>{{course.CourseDesc}}</p>';

    template += '        <div class="sub-module-accordion d-none mt-4 p-6" id="accordion_{{course.CourseId}}">';
    template += '           <div id="collapse_{{course.CourseId}}">';
    template += '               {{course.subModuleList}}';
    template += '           </div>';
    template += '       </div>';

    template += '        <div class="card-button-container">';
    template += '            {{course.LaunchButton}}';
    template += '            {{course.CertificateButton}}';
    template += '            {{course.HistoryButton}}';
    //template += '            {{course.MoreInfoButton}}';
    template += '            {{course.ResetProgressButton}}';
    template += '        </div>';

    template += '    </div>';
    template += '    <div class="card-image">';
    template += '        <img src="../course_covers/{{course.CourseLogo}}" alt="{{course.CourseName}}">';
    template += '    </div>';
    template += '</div>';



    $.urlParam = function (name) {
        var trainee = '';
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        $searchText.val(decodeURI(results[1]));
        //searchUserTraining();
    }

    function init() {
        var message = "Enter learner's email address and click ‘Search’ to get Training Card.";

        $.urlParam('trainee');
        UTILS.Alert.show($alert, 'info', message);
        $trainingCardHolder.hide();
        searchUserTraining();
    }

    function getTraineeInfo() {
        var  url = hdnBaseUrl + "Report/LoadTrainingCard"
        var learner = {
            SearchText: userEmail
        }
        UTILS.makeAjaxCall(url, learner, function (result) {
            var trainingCard = result.trainingCard;
            if (trainingCard != null && trainingCard.Trainee != null && trainingCard.Trainee.TraineeEmail != null && trainingCard.Trainee.TraineeEmail != '') {
                $trainingCardHolder.show();
                $traineeName.val(trainingCard.Trainee.TraineeName);
                $traineeEmail.val(trainingCard.Trainee.TraineeEmail);
                $traineeLoc.val(trainingCard.Trainee.TraineeLocation);
                $traineeDep.val(trainingCard.Trainee.TraineeDepartment);
                $traineePic.attr('src', trainingCard.Trainee.TraineePicURL);

                populateTestResult(trainingCard.TestResult);
                populateRAResult(trainingCard.RAResult);
                populateDocResult(trainingCard.DocResult);

            } else {
                UTILS.Alert.show($alert, "error", NO_DATA_MESSAGE);
            }
        }, function (status) {
            console.log(err.statusText);
            UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
        });
    }

    function populateTestResult(testResult) {
        var $testResultContainer = $("#div-training-testResult");
        if (testResult == null || testResult.length < 1) {
            $testResultContainer.addClass('alert alert-warning').html(NO_DATA_MESSAGE);
        } else {
            //var html = "";
            //for (var i = 0; i < testResult.length; i++) {
            //    if (testResult[i].Status == "Completed")
            //        html += "<p style='color:#000'>Completed " + testResult[i].CourseName + " on " + testResult[i].CompletionDate + " with " + testResult[i].Score + "%</p>";
            //    else if (testResult[i].Status == "Started")
            //        html += "<p style='color:#000'>Started " + testResult[i].CourseName + " on " + testResult[i].LastAccessDate +"</p>";
            //    else 
            //        html += "<p style='color:#000'>Not Started " + testResult[i].CourseName +"</p>";
            //}
            //$testResultContainer.html(html);
            $testResultContainer.html('');
            testResult.forEach(renderTemplate);
        }

    }
    function renderTemplate(item, index) {
        var currentCourse = template;
        currentCourse = currentCourse.replace(/{{course.CoursePath}}/g, item.CoursePath);
        currentCourse = currentCourse.replace(/{{course.CourseLogo}}/g, item.CourseLogo);
        currentCourse = currentCourse.replace(/{{course.ProgressStatus}}/g, (item.ProgressStatus).toUpperCase());
        currentCourse = currentCourse.replace(/{{course.CourseId}}/g, item.CourseId);
        currentCourse = currentCourse.replace(/{{course.CourseName}}/g, item.CourseName);
        currentCourse = currentCourse.replace(/{{course.CourseDesc}}/g, item.CourseDesc);
        currentCourse = currentCourse.replace(/{{course.LastAccessedOn}}/g, item.LastAccessedOn);
        currentCourse = currentCourse.replace(/{{course.courseid}}/g, item.CourseId);
        currentCourse = currentCourse.replace(/{{course.CourseResetDate}}/g, item.CourseResetOn);

        ////if (item.IsExpired == 0) {
        ////    if (item.ProgressStatus == 'passed' || item.ProgressStatus == 'PASSED')
        ////        currentCourse = currentCourse.replace('{{course.LaunchButton}}', '<button class="btn btn-primary btn-lg col-md-12" onclick = "myCoursesHandler.showLaunchConfirmationPopUp(' + item.CourseId + ', \'' + item.CoursePath + '\', \'' + item.CourseResetOn + '\')">View Course <i class="fa fa-play"></i></button>');
        ////    else
        ////        currentCourse = currentCourse.replace('{{course.LaunchButton}}', '<button class="btn btn-primary btn-lg col-md-12" onclick = "launchModulePopUp(' + item.CourseId + ', \'' + item.CoursePath + '\')">View Course <i class="fa fa-play"></i></button>');
        ////}
        ////else
        ////    currentCourse = currentCourse.replace('{{course.LaunchButton}}', '');

        //if (item.ProgressStatus == 'passed' || item.ProgressStatus == 'PASSED') {
        //    //currentCourse = currentCourse.replace('{{course.CertificateButton}}', '<button type="button" id="cert-rec-' + item.ProgressRecordId + '" class="btn btn-sm btn-dark mb-1" onclick = "myCoursesHandler.createCertificate(this)" > <i class="fa fa-print"></i> Print Certificate</button>');
        //    currentCourse = currentCourse.replace(/{{course_completion_text}}/g, 'Next due after');
        //    currentCourse = currentCourse.replace(/{{course.CourseCompleteBy}}/g, item.CourseResetOn);
        //}
        //else {
        //    //currentCourse = currentCourse.replace('{{course.CertificateButton}}', '<button type="button" id="cert-rec-' + item.ProgressRecordId + '" class="btn btn-sm btn-dark mb-1 disabled"> <i class="fa fa-print"></i> Print Certificate</button>');
        //    currentCourse = currentCourse.replace(/{{course_completion_text}}/g, 'Complete by');
        //    currentCourse = currentCourse.replace(/{{course.CourseCompleteBy}}/g, item.CourseCompleteBy);
        //}

        //if (item.CourseResetOn == null || item.CourseResetOn == '') {
        //    currentCourse = currentCourse.replace(/{{course_display_type}}/g, 'display:none');
        //}
        //else {
        //    currentCourse = currentCourse.replace(/{{course_display_type}}/g, 'display:block');
        //}

        ////currentCourse = currentCourse.replace('{{course.HistoryButton}}', '<button type="button" id="history-rec-' + item.ProgressRecordId + '" class="btn btn-sm btn-dark mb-1" onclick = "progressHistoryHandler.showCourseHistory(' + item.CourseId + ')" > <i class="fa fa-history"></i> Progress History</button>');
        ////currentCourse = currentCourse.replace('{{course.MoreInfoButton}}', '<button type="button" id="more-info-' + item.ProgressRecordId + '" class="btn btn-sm btn-dark mb-1" onclick = "myCoursesHandler.showMoreInfo(this)" > <i class="fa fa-info-circle"></i> More Info</button>');

        $("#div-training-testResult").append(currentCourse)
    }

    function populateRAResult(raResult) {
        var $raResultContainer = $("#div-training-raResult");
        if (raResult == null || raResult.length < 1) {
            $raResultContainer.addClass('alert alert-warning').html(NO_DATA_MESSAGE);
        } else {
            //var html = "";
            //for (var i = 0; i < raResult.length; i++) {
            //    //html += "<p style='color:#000'>Completed <b>" + raResult[i].CourseName + "</b> on <b>" + raResult[i].CompletionDate + "</b></p>";

            //    if (raResult[i].CompletionDate != null && raResult[i].CompletionDate != '')
            //        html += "<p style='color:#000'>Completed " + raResult[i].CourseName + " on " + raResult[i].CompletionDate + "</p>";
            //    else
            //        html += "<p style='color:#000'>Not Completed " + raResult[i].CourseName + "</p>";



            //}
            //$raResultContainer.html(html);
            $("#tableRACourseList").dataTable({ searching: false, paging: false, info: false });
            $("#tableRACourseList").DataTable().clear();
            for (var i = 0; i < raResult.length; i++) {
                $('#tableRACourseList').dataTable().fnAddData([
                    raResult[i].CourseName,
                    raResult[i].RACompletedOn,
                    raResult[i].IssueCount,
                    raResult[i].RASignOffDate
                ]);
            }
        }

    }

    function populateDocResult(docResult) {
        var $docResultContainer = $("#div-training-docResult");
        if (docResult == null || docResult.length < 1) {
            $docResultContainer.html(NO_DATA_MESSAGE);
        } else {
            var html = "";
            for (var i = 0; i < docResult.length; i++) {
               // html += "<p style='color:#000'>Read <b>" + docResult[i].DocName + "</b> on <b>" + docResult[i].ViewedOn + "</b></p>";
                html += "<p style='color:#000'>Read " + docResult[i].DocName + " on " + docResult[i].ViewedOn + "</p>";
            }
            $docResultContainer.html(html);
        }

    }

    function searchUserTraining() {
        userEmail = $searchText.val();
        if (userEmail == '') {
            return false;
        }
        UTILS.Alert.hide($alert);
        getTraineeInfo();
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        searchUserTraining();
        
    });

    $printButton.click(function (e) {
        e.preventDefault();
        //var $printableDiv = $("#printable-card");
        //$printableDiv.html('');
        //$printableDiv.append($("#div_trainee_details_holder").html());
        //$printableDiv.append($("#div_testresult_details_holder").html());
        //$printableDiv.append($("#div_ra_details_holder").html());
        //$printableDiv.append($("#div_doc_details_holder").html());
        //setTimeout(function () { // necessary for Chrome
        //    window.print();
        //}, 100);
        window.print();
    });

    return {
        init: init
    }
})();