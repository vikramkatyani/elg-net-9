$(function () {
    UTILS.activateNavigationLink('settingLink');
    UTILS.activateMenuNavigationLink('menu-set-reminder');
    $('[data-toggle="tooltip"]').tooltip();
    moduleRemiderHandler.init();
})

var moduleRemiderHandler = (function () {
    var $daysBefore = $('#txtFirstReminder');
    var $frequency = $('#txtReminderFrequency');
    var $maxReminder = $('#ddlMaxReminder');
    var $updateBtn = $('#btnUpdatReminderNotification');
    var $alert = $('#message_reminderSettings');

    $updateBtn.click(function (e) {
        e.preventDefault();
        if (confirm("Are you sure you want to update reminder settings?")) {
            var configuration = {
                DaysBefore: $daysBefore.val(),
                Frequency: $frequency.val(),
                MaxReminders: $maxReminder.val()
            }
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + "Home/UpdateTestReminder",
                dataType: 'json',
                data: configuration,
                success: function (result) {
                    if (result.success > 0) {
                        UTILS.Alert.show($alert, "success", 'Reminder settings updated successfully.');
                    }
                    else {
                        UTILS.Alert.show($alert, "error", 'Failed to update reminder settings.');
                    }
                },
                error: function (status) {
                    console.log(status);
                    UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
                }
            });
        }
        return false;
    });


    function init() {
        $.ajax({
            type: 'post',
            url: hdnBaseUrl + "home/LoadTestReminderData",
            dataType: 'json',
            success: function (res) {
                $daysBefore.val(res.config.DaysBefore);
                $frequency.val(res.config.Frequency);
                $maxReminder.val(res.config.MaxReminders);
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