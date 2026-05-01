$(function () {
    UTILS.activateMenuNavigationLink('manageRALink');
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-edit-ra');
    $('[data-toggle="tooltip"]').tooltip();
});

var manageRAHandler = (function () {

    var $course = $('#txtRAModule')
    var $searchBtn = $('#searchRAModuleBtn')
    var $clearSearchBtn = $('#clearSearchRAModuleBtn')

    var moduleId = 0;

    var moduleTable = $('#riskAssessmentModuleList').DataTable({
        lengthChange: false,
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + 'RiskAssessment/LoadRAModuleList',
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $course.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true },
        ],
        columnDefs: [ {
            // render action buttons in the last column
            targets: [1], render: function (a, b, data, d) {
                return '<button type="button" id="update-ra-' + data["ModuleID"] + '" class="btn btn-sm btn-dark mb-1" onclick="manageRAHandler.showRAQuestions(this)"><i class="fa fa-fw fa-edit"></i><span> Edit</span></button> '
                // + '<button type="button" id="set-notification-' + data["ModuleID"] + '" class="btn btn-sm btn-dark mb-1" onclick="manageModuleNotificationSettings.showModuleNotificationPopUp(this)"><i class="fa fa-fw fa-envelope"></i> <span>Notification</span></button>'
            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        moduleTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $course.val('');
        moduleTable.draw();
    });

    // function to redirect to list of module's RA questions
    function showRAQuestions(btn) {
        var moduleId = btn.id.split('-').pop();
        window.location.href = hdnBaseUrl + "RiskAssessment/EditRiskAssessment/" + moduleId;
    }

    return {
        showRAQuestions: showRAQuestions
    }
})();
