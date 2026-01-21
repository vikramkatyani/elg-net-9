$(function () {
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-acciInci-report');
    adminAccidentIncidentReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var adminAccidentIncidentReportHandler = (function () {
    var $search = $("#txtAcciInciLearner");
    var $ddlSignedOff = $('#ddlAcciInciSignedOff')
    var $ddlIsEmployee = $('#ddlIsEmployee')
    var $ddlIsReportable = $('#ddlIsReportable')
    var $searchBtn = $("#searchAcciInciReportBtn");
    var $clearSearchBtn = $("#clearSearchAcciInciReport");
    var $downloadBtn = $("#downloadAcciInciReport");

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        $ddlSignedOff.val('2');
        refreshTable();
    }

    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $search.val('');
        $ddlSignedOff.val('0');
        $ddlIsEmployee.val('3');
        $ddlIsReportable.val('3');
        refreshTable();
    });

    $searchBtn.click(function (e) {
        e.preventDefault();
        refreshTable();
    });

    //apply filters and download excel report
    $downloadBtn.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        UTILS.disableButton(btn);
        var data = {
            SearchText: $search.val().replace(/'/g, "''''"),
            SignedOff: $ddlSignedOff.val(),
            IsEmployee: $ddlIsEmployee.val(),
            IsPermitted: $ddlIsReportable.val(),
        }

        var path = 'DownloadAccidentIncidentReport?' + $.param(data);
        window.location = path;
        UTILS.resetButton(btn);
    });

    function init() {
        $.urlParam('filter');;
    }

    function refreshTable(){
        adminAccidentIncidentsTable.draw();
    }


    var adminAccidentIncidentsTable = $('#orgAccidentIncidentReport').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": "LoadAccidentIncidentReport",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $search.val().replace(/'/g, "''''");
                data.SignedOff = $ddlSignedOff.val();
                data.IsEmployee = $ddlIsEmployee.val();
                data.IsPermitted = $ddlIsReportable.val();
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
            { "data": "IsEmployee", "name": "r.is_a_employee", "autoWidth": true },
            { "data": "IsPermitted", "name": "r.is_permited_activity", "autoWidth": true },
            { "data": "SignedOffOn", "name": "r.SignedOffOn", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [7], render: function (a, b, data, d) {

                if (data["SignedOffOn"] == null || data["SignedOffOn"] == '') {
                    return '';
                } else {
                    return '<span class="spn-al-uname" title="' + data["Comment"] +'" style="cursor:pointer">' + data["SignedOffOn"] + '</span>';
                }
                
            }
        },{
            // render action buttons in the last column
            targets: [8],
            render: function (a, b, data, d) {
                var btn = "";
                btn += '<button type="button" id="inc-report-' + data["AccidentIncidentId"] + '" class="btn btn-sm btn-dark mb-1" onclick="adminReviewAccidentIncidentHandler.showIncidentDetails(this)"><i class="fa fa-fw fa-eye"></i><span> View</span></button> '
                return btn;
            }
        }],
    });

    return {
        init: init,
        refreshTable: refreshTable,
        adminAccidentIncidentsTable: adminAccidentIncidentsTable
    }

})();

var adminReviewAccidentIncidentHandler = (function () {
    var $modal = $("#adminReviewAccidentIncidentModal");
    var $modalTitle = $("#adminReviewAccidentIncidentModal .modal-title");
    var $alert = $("#adminReviewAccidentIncidentErrorMessage");
    var $loader = $("#divAdminReviewAccidentIncidentModalContainer #loader");
    var $divQueTab = $("#divAdminReviewAccidentIncidentModalContainer #divAdminReviewAccidentIncidentQuestionTab");
    var $form = $("#divAdminReviewAccidentIncidentQuestionTab form");
    var $signOffBtn = $("#btnSigOffAccident");

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
        var reportTable = adminAccidentIncidentReportHandler.adminAccidentIncidentsTable;
        var rowData = reportTable.row(btn.closest('tr')).data();
        aiID = rowData["AccidentIncidentId"];
        resID = rowData["ResponseId"];
        isSignedOff = rowData["IsSignedOff"] ? 1:0;
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

        if (isSignedOff == 1)
            $signOffBtn.hide();
        else
            $signOffBtn.show();
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

        html += "<hr/><div class='form-group col-md-9'>"
        html += "<h5 for='cmnt_txtArea'>Sign-Off comments</h5>"
        if (isSignedOff == 1) {
            html += "<textarea disabled class='form-control mb-4' id='cmnt_txtArea' name='cmnt_txtArea' rows='4'>" + comment + "</textarea>";
            html += "<div><label><b>Signed-Off On:</b>" + signedOffOn + "</label></div>";
            html += "<div><label><b>Signed-Off By:</b>" + signedOfBy + "</label></div>";
        }else
            html += "<textarea class='form-control' id='cmnt_txtArea' name='cmnt_txtArea' rows='4'></textarea>";
        html += "</div>"

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
                    questionTemplate += "<input disabled type='text' class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input disabled type='text' class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' placeholder='Optional'>";
                questionTemplate += "</div>"
                break;
            case "textarea": //text area template
                questionTemplate = "<div class='form-group col-md-9 q-box' data-qid='" + qid + "' data-q-mandatory='" + isMandatory + "' data-q-type='" + type + "'>"
                questionTemplate += "<h5 for='resp_txt_" + seq + "'>" + seq + ". " + text + "</h5>"
                if (isMandatory)
                    questionTemplate += "<textarea disabled class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' rows='3' placeholder='Required'></textarea>";
                else
                    questionTemplate += "<textarea disabled data-mandatory='0' class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' rows='3' placeholder='Optional'></textarea>";

                questionTemplate += "</div>"
                break;
            case "single"://radio button template

                var options = options.split('|');
                var optionTemplate = "";
                for (var j = 0; j < options.length; j++) {
                    optionTemplate += "<div class='form-check'><input disabled class='form-check-input' type='radio' id='a_rv_que_" + qid + "_" + j + "' name='a_rv_que_" + qid + "'  value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='a_rv_que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
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
                    optionTemplate += "<div class='form-check'><input disabled class='form-check-input' type='checkbox' id='a_rv_que_" + qid + "_" + j + "' name='a_rv_que_" + qid + "' value='" + options[j] + "'>";
                    optionTemplate += "<label class='form-check-label' for='a_rv_que_" + qid + "_" + j + "'> " + options[j] + " </label></div>"
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
                    questionTemplate += "<input type='text' disabled class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' placeholder='Required'>";
                else
                    questionTemplate += "<input type='text' disabled class='form-control' id='a_rv_que_" + qid + "' name='a_rv_que_" + qid + "' placeholder='Optional'>";
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
                var qName = "a_rv_que_" + res[i].queid;
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

    //function to sign off reported accident/incident
    $signOffBtn.click(function (e) {
        e.preventDefault();
        if (confirm("Are you sure, you want to sign-off?")) {
            UTILS.disableButton($signOffBtn);
            UTILS.Alert.hide($alert);
            var url = hdnBaseUrl + "Report/SignOffAccidentIncident";
            var data = {
                aiid: resID,
                adminComments: $('#cmnt_txtArea').val()
            }
            UTILS.makeAjaxCall(url, data, function (e) {
                //var obj = $.parseJSON(e);
                if (e.success > 0) {
                    UTILS.Alert.show($alert, "success", "Data saved successfully");
                    $signOffBtn.hide();
                    $('#cmnt_txtArea').attr('disabled', 'disabled')
                    adminAccidentIncidentReportHandler.refreshTable();
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to save data. Please try again later.");
                }
                UTILS.resetButton($signOffBtn);
            }, function (e) {
                    UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
                    UTILS.resetButton($signOffBtn);
            });
        }
    })

    return {
        showIncidentDetails: showIncidentDetails
    }
})();
