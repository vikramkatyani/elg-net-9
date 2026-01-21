
var loading = false;

function removeCookieAlert() {
    var cookieHeader = document.querySelector('.cookie-header');
    if (cookieHeader) cookieHeader.style.display = 'none';
}

// Password toggle
const togglePassword = document.querySelector('#togglePassword');
const password = document.querySelector('#adminLoginForm #Password');
if (togglePassword && password) {
    togglePassword.addEventListener('click', function () {
        const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
        password.setAttribute('type', type);
        this.classList.toggle('fa-eye-slash');
    });
}

// Form validation (vanilla JS)
function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function validateLoginForm(form) {
    var email = form.querySelector('#Email').value.trim();
    var pwd = form.querySelector('#Password').value.trim();
    if (!email || !pwd) return false;
    if (!isValidEmail(email)) return false;
    return true;
}

function serializeForm(form) {
    var obj = {};
    Array.from(form.elements).forEach(function (el) {
        if (el.name && !el.disabled) obj[el.name] = el.value;
    });
    return obj;
}

function makeAjaxCall(url, data, success, error) {
    var xhr = new XMLHttpRequest();
    xhr.open('POST', url, true);
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    var resp = JSON.parse(xhr.responseText);
                    success(resp);
                } catch (e) {
                    error('Invalid server response');
                }
            } else {
                error('Request failed');
            }
        }
    };
    var params = Object.keys(data).map(function (k) {
        return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]);
    }).join('&');
    xhr.send(params);
}

function disableButton(btn) {
    btn.disabled = true;
    btn.setAttribute('data-loading-text', 'Logging...');
}
function resetButton(btn) {
    btn.disabled = false;
    btn.setAttribute('data-loading-text', 'Login');
}

document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('adminLoginForm');
    var btnLogin = document.getElementById('btnLogin');
    var btnLoginLink = document.getElementById('btnLoginLink');
    
    // Easy Login button handler
    if (btnLoginLink) {
        btnLoginLink.addEventListener('click', function (e) {
            e.preventDefault();
            window.location.href = window.hdnBaseUrl ? window.hdnBaseUrl + 'Account/GenerateLoginLink' : 'Account/GenerateLoginLink';
        });
    }
    
    if (form && btnLogin) {
        btnLogin.addEventListener('click', function (e) {
            e.preventDefault();
            if (!loading) {
                disableButton(btnLogin);
                if (validateLoginForm(form)) {
                    var url = window.hdnBaseUrl ? window.hdnBaseUrl + 'Account/Login' : 'Account/Login';
                    var data = serializeForm(form);
                    makeAjaxCall(url, data, function (response) {
                        if (response.Err == 0)
                            window.location.href = response.Url;
                        else
                            showError(response.Message);
                        resetButton(btnLogin);
                    }, function (er) {
                        resetButton(btnLogin);
                        showError(er);
                    });
                } else {
                    showError('Please enter valid email and password');
                    resetButton(btnLogin);
                }
            }
        });
    }
});


var validateCompanyHandler = (function () {
    //var $company = $('#companyNumberBox');
    //var $companyBtn = $('#validateCompanyBtn');
    //var $validationForm = $('#formCompanyValidation');

    //validate company button click

    function validateSelectedCompany(btn) {
        var companyNumber = btn.id.split('-').pop();
        if (!loading) {
            UTILS.disableButton($(btn));
            var url = hdnBaseUrl + "Account/ValidateAdminCompany";
                var data = {
                    companyNumber: companyNumber
                }
                UTILS.makeAjaxCall(url, data, function (response) {
                    if (response.Err == 0)
                        window.location.href = response.Url;
                    else if (response.Err == 1)
                        showError(response.Message);
                    else
                        showError("Invalid company number");
                    UTILS.resetButton($(btn));
                }, function (er) {
                    UTILS.resetButton($(btn));
                    showError(er);
                });
        }

    }
    return {
        validateSelectedCompany: validateSelectedCompany
    }
    //$companyBtn.click(function (e) {
    //    e.preventDefault();
    //    var btn = $(this);
    //    if (!loading) {
    //        UTILS.disableButton(btn);
    //        if ($validationForm.valid()) {
    //            var url = hdnBaseUrl + "Account/ValidateCompany";
    //             var data= {
    //                companyNumber: $company.val()
    //            }
    //            UTILS.makeAjaxCall(url, data, function (response) {
    //                if (response.Err == 0)
    //                    window.location.href = response.Url;
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
    $alertBox.html(message);
    $alertBox.show()
}