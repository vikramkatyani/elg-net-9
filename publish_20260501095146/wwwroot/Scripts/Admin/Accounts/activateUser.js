var loading = false;

var activateUserHandler = (function () {
    var $form = $('#learnerActivateUserForm');
    var $submitButton = $('#btnlearnerActivateUser');

    //login button click
    $submitButton.click(function (e) {
        e.preventDefault();
        var btn = $(this)
        if (!loading) {
            UTILS.disableButton(btn);
            if ($form.valid()) {
                var url = hdnBaseUrl + "Account/ActivateUserPassword";
                var data = $form.serialize();
                UTILS.makeAjaxCall(url, data, function (response) {
                    if (response.Err == 0)
                    {
                        $("#divsuccess").show();
                        $("#learnerActivateUserForm").hide();
                    }
                    else
                        showError(response.Message);
                    UTILS.resetButton(btn);
                }, function (er) {
                    UTILS.resetButton(btn);
                    showError(er);
                });
            }
        }
    });
})();

function showError(message) {
    $alertBox = $('#divErrorMessage');
    $alertBox.removeAttr("class").attr("class", "alert alert-danger collapse");
    $alertBox.html(message);
    $alertBox.show()
}

function showMessage(message) {
    $alertBox = $('#divErrorMessage');
    $alertBox.removeAttr("class").attr("class", "alert alert-success collapse");
    $alertBox.html(message);
    $alertBox.show()
}