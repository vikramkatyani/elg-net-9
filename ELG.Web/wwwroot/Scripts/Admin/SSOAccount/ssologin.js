
var loading = false;


function removeCookieAlert() {
    $('.cookie-header').hide();
}
var loginHandler = (function () {
    var $container = $('#learnerPageContent');
    var $form = $('#learnerLoginForm');
    var $userName = $('#ssoInputEmail');
    var $submitButton = $('#btnLogin');

    //login button click
    $submitButton.click(function (e) {
        e.preventDefault();
        var btn = $(this);
        var userName = $userName.val();

        if (!loading) {
            UTILS.disableButton(btn);
            if (validate(userName)) {
                var url = hdnBaseUrl + "SSOAccount/ValidateUser";
                var data = {
                    id: userName
                }
                UTILS.makeAjaxCall(url, data, function (response) {
                    if (response.Err == 0)
                        window.location.href = response.Url;
                    else if (response.Err == 1) {
                        showError("Email not registered.");
                    } else {
                        showError("Something went wrong. Please try again later.");
                    }
                        
                    UTILS.resetButton(btn);
                }, function (er) {
                    UTILS.resetButton(btn);
                    showError(er);
                });
            } else {
                showError("Please enter valid email adress");
                UTILS.resetButton(btn);
            }
        }
    });

    function validate(emailAddress) {
        var pattern = new RegExp(/^[+a-zA-Z0-9._'-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/i);
        return pattern.test(emailAddress);
    }

})();



function showError(message) {
    $alertBox = $('#divErrorMessage');
    $alertBox.html(message);
    $alertBox.show()
}