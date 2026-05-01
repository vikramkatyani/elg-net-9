
var addRAGroupHandler = (function () {
    var $modal = $('#addRAGroupModal');
    var $groupName = $('#txtGroupName');
    var $alert = $('#addRAGroupMessage');
    var $createBtn = $('#btnAddNewRAGroup');

    function showCreatePopUp(courseid) {
        $modal.modal('show')
    }

    $createBtn.click(function () {
        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "RiskAssessment/CreateRAGroup"
            var data = {
                GroupName: $groupName.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Group created successfully");
                }
                else if (res.success == 0) {
                    UTILS.Alert.show($alert, "error", "Group already exists.")
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add group.")
                }

                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                    UTILS.Alert.show($alert, "error", "Failed to add group. Please try again later")
                    UTILS.resetButton($createBtn);
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "Group name can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($groupName.val() != null && $groupName.val().trim() != '') {
            return true
        }
        else {
            return false;
        }
    }
    return {
        showCreatePopUp: showCreatePopUp
    }
})();