var selectCourseGUID = '';

$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-que-widget');
    $('[data-toggle="tooltip"]').tooltip();
    courseWidgetHandler.init();
});

var courseWidgetHandler = (function () {
    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        return (results[1]);
    }
    selectCourseGUID = $.urlParam('id');
    var courseName = decodeURIComponent($.urlParam('c'));
    var $courseHeader = $('#coursename-header');
    var $backBtn = $('#bckToCourseBtn');
    var $createBtn = $('#btnAddCourseWidgetPopUp')
    var $alert = $('#divMessage_coursewidget');

    $courseHeader.html(courseName);

    $backBtn.on("click", function () {
        window.location.href = hdnBaseUrl + "QueWidget/Course";
    });

    function init() {
        $(document).on('click', '#btnAddCourseWidgetPopUp', function (e) {
            e.preventDefault();
        addCourseWidgetHandler.showCreatePopUp();
    });
    }

    var courseWidgetTable = $('#courseWidgetList').DataTable({
        processing: true,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-2x text-primary"></i> Loading...',
            emptyTable: 'No record(s) found.'
        },
        serverSide: true,
        filter: false,
        orderMulti: false,
        lengthChange: false,
        ajax: {
            url: 'LoadCourseWidgetData',
            type: 'POST',
            datatype: 'json',
            data: function (data) {
                data.SearchText = selectCourseGUID;
            },
            error: function (xhr, error, code) {
                console.error(xhr);
                alert('Oops! Something went wrong. Please try again.');
            }
        },
        columns: [
            { data: 'QueType', name: 'q.widget_que_type', autoWidth: true },
            { data: 'QuesText', name: 'q.widget_que_text', autoWidth: true },
            { data: 'QuesRef', name: 'q.widget_que_ref', autoWidth: true },
            { data: null, orderable: false, searchable: false }
        ],
        columnDefs: [
            {
                targets: [0],
                render: function (a, b, data) {
                    switch (parseInt(data.QueType)) {
                        case 1: return '<span class="badge bg-info text-dark">TIW</span>';
                        case 2: return '<span class="badge bg-success">MAC</span>';
                        case 3: return '<span class="badge bg-warning text-dark">BPC</span>';
                        default: return '<span class="text-muted">Unknown</span>';
                    }
                }
            },
            {
                targets: [3],
                className: 'text-center',
                render: function (a, b, data) {
                    const guid = data.QueGUID;
                    return `
          <div class="dropdown">
            <button class="btn btn-sm rounded-circle" type="button"
                    id="actionDropdown-${guid}" data-bs-toggle="dropdown" aria-expanded="false"
                    style="width:2.5rem;height:2.5rem;">
              <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${guid}">
              <li>
                <a class="dropdown-item" href="#" id="show_widget_${guid}"
                   onclick="tagViewHandler.openTagPopUp(this)">
                  <i class="fa fa-fw fa-code me-2"></i>View Script
                </a>
              </li>
              <li>
                <a class="dropdown-item text-danger" href="#" id="delete_widget_${guid}"
                   onclick="deleteWidgetHandler.confirmDelete('${guid}')">
                  <i class="fa fa-fw fa-trash me-2"></i>Delete
                </a>
              </li>
              <!-- Future: Preview/Edit options -->
              <!--
              <li><a class="dropdown-item" href="#"><i class="fa fa-eye me-2"></i>Preview</a></li>
              <li><a class="dropdown-item" href="#"><i class="fa fa-edit me-2"></i>Edit</a></li>
              -->
            </ul>
          </div>
        `;
                }
            }
        ],
        order: [[1, 'asc']]
    });

    return {
        init: init
    };

})();


var addCourseWidgetHandler = (function () {
    var $modal = $('#addCourseWidgetModal');
    var $widgetTypeDLL = $('#widgetTypeDLL');
    var $queTitle = $('#addCourseWidgetModal #txtWidgetTitle');
    var $queRef = $('#addCourseWidgetModal #txtWidgetRef');
    var $headerColor = $('#addCourseWidgetModal #txtHeaderColor');
    var $alert = $('#addCourseWidgetModal #addCourseWidgetMessage');
    var $createBtn = $('#addCourseWidgetModal #btnAddNewCourseWidget');
    var widgetType = 0;

    //que type 1
    var $queText = $('#addCourseWidgetModal #txtQue');
    var $queAfterText = $('#addCourseWidgetModal #txtQueAfter');
    var $showQueTextBeforeFR = $('#addCourseWidgetModal #chkShowFRQ');

    //que type 2
    var $queText_mac = $('#addCourseWidgetModal #txtQue_mac');
    var $modelAnswer_h = $('#addCourseWidgetModal #txtModelAnswer_mac');
    var $modelAnswer_r1 = $('#addCourseWidgetModal #txtModelAnswer_mac_res1');
    var $modelAnswer_r2 = $('#addCourseWidgetModal #txtModelAnswer_mac_res2');
    var $modelAnswer_r3 = $('#addCourseWidgetModal #txtModelAnswer_mac_res3');

    //que type 3
    var $queText_bpc = $('#addCourseWidgetModal #txtQue_bpc');
    var $lblAfterText_bpc = $('#addCourseWidgetModal #txtAfter_bpc');
    var $queAfterText_bpc = $('#addCourseWidgetModal #txtQueAfter_bpc');

    function clearpopUp() {
        $widgetTypeDLL.val('-1');
        $queText.val('');
        $queAfterText.val('');
        $queText_mac.val('');
        $modelAnswer_h.val('');
        $modelAnswer_r1.val('');
        $modelAnswer_r2.val('');
        $modelAnswer_r3.val('');
        $('.question-container').hide();
        $('.wdgt-field-container').hide();

    }

    $widgetTypeDLL.on('change', function () {
        widgetType = $(this).val();
        $('.wdgt-field-container').hide();
        $(".question-container").show();
        switch (widgetType) {
            case '1': $("#wdgt_tiw_field").show();
                break;
            case '2': $("#wdgt_mac_field").show();
                break;
            case '3': $("#wdgt_bpc_field").show();
                break;
            default: widgetType = 0;
                $('.question-container').hide();
                break;
        }
    })
    function showCreatePopUp() {
        clearpopUp();
        UTILS.Alert.hide($alert);
        $modal.modal('show')
    }

    $createBtn.click(function () {
        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "QueWidget/CreateWidget"
            var data = {}

            switch (widgetType) {
                case '1': data = {
                    CourseGUID: selectCourseGUID,
                    QuesText: $queText.val(),
                    QuesTitle: $queTitle.val(),
                    QuesRef: $queRef.val(),
                    AfterQuesText: $queAfterText.val(),
                    QueType: widgetType,
                    HeaderColor: $headerColor.val(),
                    ShowQueTextBeforeFR: $showQueTextBeforeFR.prop('checked')
                }
                    break;
                case '2': data = {
                    CourseGUID: selectCourseGUID,
                    QuesText: $queText_mac.val(),
                    QuesTitle: $queTitle.val(),
                    QuesRef: $queRef.val(),
                    QueModelAnswerHeading: $modelAnswer_h.val(),
                    QueModelAnswerResp_1: $modelAnswer_r1.val(),
                    QueModelAnswerResp_2: $modelAnswer_r2.val(),
                    QueModelAnswerResp_3: $modelAnswer_r3.val(),
                    QueType: widgetType,
                    HeaderColor: $headerColor.val()
                }
                    break;
                case '3': data = {
                    CourseGUID: selectCourseGUID,
                    QuesText: $queText_bpc.val(),
                    QuesTitle: $queTitle.val(),
                    QuesRef: $queRef.val(),
                    AfterQuesLabelText: $lblAfterText_bpc.val(),
                    AfterQuesText: $queAfterText_bpc.val(),
                    QueType: widgetType,
                    HeaderColor: $headerColor.val()
                }
                    break;
                default: data = {}
                    break;
            }

            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Widget created successfully");
                    $('#courseWidgetList').DataTable().draw();
                    clearpopUp();
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add Widget. Please try again later")
                }

                loading = false;
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "Question text can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {

        switch (widgetType) {
            case '1':
                if ($queText.val() != null && $queText.val().trim() != '' && parseInt(widgetType) > 0) {
                    return true
                }
                else {
                    return false;
                }
                break;
            case '2':
                if ($queText_mac.val() != null && $queText_mac.val().trim() != '' && parseInt(widgetType) > 0) {
                    return true
                }
                else {
                    return false;
                }
                break;
            case '3':
                if ($queText_bpc.val() != null && $queText_bpc.val().trim() != '' && parseInt(widgetType) > 0) {
                    return true
                }
                else {
                    return false;
                }
                break;
        }
        
    }
    return {
        showCreatePopUp: showCreatePopUp
    }
})();


var tagViewHandler = (function () {
    var $modal = $("#viewWidgetTagModal");
    var $tagBefore = $("#viewWidgetTagModal #viewtxtQue");
    var $tagAfter = $("#viewWidgetTagModal #viewtxtQueAfter");
    var $tagMac = $("#viewWidgetTagModal #viewtxtQue_mac");
    var $tagBefore_bpc = $("#viewWidgetTagModal #viewtxtQue_bpc");
    var $tagAfter_bpc = $("#viewWidgetTagModal #viewtxtQueAfter_bpc");
    //const widget_baseURL = 'http://localhost:61544';
    const widget_baseURL = 'https://learning.fraud-sentinel.com';


    var tagTemplate = `<!DOCTYPE html>
<html lang="en">
<head>
    <title></title>
    <script src="${widget_baseURL}/Scripts/widget/{{jsFile}}"></script>
</head>
<body>
<div id='{{divId}}'></div>`;

    var scriptTemplate = `<script> {{function}} </script>
    </body>
</html>`;

    function openTagPopUp(btn) {
        $('.script-holder').hide();
        var rowData = $('#courseWidgetList').DataTable().row(btn.closest('tr')).data();

        var widgetType = parseInt(rowData["QueType"]);

        let jsFile = "";
        let widgetCode = '';
        switch (widgetType) {
            case 1: jsFile = "widgetRender.js";
                widgetCode = 'tiw'
                var divTemplateBfr = tagTemplate;
                var divTemplateAfr = tagTemplate;
                var scriptTagBefore = scriptTemplate;
                var scriptTagAfter = scriptTemplate;

                divTemplateBfr = divTemplateBfr.replace(/{{divId}}/g, rowData["QueGUID"] + "_bfr");
                divTemplateBfr = divTemplateBfr.replace(/{{jsFile}}/g, jsFile+"?v=tiw_bfr");
                scriptTagBefore = scriptTagBefore.replace(/{{function}}/g, `widgetHandler.loadWidget('${rowData["QueGUID"]}', '${rowData["QueGUID"]}_bfr', 'first')`);

                divTemplateAfr = divTemplateAfr.replace(/{{divId}}/g, rowData["QueGUID"] + "_afr");
                divTemplateAfr = divTemplateAfr.replace(/{{jsFile}}/g, jsFile + "?v=tiw_bfr");
                scriptTagAfter = scriptTagAfter.replace(/{{function}}/g, `widgetHandler.loadWidget('${rowData["QueGUID"]}', '${rowData["QueGUID"]}_afr', 'after')`);

                //scriptTagBefore = scriptTagBefore.replace(/{{id}}/g, rowData["QueGUID"]);
                //scriptTagBefore = scriptTagBefore.replace(/{{divId}}/g, rowData["QueGUID"] + "_bfr");
                //scriptTagBefore = scriptTagBefore.replace(/{{viewmode}}/g, "first");

                //scriptTagAfter = scriptTagAfter.replace(/{{id}}/g, rowData["QueGUID"]);
                //scriptTagAfter = scriptTagAfter.replace(/{{divId}}/g, rowData["QueGUID"] + "_afr");
                //scriptTagAfter = scriptTagAfter.replace(/{{viewmode}}/g, "after");

                $tagBefore.val(divTemplateBfr + scriptTagBefore);
                $tagAfter.val(divTemplateAfr + scriptTagAfter);
                $("#div-scr-tiw").show();
                break;
            case 2: jsFile = "mac_widgetRender.js";
                widgetCode = 'mac'
                var divTemplateBfr = tagTemplate;
                var scriptTagBefore = scriptTemplate;

                divTemplateBfr = divTemplateBfr.replace(/{{divId}}/g, rowData["QueGUID"] + "_mac");
                divTemplateBfr = divTemplateBfr.replace(/{{jsFile}}/g, jsFile + "?v=mac_bfr");
                scriptTagBefore = scriptTagBefore.replace(/{{function}}/g, `macWidgetHandler.loadWidget('${rowData["QueGUID"]}')`);

                $tagMac.val(divTemplateBfr + scriptTagBefore);
                $("#div-scr-mac").show();
                break;
            case 3: jsFile = "bpc_widgetRender.js";
                widgetCode = 'bpc'
                var divTemplateBfr = tagTemplate;
                var divTemplateAfr = tagTemplate;
                var scriptTagBefore = scriptTemplate;
                var scriptTagAfter = scriptTemplate;

                divTemplateBfr = divTemplateBfr.replace(/{{divId}}/g, rowData["QueGUID"] + "_bfr");
                divTemplateBfr = divTemplateBfr.replace(/{{jsFile}}/g, jsFile + "?v=bpc_bfr");
                scriptTagBefore = scriptTagBefore.replace(/{{function}}/g, `bpcWidgetHandler.loadWidget('${rowData["QueGUID"]}', '${rowData["QueGUID"]}_bfr', 'first')`);

                divTemplateAfr = divTemplateAfr.replace(/{{divId}}/g, rowData["QueGUID"] + "_afr");
                divTemplateAfr = divTemplateAfr.replace(/{{jsFile}}/g, jsFile + "?v=bpc_bfr");
                scriptTagAfter = scriptTagAfter.replace(/{{function}}/g, `bpcWidgetHandler.loadWidget('${rowData["QueGUID"]}', '${rowData["QueGUID"]}_afr', 'after')`);

                $tagBefore_bpc.val(divTemplateBfr + scriptTagBefore);
                $tagAfter_bpc.val(divTemplateAfr + scriptTagAfter);
                $("#div-scr-bpc").show();
                break;

        }

        $modal.modal('show');
    }

    return {
        openTagPopUp: openTagPopUp
    }

})();


var deleteWidgetHandler = (function () {
    var $alert = $('#divMessage_coursewidget');

    function confirmDelete(widgetGuid) {
        event.preventDefault();
        
        // First get the count of linked responses
        var url = hdnBaseUrl + "QueWidget/GetWidgetResponseCount";
        var data = { WidgetGuid: widgetGuid };
        
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success > 0) {
                var count = res.count;
                var message = count > 0 
                    ? `This widget has ${count} linked response(s) in the system. Deleting this widget will also delete all associated responses. Are you sure you want to proceed?`
                    : 'Are you sure you want to delete this widget?';
                
                if (confirm(message)) {
                    deleteWidget(widgetGuid);
                }
            } else {
                alert('Unable to retrieve widget information. Please try again.');
            }
        }, function (err) {
            console.error(err);
            alert('An error occurred while checking widget responses. Please try again.');
        });
    }

    function deleteWidget(widgetGuid) {
        var url = hdnBaseUrl + "QueWidget/DeleteWidget";
        var data = { WidgetGuid: widgetGuid };
        
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success > 0) {
                UTILS.Alert.show($alert, "success", "Widget deleted successfully");
                $('#courseWidgetList').DataTable().draw();
            } else {
                UTILS.Alert.show($alert, "error", "Failed to delete widget. Please try again later");
            }
        }, function (err) {
            console.error(err);
            UTILS.Alert.show($alert, "error", "An error occurred while deleting the widget. Please try again.");
        });
    }

    return {
        confirmDelete: confirmDelete
    };

})();
