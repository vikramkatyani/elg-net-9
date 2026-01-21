var loading = false;

//const username = document.querySelector('#learnerForgetpwd #Email');
//username.addEventListener('keydown', function (event) {
//    if (event.ctrlKey && (event.key === 'c' || event.key === 'v' || event.key === 'x')) {
//        event.preventDefault();
//    }
//});

var forgetPwdHandler = (function () {
    var $container = $('#learnerPageContent');
    var $form = $('#learnerForgetpwd');
    var $userName = $('#Email');
    var $submitButton = $('#btnResetPassword');

    //login button click
    $submitButton.click(function (e) {
        e.preventDefault();
        var btn = $(this)
        if (!loading) {
            UTILS.disableButton(btn);
            if ($form.valid()) {
                var url = hdnBaseUrl + "Account/ForgetPassword";
                var data = $form.serialize();
                UTILS.makeAjaxCall(url, data, function (response) {
                    if (response.Err == 0 && response.Url !="")
                        window.location.href = response.Url;
                    else if (response.Err == 0 && response.Url != "")
                        showMessage(response.Message);
                    else
                        showError(response.Message);
                    UTILS.resetButton(btn);
                }, function (er) {
                    UTILS.resetButton(btn);
                    showError(er);
                });
            } else {
                showError("Please enter valid email.");
                UTILS.resetButton(btn);
            }
        }
    });

})();

var validateForgotCompanyHandler = (function () {
    var $company = $('#companyNumberBox');
    var $companyBtn = $('#validateCompanyBtn');
    var $validationForm = $('#formCompanyValidationfrgtPwd');

    function validateForgotSelectedCompany(btn) {
        var companyNumber = btn.id.split('-').pop();
        if (!loading) {
            UTILS.disableButton($(btn));
            if ($validationForm.valid()) {
                var url = hdnBaseUrl + "Account/ValidateCompany_ForgetPassword";
                var data = {
                    companyNumber: companyNumber
                }
                UTILS.makeAjaxCall(url, data, function (response) {
                    if (response.Err == 0)
                        showMessage(response.Message);
                    else if (response.Err == 1)
                        showError(response.Message);
                    else
                        showError("Please enter a valid company number");
                    UTILS.resetButton($(btn));
                }, function (er) {
                    UTILS.resetButton($(btn));
                    showError(er);
                });
            } else {
                showError("Please enter company number");
                UTILS.resetButton($(btn));
            }
        }

    }
    return {
        validateForgotSelectedCompany: validateForgotSelectedCompany
    }

    ////validate company button click
    //$companyBtn.click(function (e) {
    //    e.preventDefault();
    //    var btn = $(this);
    //    if (!loading) {
    //        UTILS.disableButton(btn);
    //        if ($validationForm.valid()) {
    //            var url = hdnBaseUrl + "Account/ValidateCompanyForgetPassword";
    //            var data = {
    //                companyNumber: $company.val()
    //            }
    //            UTILS.makeAjaxCall(url, data, function (response) {
    //                if (response.Err == 0)
    //                    showMessage(response.Message);
    //                else if (response.Err == 1)
    //                    showError(response.Message);
    //                else
    //                    showError("Please enter a valid company number");
    //                UTILS.resetButton(btn);
    //            }, function (er) {
    //                UTILS.resetButton(btn);
    //                showError(er);
    //            });
    //        } else {
    //            showError("Please enter company number");
    //            UTILS.resetButton(btn);
    //        }
    //    }
    //});

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