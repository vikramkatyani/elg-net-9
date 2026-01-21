$(function () {
    UTILS.activateNavigationLink('accinciLink');
    $('[data-toggle="tooltip"]').tooltip();
});

var accidentIncidentReportHandler = (function () {
    var $search = $("#txtAcciInci");
    var $searchBtn = $("#searchAcciInci");
    var $clearSearchBtn = $("#clearSearchAcciInci");
    var $newIncidentBtn = $("#btnShowAcciInciPopUp");


    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $search.val('');
        refreshTable();
    });

    $searchBtn.click(function (e) {
        e.preventDefault();
        refreshTable();
    });

    $newIncidentBtn.click(function (e) {
        e.preventDefault();
        reportAccidentIncidentHandler.showPopUP(1); //hard coded value for first incident; to be changes when multiple are added in the system.
    })

    function refreshTable(){
        incidentsTable.draw();
    }


    var incidentsTable = $('#tableAccidentIncidentList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": "LoadReportedIncidentData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                searchText = $search.val().replace(/'/g, "''''");
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "ReportedBy", "name": "r.reportedBy", "autoWidth": true },
            { "data": "IncidentOn", "name": "r.incident_happendon", "autoWidth": true },
            { "data": "ReportedOn", "name": "r.incident_reportedon", "autoWidth": true },
            { "data": "ReportedFor", "name": "r.reportedFor", "autoWidth": true },
            { "data": "CreatorName", "name": "c.strFirstname", "autoWidth": true },
            { "data": "SignedOffOn", "name": "r.SignedOffOn", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [5], render: function (a, b, data, d) {

                if (data["SignedOffOn"] == null || data["SignedOffOn"] == '') {
                    return '';
                } else {
                    return '<span class="spn-al-uname" title="' + data["Comment"] +'" style="cursor:pointer">' + data["SignedOffOn"] + '</span>';
                }
                
            }
        },{
            // render action buttons in the last column
            targets: [6],
            render: function (a, b, data, d) {
                var btn = "";
                btn += '<button type="button" id="inc-report-' + data["AccidentIncidentId"] + '" class="btn btn-sm btn-dark mb-1" onclick="reviewAccidentIncidentHandler.showIncidentDetails(this)"><i class="fa fa-fw fa-eye"></i><span>View</span></button> '
                return btn;
            }
        }],
    });

    function showIncidentDetails() {

    }

    return {
        refreshTable: refreshTable,
        incidentsTable: incidentsTable
    }

})();

var reviewAccidentIncidentHandler = (function () {
    var $modal = $("#reviewAccidentIncidentModal");
    var $modalTitle = $("#reviewAccidentIncidentModal .modal-title");
    var $alert = $("#reviewAccidentIncidentErrorMessage");
    var $loader = $("#divReviewAccidentIncidentModalContainer #loader");
    var $divQueTab = $("#divReviewAccidentIncidentModalContainer #divReviewAccidentIncidentQuestionTab");
    var $form = $("#divReviewAccidentIncidentQuestionTab form");

    var aiID = 0;
    var resID = 0;
    var isSignedOff = 0;
    var signedOffOn = '';
    var signedOfBy = '';
    var responses = '';
    var comment = '';
    var imageURL = '';

    //function to open modal for review accident/incident
    function showIncidentDetails(btn) {
        var reportTable = accidentIncidentReportHandler.incidentsTable;
        var rowData = reportTable.row(btn.closest('tr')).data();
        aiID = rowData["AccidentIncidentId"];
        resID = rowData["ResponseId"];
        isSignedOff = rowData["IsSignedOff"] ? 1 : 0;
        signedOffOn = rowData["SignedOffOn"];
        signedOfBy = rowData["SignedOffBy"];
        comment = rowData["Comment"];
        responses = rowData["Response"];
        imageURL = rowData["ResponseImageURL"];
        $loader.show();
        $divQueTab.html('');
        $divQueTab.show();
        $modal.modal({
            backdrop: 'static',
            keyboard: true
        });
    }
    $modal.on('show.bs.modal', function (e) {
        UTILS.Alert.hide($alert);
        renderReviewAccidentIncidentQuestions();
    });
    function renderReviewAccidentIncidentQuestions() {
        var url = hdnBaseUrl + "AccidentIncident/GetAccidentIncidentQuestions";
        var data = {
            aiID: aiID
        }
        UTILS.makeAsyncAjaxCall(url, data, function (res) {
            //$container.html('');
            if (res.quesList.length > 0) {
                //raQuestions = res.quesList;
                //$loader.hide();
                //$startRAPage.show();
                populateQuestions(res.quesList);
            }
            else {
                //$container.html('<div class="alert alert-warning">No questions found</div>');
                UTILS.Alert.show($alert, 'warning', "No questions found.");
            }

        }, function (err) {
            console.log(err);
        });
    }

    function populateQuestions(ques) {

        var html = "";
        html += "<form>  <div class='form-row'>";

        for (var i = 0; i < ques.length; i++) {

            if (i == 0) {
                $modalTitle.html(ques[i].Title);
            }

            var qid = ques[i].QuestionID;
            var type = ques[i].QuestionType;
            var text = ques[i].Question;
            var options = ques[i].Options;
            var isMandatory = ques[i].IsMandatory;
            var seq = ques[i].DisplaySequence;

            html += getQuestionTemplate(type, text, options, isMandatory, seq, qid);
        }

        //embed uploaded image 
        if (imageURL != null && imageURL != '') {
            var raImageTemplate = '<div class="card col-md-9"> <div class="card-body"><div class="row mb-4"><div class="incident-picture-container"><div class="incident-picture"><img src="' + imageURL + '" class="picture-src" title=""></div></div></div> </div></div>';
            html += raImageTemplate;
        }

        html += "</div> </form>"

        //sign off comment
        if (isSignedOff == 1) {
            html += "<hr/><div class='form-group col-md-9'>";
            html += "<h5 for='cmnt_txtArea'>Sign-Off comments</h5>";
            html += "<textarea disabled class='form-control mb-4' id='cmnt_txtArea' name='cmnt_txtArea' rows='4'>" + comment + "</textarea>";
            html += "<div><label><b>Signed-Off On:</b>" + signedOffOn + "</label></div>";
            html += "<div><label><b>Signed-Off By:</b>" + signedOfBy + "</label></div>";
            html += "</div>";
        }

        $divQueTab.html(html);
        populateResponses();
        $loader.hide();

    }

    function getQuestionTemplate(type, text, options, isMandatory, seq, qid) {

        var questionTemplate = "";

        switch (type) {
            case "text": //text template
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>"
                questionTemplate += "<h5 for='resp_txtArea_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<input disabled type='text' class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input disabled type='text' class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' placeholder='Optional'>";
                questionTemplate += "</div>"
                break;
            case "textarea": //text area template
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>"
                questionTemplate += "<h5 for='resp_txt_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<textarea disabled class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' rows='3' placeholder='Required'></textarea>";
                else
                    questionTemplate += "<textarea disabled data-mandatory='0' class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' rows='3' placeholder='Optional'></textarea>";

                questionTemplate += "</div>"
                break;
            case "single"://radio button template

                var options = options.split('|');
                var optionTemplate = "";
                for (var j = 0; j < options.length; j++) {
                    optionTemplate += "<div class='form-check'><input disabled class='form-check-input' type='radio' id='rv_que_" + qid + "_" + j + "' name='rv_que_" + qid + "'  value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='rv_que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
                }
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>";
                questionTemplate += "<h5 for='resp_radio_" + seq + "'>" + seq + ". " + text + "</h5>"
                questionTemplate += optionTemplate;
                questionTemplate += "</div>";
                break;
            case "multiple": //checkbox template
                var options = options.split('|');
                var optionTemplate = "";
                for (var j = 0; j < options.length; j++) {
                    optionTemplate += "<div class='form-check'><input disabled class='form-check-input' type='checkbox' id='rv_que_" + qid + "_" + j + "' name='rv_que_" + qid + "' value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='rv_que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
                }
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>";
                questionTemplate += "<h5 for='resp_chk_" + seq + "'>" + seq + ". " + text + "</h5>"
                questionTemplate += optionTemplate;
                questionTemplate += "</div>";
                break;
            case "date": //calendar template

                questionTemplate = "<div class='form-group col-md-5 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>";
                questionTemplate += "<h5 for='resp_date_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<input type='text' disabled class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input type='text' disabled class='form-control' id='rv_que_" + qid + "' name='rv_que_" + qid + "' placeholder='Optional'>";
                questionTemplate += "</div>";
                break;
            default:
                break;
        }
        return questionTemplate;

    }

    function populateResponses() {
        var res = JSON.parse(responses);
        if (res.length > 0) {
            for (var i = 0; i < res.length; i++) {
                var type = res[i].type;
                var qName = "rv_que_" + res[i].queid;
                switch (type) {
                    case "text":
                    case "textarea":
                    case "date":
                        $("#" + qName).val(res[i].resp);
                        break;
                    case "single":
                        $('input:radio[name="' + qName + '"][value="' + res[i].resp + '"]').prop('checked', true);
                        break;
                    case "multiple":
                        var resps = res[i].resp;
                        if (resps.length > 0) {
                            for (var j = 0; j < resps.length; j++) {
                                $('input[name="' + qName + '"][value="' + resps[j] + '"]').prop('checked', true);
                            }
                        }
                        break;
                }
            }
        }
    }

    return {
        showIncidentDetails: showIncidentDetails
    }
})();


var reportAccidentIncidentHandler = (function () {
    var $modal = $("#newAccidentIncidentModal");
    var $modalTitle = $("#newAccidentIncidentModal .modal-title");
    var $alert = $("#newAccidentIncidentErrorMessage");
    var $loader = $("#divNewAccidentIncidentModalContainer #loader");
    var $divQueTab = $("#divNewAccidentIncidentModalContainer #divnewAccidentIncidentQuestionTab");
    var $form = $("#divnewAccidentIncidentQuestionTab form");
    var $submitBtn = $("#btnSubmitAccident");

    var aiID = 0;
    var responses = []

    //function to open modal for new accident/incident
    function showPopUP(id) {
        //aiID = btn.id.split('-').pop();
        aiID = id;
        $loader.show();
        $divQueTab.html('');
        $divQueTab.show();
        $modal.modal({
            backdrop: 'static',
            keyboard: true
        });
    }

    $modal.on('show.bs.modal', function (e) {
        UTILS.Alert.hide($alert);
        $submitBtn.show();
        renderAccidentIncidentQuestions();
    });


    function renderAccidentIncidentQuestions() {
        var url = hdnBaseUrl + "AccidentIncident/GetAccidentIncidentQuestions";
        var data = {
            aiID: aiID
        }
        UTILS.makeAsyncAjaxCall(url, data, function (res) {
            //$container.html('');
            if (res.quesList.length > 0) {
                //raQuestions = res.quesList;
                //$loader.hide();
                //$startRAPage.show();
                populateQuestions(res.quesList);
            }
            else {
                //$container.html('<div class="alert alert-warning">No questions found</div>');
                UTILS.Alert.show($alert, 'warning', "No questions found.");
            }

        }, function (err) {
            console.log(err);
        });
    }

    function populateQuestions(ques) {

        var html = "";
        html += "<form>  <div class='form-row'>";

        for (var i = 0; i < ques.length; i++) {

            if (i == 0) {
                $modalTitle.html(ques[i].Title);
            }

            var qid = ques[i].QuestionID;
            var type = ques[i].QuestionType;
            var text = ques[i].Question;
            var options = ques[i].Options;
            var isMandatory = ques[i].IsMandatory;
            var seq = ques[i].DisplaySequence; 

            html += getQuestionTemplate(type, text, options, isMandatory, seq, qid);
        }

        //embed image upload
        var raImageTemplate = '<div class="card acci-inci-evidence col-md-9" id="inci-evidence-container"><div class="card-body"><div class="row mb-4"><div class="incident-picture-container"><div class="incident-picture"><img src="" class="picture-src" id="img-incident" title=""><input type="file" accept="image/*" id="incident-pic" class="">     </div><a href="#" style="display:none" id="btn-remove-pic" onclick="reportAccidentIncidentHandler.removeIncidentImage()">&times; Remove</a></div></div><div class="row"> <div class="picture-msg alert alert-info"><h6>Upload image (optional)</h6> <h6>Supported image formats are .jpg, .jpeg, .png and .gif and size should not exceed 10mb.</h6> </div></div> </div></div>';
        html += raImageTemplate;

        html += "</div> </form>"
        $divQueTab.html(html);
        $('.date-input').datetimepicker({
            format: "YYYY-MM-DD HH:mm:ss",
            defaultDate: new Date(),
        });
        $loader.hide();

    }

    function getQuestionTemplate(type, text, options, isMandatory, seq, qid) {

        var questionTemplate = "";

        switch (type) {
            case "text": //text template
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid +"' data-q-mandatory='" + isMandatory +"' data-q-type='" + type+"'>"
                questionTemplate += "<h5 for='resp_txtArea_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<input type='text' class='form-control' id='que_" + qid + "' name='que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input type='text' class='form-control' id='que_" + qid + "' name='que_" + qid + "' placeholder='Optional'>";
                questionTemplate += "</div>"
                break;
            case "textarea": //text area template
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid +"' data-q-mandatory='" + isMandatory + "' data-q-type='" + type +"'>"
                questionTemplate += "<h5 for='resp_txt_" + seq + "'>" + seq + ". " + text +"</h5>"
                if (isMandatory)
                    questionTemplate += "<textarea class='form-control' id='que_" + qid + "' name='que_" + qid + "' rows='3' placeholder='Required'></textarea>";
                else
                    questionTemplate += "<textarea data-mandatory='0' class='form-control' id='que_" + qid + "' name='que_" + qid + "' rows='3' placeholder='Optional'></textarea>";
                
                questionTemplate += "</div>"
                break;
            case "single"://radio button template

                var options = options.split('|');
                var optionTemplate = "";
                for (var j = 0; j < options.length; j++) {
                    optionTemplate += "<div class='form-check'><input class='form-check-input' type='radio' id='que_" + qid + "_" + j + "' name='que_" + qid + "'  value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
                }
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid +"' data-q-mandatory='" + isMandatory + "' data-q-type='" + type +"'>";
                questionTemplate += "<h5 for='resp_radio_" + seq + "'>" + seq + ". " + text + "</h5>"
                questionTemplate +=  optionTemplate ;
                questionTemplate += "</div>";
                break;
            case "multiple": //checkbox template
                var options = options.split('|');
                var optionTemplate = "";
                for (var j = 0; j < options.length; j++) {
                    optionTemplate += "<div class='form-check'><input class='form-check-input' type='checkbox' id='que_" + qid + "_" + j + "' name='que_" + qid + "' value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
                }
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid +"' data-q-mandatory='" + isMandatory + "' data-q-type='" + type +"'>";
                questionTemplate += "<h5 for='resp_chk_" + seq + "'>" + seq+". "+ text + "</h5>"
                questionTemplate +=  optionTemplate ;
                questionTemplate += "</div>";
                break;
            case "date": //calendar template

                questionTemplate = "<div class='form-group col-md-5 q-box' data-qid='" + qid +"' data-q-mandatory='" + isMandatory + "' data-q-type='" + type +"'>";
                questionTemplate += "<h5 for='resp_date_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<input type='text' class='form-control date-input' id='que_" + qid +  "' name='que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input type='text' class='form-control date-input' id='que_" + qid + "' name='que_" + qid + "' placeholder='Optional'>";
                questionTemplate += "</div>";
                break;
            default:
                break;
        }
        return questionTemplate;

    }

    //submit
    $submitBtn.click(function (e) {
        e.preventDefault();
        validateAndSaveData();        
    })

    function validateAndSaveData() {
        var isValid = true;
        responses = [];
        $("#divnewAccidentIncidentQuestionTab .q-box").each(function () {
            var qBox = $(this);
            var type = qBox.data('q-type');
            var qid = qBox.data('qid');
            var isMand = qBox.data('q-mandatory');
            var qName = "que_" + qid;
            var response = {}

            switch (type) {
                case "text":
                    var resVal = $("#" + qName).val();
                    if (isMand == 1 && (resVal == null || resVal == '' || resVal.trim() == '')) {
                        isValid = false;
                        return isValid;
                    } else {
                        response = {
                            queid: qid,
                            type: type,
                            resp: resVal
                        } 

                        responses.push(response);
                    }
                    break;
                case "textarea":
                    var resVal = $("#" + qName).val();
                    if (isMand == 1 && (resVal == null || resVal == '' || resVal.trim() == '')) {
                        isValid = false;
                        return isValid;
                    } else {
                        response = {
                            queid: qid,
                            type: type,
                            resp: resVal
                        }

                        responses.push(response);
                    }
                    break;
                case "single":
                    if (isMand == 1 && (!$('input:radio[name="' + qName + '"]:checked').length)) {
                        valid = false;
                        return;
                    } else {
                        response = {
                            queid: qid,
                            type: type,
                            resp: $('input:radio[name="' + qName + '"]:checked').val()
                        }  

                        responses.push(response);
                    }
                    break;
                case "multiple":
                    if (isMand == 1 && (!$('input[name="' + qName + '"]:checked').length)) {
                        valid = false;
                        return;
                    } else {
                        var selections = [];
                        $.each($('input[name="' + qName + '"]:checked'), function () {
                            selections.push($(this).val());
                        });
                        response = {
                            queid: qid,
                            type: type,
                            resp: selections
                        }

                        responses.push(response);
                    }
                    break;
                case "date":
                    var resVal = $("#" + qName).val();
                    if (isMand == 1 && (resVal == null || resVal == '' || resVal.trim() == '')) {
                        isValid = false;
                        return isValid;
                    } else {
                        response = {
                            queid: qid,
                            type: type,
                            resp: resVal
                        }

                        responses.push(response);
                    }
                    break;
            }
        });

        if (isValid) {
            saveData(responses);
        } else {
            UTILS.Alert.show($alert, "warning", "Please provide responses for all required fields.")
        }
    }

    function saveData(responses) {
        UTILS.disableButton($submitBtn);
        UTILS.Alert.hide($alert);
        var url = hdnBaseUrl + "AccidentIncident/ReportNew";
        var jsonResp = JSON.stringify(responses);

        //check if ra evidence is to be uploaded
        var fileUpload = $("#incident-pic").get(0);
        var files = fileUpload.files;

        if (files.length > 0) {
            // Create FormData object  
            var raEvidenceData = new FormData();
            raEvidenceData.append(files[0].name, files[0]);
            raEvidenceData.append("Response", jsonResp);
            raEvidenceData.append("AccidentIncidentId", aiID);
            raEvidenceData.append("ReportedBy", $("#que_1").val());
            raEvidenceData.append("ReportedFor", $("#que_4").val());
            raEvidenceData.append("IncidentOn", $("#que_3").val());
            raEvidenceData.append("IsEmployee", $('input:radio[name="que_6"]:checked').val() == "Yes" ? 1 : 0);
            raEvidenceData.append("IsPermitted", $('input:radio[name="que_9"]:checked').val() == "Yes" ? 1 : 0);
            //upload ra evidence
            $.ajax({
                url: url,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: raEvidenceData,
                success: function (e) {
                    //var obj = $.parseJSON(e);
                    if (e.success > 0) {
                        UTILS.Alert.show($alert, "success", "Data saved successfully");
                        $submitBtn.hide();
                        $form.hide();
                        accidentIncidentReportHandler.refreshTable();
                    } else {
                        UTILS.Alert.show($alert, "error", "Failed to save data. Please try again later.");
                    }
                    UTILS.resetButton($submitBtn);
                },
                error: function (e) {
                    UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
                    UTILS.resetButton($submitBtn);
                }
            });

        } else {
            var data = {
                Response: jsonResp,
                ResponseDetails: {
                    AccidentIncidentId: aiID,
                    ReportedBy: $("#que_1").val(),
                    ReportedFor: $("#que_4").val(),
                    IncidentOn: $("#que_3").val(),
                    IsEmployee: $('input:radio[name="que_6"]:checked').val() == "Yes" ? 1 : 0,
                    IsPermitted: $('input:radio[name="que_9"]:checked').val() == "Yes" ? 1 : 0
                }
            }
            UTILS.makeAjaxCall(url, data, function (e) {
                //var obj = $.parseJSON(e);
                if (e.success > 0) {
                    UTILS.Alert.show($alert, "success", "Data saved successfully");
                    $submitBtn.hide();
                    $form.hide();
                    accidentIncidentReportHandler.refreshTable();
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to save data. Please try again later.");
                }
                UTILS.resetButton($submitBtn);
            }, function (e) {
                UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
                UTILS.resetButton($submitBtn);
            });
        }


    }

    return {
        showPopUP: showPopUP
    }

})();