$(function () {
    UTILS.activateNavigationLink('settingLink');
    UTILS.activateMenuNavigationLink('menu-set-notification');
    $('[data-toggle="tooltip"]').tooltip();
    notificationHandler.init();
})


var notificationHandler = (function () {
    var $chkCourseAssignmentEmailToLearner = $('#chkCourseAssignmentEmailToLearner');
    var $alert = $('#message_notificationSettings');

    $chkCourseAssignmentEmailToLearner.change(function (e) {
        e.preventDefault();
        if (confirm("Are you sure you want to update notification settings?")) {
            var configuration = {
                SendCourseAssignmentMailToLearner: $chkCourseAssignmentEmailToLearner.is(":checked")
            }
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + "Home/UpdateCourseAssignmentEmailSettings",
                dataType: 'json',
                data: configuration,
                success: function (result) {
                    if (result.success > 0) {
                        UTILS.Alert.show($alert, "success", 'Notification settings updated successfully.');
                    }
                    else {
                        UTILS.Alert.show($alert, "error", 'Failed to update notification settings.');
                    }
                },
                error: function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
                }
            });
        }
    });


    function init() {
        $.ajax({
            type: 'post',
            url: hdnBaseUrl + "home/LoadModuleAssignmentData",
            dataType: 'json',
            success: function (res) {
                $chkCourseAssignmentEmailToLearner.attr('checked', res.config.SendCourseAssignmentMailToLearner)
            },
            error: function (status) {
                console.log(status);
                UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
            }
        });
    }

    return {
        init: init
    }
})();


//var notificationHandler = (function () {
//    var $chkPassedTestToLocationAdmin = $('#chkPassedTestToLocationAdmin');
//    var $chkPassedTestToDepartmentAdmin = $('#chkPassedTestToDepartmentAdmin');
//    var $chkRACompletionToLocationAdmin = $('#chkRACompletionToLocationAdmin');
//    var $chkRACompletionTestToDepartmentAdmin = $('#chkRACompletionTestToDepartmentAdmin');
//    var $chkFailedTestToLocationAdmin = $('#chkFailedTestToLocationAdmin');
//    var $chkFailedTestToDepartmentAdmin = $('#chkFailedTestToDepartmentAdmin');
//    var $chkOverdueTestToLocationAdmin = $('#chkOverdueTestToLocationAdmin');
//    var $chkOverdueTestToDepartmentAdmin = $('#chkOverdueTestToDepartmentAdmin');
//    var $chkOverdueRAToLocationAdmin = $('#chkOverdueRAToLocationAdmin');
//    var $chkOverdueRAToDepartmentAdmin = $('#chkOverdueRAToDepartmentAdmin');
//    var $updateBtn = $('#btnUpdateModuleNotification');
//    var $alert = $('#message_notificationSettings');

//    $updateBtn.click(function (e) {
//        e.preventDefault();
//        if (confirm("Are you sure you want to update notification settings?")) {
//            var configuration = {
//                SendCoursePassedToLocationAdmin: $chkPassedTestToLocationAdmin.val(),
//                SendCoursePassedToDepartmentAdmin: $chkPassedTestToDepartmentAdmin.val(),
//                SendCourseFailedToLocationAdmin: $chkFailedTestToLocationAdmin.val(),
//                SendCourseFailedToDepartmentAdmin: $chkFailedTestToDepartmentAdmin.val(),
//                SendRACompletionToLocationAdmin: $chkRACompletionToLocationAdmin.val(),
//                SendRACompletionToDepartmentAdmin: $chkRACompletionTestToDepartmentAdmin.val(),
//                SendCourseOverdueToLocationAdmin: $chkOverdueTestToLocationAdmin.val(),
//                SendCourseOverdueToDepartmentAdmin: $chkOverdueTestToDepartmentAdmin.val(),
//                SendRAOverDueToLocationAdmin: $chkOverdueRAToLocationAdmin.val(),
//                SendRAOverDueToDepartmentAdmin: $chkOverdueRAToDepartmentAdmin.val()
//            }
//            $.ajax({
//                type: 'post',
//                url: hdnBaseUrl + "Home/UpdateNotificationSettings",
//                dataType: 'json',
//                data: configuration,
//                success: function (result) {
//                    if (result.success > 0) {
//                        UTILS.Alert.show($alert, "success", 'Notification settings updated successfully.');
//                    }
//                    else {
//                        UTILS.Alert.show($alert, "error", 'Failed to update notification settings.');
//                    }
//                },
//                error: function (status) {
//                    console.log(status);
//                    UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
//                }
//            });
//        }
//    });


//    function init() {
//        $.ajax({
//            type: 'post',
//            url: hdnBaseUrl + "home/LoadModuleNotificationData",
//            dataType: 'json',
//            success: function (res) {
//                $chkPassedTestToLocationAdmin.val(),
//                    $chkPassedTestToDepartmentAdmin.val(),
//                    $chkFailedTestToLocationAdmin.val(),
//                    $chkFailedTestToDepartmentAdmin.val(),
//                    $chkRACompletionToLocationAdmin.val(),
//                    $chkRACompletionTestToDepartmentAdmin.val(),
//                    $chkOverdueTestToLocationAdmin.val(),
//                    $chkOverdueTestToDepartmentAdmin.val(),
//                    $chkOverdueRAToLocationAdmin.val(),
//                    $chkOverdueRAToDepartmentAdmin.val()
//            },
//            error: function (status) {
//                console.log(status);
//                UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
//            }
//        });
//    }

//    return {
//        init: init
//    }
//})();