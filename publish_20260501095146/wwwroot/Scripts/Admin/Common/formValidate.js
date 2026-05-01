var ELGLMSVALIDATE = ELGLMSVALIDATE || {};

ELGLMSVALIDATE.VALIDATOR = ELGLMSVALIDATE.VALIDATOR || {};

ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES = {
    'VALIDATIONTYPE_EMAIL': "email",
    'VALIDATIONTYPE_BLANK': "blank",
    'VALIDATIONTYPE_NUMBER': "number",
    'VALIDATIONTYPE_DATE': "date",
    'VALIDATIONTYPE_SELECT': "select"

};

ELGLMSVALIDATE.VALIDATOR.validateControl = function (control, vaildationType) {
    var controlParent = control.closest('.form-group');
    //control.parent().parent();
    var isValid = true;
    var text = control.val();
    if (control.get(0).tagName.toUpperCase() == "DIV") {
        text = control.text();
    };

    switch (vaildationType) {
        case ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_EMAIL:
            isValid = ELGLMSVALIDATE.VALIDATOR.isValidEmailAddress(text);
            break;
        case ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_BLANK:
            isValid = !ELGLMSVALIDATE.VALIDATOR.isBlank(text);
            break;
        case ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_NUMBER:
            isValid = ELGLMSVALIDATE.VALIDATOR.isNumber(text);
            break;
        case ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_DATE:
            isValid = ELGLMSVALIDATE.VALIDATOR.isDate(text);
            break;
        case ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_SELECT:
            if (text == "-1" || text == -1 || text == "" || text == "0" || text == 0) {
                isValid = false;
            } else {
                isValid = true;
            }
            break;
        default:

            break;
    }
    if (isValid) {
        ELGLMSVALIDATE.VALIDATOR.removeError(controlParent);
    } else {
        ELGLMSVALIDATE.VALIDATOR.addError(controlParent);
    }
    return isValid;
};

ELGLMSVALIDATE.VALIDATOR.addError = function (control) {
    control.addClass("has-error");
};

ELGLMSVALIDATE.VALIDATOR.removeError = function (control) {
    control.removeClass("has-error");
};

ELGLMSVALIDATE.VALIDATOR.isValidEmailAddress = function (emailAddress) {
    var pattern = new RegExp(/^[+a-zA-Z0-9._'-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,10}$/i);
    return pattern.test(emailAddress);
};

ELGLMSVALIDATE.VALIDATOR.isBlank = function (text) {
    var t = $.trim(text);
    if (t == "") {
        return true;
    }
    return false;
};

ELGLMSVALIDATE.VALIDATOR.isNumber = function (n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
};

ELGLMSVALIDATE.VALIDATOR.isDate = function (n) {
    var date = Date.parse(n);
    if (isNaN(date)) {
        return false;
    };
    return true;
};


