$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-configure-module');
    resetTrainingRenewalDateHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var resetTrainingRenewalDateHandler = (function () {

    var $inputHolder = $("#reset_date_input_holder");
    //var $resetDateHolder = $("#reset_date_holder");
    var $resetDate = $("#spn_reset_date");
    var $alert = $("#divMessage_resetLearning");

    var $modal = $("#updateTrainingResetDateModal");
    var $resetDate_input = $("#updateTrainingResetDateModal #txt_new_reset_date");
    var $resetOTP = $("#updateTrainingResetDateModal #txtUpdateResetDateOTP");
    var $updateResetDate = $("#updateTrainingResetDateModal #btn_update_reset_date");
    var $sendTrainingResetOTP = $("#updateTrainingResetDateModal #btnSendTrainingResetOTP");
    var $alert_modal = $("#updateTrainingResetDateModal #updateResetDateModal_message");
    $resetDate_input.datepicker({ dateFormat: 'yy-mm-dd', minDate: new Date() });

    var otp_txn_id = "";

    $sendTrainingResetOTP.click(function (e) {
        e.preventDefault();
        otp_txn_id = "";
        var url = hdnBaseUrl + "CourseManagement/SendTrainingResetDateOTP";
        UTILS.makeAjaxCall(url, {}, function (e) {
            if (e.otp_txn != undefined && e.otp_txn != null && e.otp_txn != '') {
                otp_txn_id = e.otp_txn;
                UTILS.Alert.show($alert_modal, "success", "OTP has been sent to your registered email address");
            } else {
                UTILS.Alert.show($alert_modal, "error", "Failed to send OTP . Please try again later.");
            }
        }, function (e) {
            UTILS.Alert.show($alert_modal, "error", "Something went wrong. Please try again later.");
        });
    });

    $updateResetDate.click(function (e) {
        e.preventDefault();
        var otp = $resetOTP.val();

        if (otp == null || otp == '') {
            UTILS.Alert.show($alert, "warning", "Please enter OTP");
            return false;
        }

        if (confirm("Are you sure you want to update training reset date?")) {
            var url = hdnBaseUrl + "CourseManagement/UpdateTrainingResetDate";
            var data = {
                resetDate: $resetDate_input.val(),
                otp: otp,
                otp_txn: otp_txn_id
            }
            UTILS.makeAjaxCall(url, data, function (e) {
                if (e.success == 1 ) {
                    UTILS.Alert.show($alert_modal, "success", "Training Reset Date updated successfully");
                    init();
                } else if(e.success == 2) {
                    UTILS.Alert.show($alert_modal, "error", "Incorrect OTP.");
                } else {
                    UTILS.Alert.show($alert_modal, "error", "Failed to update Training Reset Date . Please try again later.");
                }
            }, function (e) {
                UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
            });
        }
    });

    function init() {
        getTrainingResetDate();
    }

    function getTrainingResetDate() {
        var url = hdnBaseUrl + "CourseManagement/GetTrainingResetDate";
        UTILS.makeAjaxCall(url, {}, function (e) {
            if (e.resetDate != null) {
                $resetDate.html(e.resetDateString);
            } else {
                $resetDate.html('');
            }
        }, function (e) {
            UTILS.Alert.show($alert, "error", "Something went wrong. Please try again later.");
        });
    }

    function showTrainingResetPopUP() {
        $modal.modal({
            backdrop: 'static',
            keyboard: true
        });
    }

    return {
        init: init,
        showTrainingResetPopUP: showTrainingResetPopUP
    }
})()