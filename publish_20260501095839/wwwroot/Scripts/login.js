
var loginHandler = (function () {
    var $form = $('#adminLoginForm');
    var $userName = $('#Email');
    var $password = $('#Password');
    var $submitButton = $('#btnLogin');

    $submitButton.click(function () {
        if ($form.valid()) {
            $.ajax({
                url: hdnBaseUrl + "Account/Login",
                type: "Post",
                data: $form.serialize(),
                success: function (e) {
                    alert("success")
                },
                error: function (er) {
                }
            });
        }
    });

})();

function loginSubmit() {
    var form = $('#adminLoginForm');

    if (form.valid()) {
        $.ajax({
            url: hdnBaseUrl + "Account/Login",
            type: "Post",
            data: form.serialize(),
            success: function (e) {
                alert("success")
            },
            error: function (er) {
            }
        });
    }
}

//Add onkeypress event on login password textbox
function LoginPasswordKeypress(e) {
    if (e.keyCode === 13) {
        e.preventDefault(); // Ensure it is only this code that run
        loginSubmit();
    }
}

$('.modal .close').click(function () {
    $(".modal-backdrop").not(':first').remove();
});

//forget password
function showForgetpasswordpanel() {
    $('#forgotpasswordModal').modal('show');
}

function forgetPasswordSubmit() {
    var form = $('#forgotpasswordform');

    if (form.valid()) {
        showloader();
        $.ajax({
            url: hdnBaseUrl + "Account/ForgotPassword",
            type: "Post",
            data: form.serialize(),
            success: function (response) {
                hideloader();
                if (response.Success != null && response.Success != undefined && response.Success) {
                    $("#forgotpasswordModal .alert-success").show();
                    $("#forgotpasswordModal #lblSuccessMsg").text(response.Messsge);
                    $("#forgotpasswordModal .validation-summary-errors.alert.alert-danger").hide();
                }
                else {
                    $("#forgotpasswordModal").html(response);
                }
            },
            error: function (er) {
                hideloader();
            }
        });
    }
}

//Registration
function showRegistration() {
    var container = $('#registerIndividualModal');
    showloader();
    $.ajax({
        url: hdnBaseUrl + "Account/Registration",
        type: 'GET',
        cache: false,
        success: function (result) {
            container.html(result);
            hideloader();
            $('#registerIndividualModal').modal('show');
        },
        error: function (er) {
            hideloader();
        }
    });
}

function showRegistrationPanel() {
    showRegistration();
}

function registrationSubmit() {
    var form = $('#formRegisterUser');
    if (form.valid()) {
        showloader();
        $.ajax({
            url: hdnBaseUrl + "Account/Registration",
            type: "Post",
            data: form.serialize(),
            success: function (response) {
                hideloader();
                if (response.Success != null && response.Success != undefined && response.Success) {
                    $("#registerIndividualModal .alert-success").show();
                    $("#registerIndividualModal #lblSuccessMsg").text(response.Register_Success);
                    $("#registerIndividualModal .validation-summary-errors.alert.alert-danger").hide();
                }
                else {
                    $("#registerIndividualModal").html(response);
                }
            },
            error: function (er) {
                hideloader();
            }
        });
    }
}