$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-add-user');
    createLearnerHandler.init();
})

var loading = false;

var createLearnerHandler = (function () {
    var $createLearnerForm = $('#createLearnerForm');
    var $empNo = $("#txtAddEmpNo");
    var $learnerTitle = $('#txtAddTitle');
    var $fName = $('#txtAddFirstName');
    var $lName = $('#txtAddLastName');
    var $email = $('#txtAddEmail');
    var $location = $('#ddlAddLocation');
    var $department = $('#ddlAddDpartment');
    var $alert = $('#divAddLearnerErrorMessage');
    var $createBtn = $('#btnAddNewUser');

    //initalise view
    function init() {
        renderLocationDropDown();
    }

    // populate department drop down on location change
    $location.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $location.empty();
        $location.append($('<option/>', { value: '0', text: 'Select' }));

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                $.each(data.locationList, function (index, item) {
                    $location.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }

    //function to render list of all departments for location
    function renderDepartmentDropDown(locationId) {
        $department.empty();
        $department.append($('<option/>', { value: '0', text: 'Select' }));

        UTILS.data.getAllDepartments(locationId, function (data) {
            if (data && data.departmentList != null) {
                $.each(data.departmentList, function (index, item) {
                    $department.append($('<option/>', {
                        value: item.DepartmentId,
                        text: item.DepartmentName
                    }))
                });
            }
        })
    }

    //create button click
    $createBtn.click(function () {
        var $this = $(this);
        if (!loading) {
            if (isValidForm()) {

                UTILS.disableButton($this);
                loading = true;

                var learner = {
                    EmployeeNumber: $empNo.val(),
                    FirstName: $fName.val(),
                    LastName: $lName.val(),
                    EmailId: $email.val(),
                    LocationID: $location.val(),
                    DepartmentID: $department.val(),
                    Title: $learnerTitle.val()
                }

                UTILS.makeAjaxCall("CreateAccount", learner, function (res) {
                    if (res.success > 0) {
                        UTILS.Alert.show($alert, "success", "User created successfully. Account activation email has been sent to the user.")
                    }
                    else if (res.success == -99) {
                        UTILS.Alert.show($alert, "error", "Not allowed to add more users to the account.")
                    }
                    else if (res.success == 0) {
                        UTILS.Alert.show($alert, "error", "Email is already registered with another user.")
                    }
                    else {
                        UTILS.Alert.show($alert, "error", "Failed to add user. Please try again later.")
                    }

                    loading = false;
                    UTILS.resetButton($this);
                })
            } else {
                //UTILS.Alert.show($alert, "error", "Please enter valid information. All fields are mandatory.")

                loading = false;
                UTILS.resetButton($this);
            }
        }
    });

    function isValidForm() {
        var requiredControlsArray = [
            [$email, ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_EMAIL],
            [$fName, ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_BLANK],
            [$lName, ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_BLANK],
            [$location, ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_SELECT],
            [$department, ELGLMSVALIDATE.VALIDATOR.VALIDATIONTYPES.VALIDATIONTYPE_SELECT]];

        var isError = false;
        var validationStatusArray = [];
        var errorMessage = "<ul>";
        for (var i = 0; i < requiredControlsArray.length; i++) {
            var isValid = ELGLMSVALIDATE.VALIDATOR.validateControl(requiredControlsArray[i][0], requiredControlsArray[i][1]);
            validationStatusArray.push(isValid);
            if (!isValid) {
                isError = true;
                switch (i) {
                    case 0:
                        errorMessage = errorMessage + "<li>Please enter a valid " + $email.closest('.form-floating').find('label').text() + ".</li>";
                        break;
                    case 1:
                        errorMessage = errorMessage + "<li>Please enter " + $fName.closest('.form-floating').find('label').text() + ".</li>";
                        break;
                    case 2:
                        errorMessage = errorMessage + "<li>Please enter " + $lName.closest('.form-floating').find('label').text() + ".</li>";
                        break;
                    case 3:
                        errorMessage = errorMessage + "<li>Please select " + $location.closest('.form-floating').find('label').text() + ".</li>";
                        break;
                    case 4:
                        errorMessage = errorMessage + "<li>Please select " + $department.closest('.form-floating').find('label').text() + ".</li>";
                    default:
                        break;
                }
            }
        }
        if (isError) {
            errorMessage = errorMessage + "</ul>";
            UTILS.Alert.show($alert, "error", errorMessage);
            return false;
        }
        return true;
    }

    return {
        init: init
    }
})()