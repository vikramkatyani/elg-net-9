$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-configure-module');
    //configureModuleHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var configureModuleHandler = (function () {

    var $course = $('#txtModule')
    var $searchBtn = $('#searchModuleBtn')
    var $clearSearchBtn = $('#clearSearchModuleBtn')

    var $updateScoreModal = $('#updateScoreModal');
    var $updateModuleTitle = $('#updateScoreModalLabel');
    var $scoreBox = $('#txtUpdateModuleScore');
    var $freqBox = $('#txtUpdateModuleFrequency');
    var $completionDaysBox = $('#txtUpdateModuleCompletionDays');
    var $updateScoreBtn = $('#btnUpdateModuleScore');
    var $updateScoreMessage = $('#updateScoreMessage');

    var moduleId = 0;

    var moduleTable = $('#configureModuleList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + 'CourseManagement/LoadModuleData',
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
            { "data": "PassingMarks", "name": "c.intPassmark", "autoWidth": true },
            { "data": "CompletionDays", "name": "c.intDaysForCompletion", "autoWidth": true },
            { "data": "Frequency", "name": "c.inttutorialfrequency", "autoWidth": true }
        ],
        columnDefs: [{
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                if (data["CompletionDays"] > 0)
                    return '<span>' + data["CompletionDays"] + ' days</span>';
                else
                    return '<span>Never</span>';
            }
        }, {
            // render action buttons in the last column
            targets: [3], render: function (a, b, data, d) {
                if (data["Frequency"] > 0)
                    return '<span>' + data["Frequency"] + ' days</span>';
                else
                    return '<span>Never</span>';
            }
        },{
                // render action buttons in the last column
                targets: [4], render: function (a, b, data, d) {
                    return '<button type="button" id="update-score-' + data["ModuleID"] + '" class="btn btn-sm btn-dark mb-1" onclick="configureModuleHandler.showUpdateScorePopUP(this)"><i class="fa fa-fw fa-wrench"></i><span>Configure</span></button> '
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

    //function to open modal for update score
    function showUpdateScorePopUP(btn) {
        UTILS.Alert.hide($updateScoreMessage)

        moduleId = btn.id.split('-').pop();
        var moduleName = moduleTable.row(btn.closest('tr')).data()["ModuleName"];
        var cmpDays = moduleTable.row(btn.closest('tr')).data()["CompletionDays"];
        //var minScore = moduleTable.row(btn.closest('tr')).data()["PassingMarks"];
        var freq = moduleTable.row(btn.closest('tr')).data()["Frequency"]; 
        $updateModuleTitle.html('Update - ' + moduleName);
        //$scoreBox.val(minScore);
        $freqBox.val(freq);
        $completionDaysBox.val(cmpDays);

        $updateScoreModal.modal('show');
    }

    //function to update minimum passing score for a module

    $updateScoreBtn.click(function () { updateScore(moduleId) });

    function updateScore(btn) {
        if (confirm("Are you sure you want to update configurations for this course?")) {            
            var url = hdnBaseUrl + 'CourseManagement/UpdateFrequency';
           // var score = $scoreBox.val();
            var freq = $freqBox.val();
            var cmpDays = $completionDaysBox.val();
            var data = { ModuleID: moduleId, Frequency: freq, CompletionDays: cmpDays}
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($updateScoreMessage, 'success', 'Configurations updated successfully');
                    moduleTable.draw();
                }                   
                else
                    UTILS.Alert.show($updateScoreMessage, 'error', 'Failed to update configurations');
            }, function (status) {
                console.log(status);
                UTILS.Alert.show($updateScoreMessage, 'success', 'Failed to update configuration, Please try again later');
            });
        }
    }
    return {
        showUpdateScorePopUP: showUpdateScorePopUP,
        updateScore: updateScore
    }
})();

var manageModuleNotificationSettings = (function () {
    var $notificationModal = $('#updateModuleNotificationModal');

    function showModuleNotificationPopUp(btn) {
        $notificationModal.modal('show');
    }
    return {
        showModuleNotificationPopUp: showModuleNotificationPopUp
    }
})();