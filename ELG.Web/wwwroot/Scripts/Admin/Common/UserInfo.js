//$('#menu-knowledge').click(function (e) {
//    e.preventDefault();
//    var url = $(this).data('href')
//    var left = ($(window).width() / 2) - (900 / 2),
//        top = ($(window).height() / 2) - (600 / 2);
//    window.open(url + "/categories/200851472-Administrator-Features", "ATF_Knowledge_Base", "width=900, height=600, top=" + top + ", left=" + left);

//});

var initialProfilePic = $('#imgLearnerProfile').attr('src');

var updateSessionLearnerHandler = (function () {

    $sessionInfomodal = $('#updateSessionLearnerInfoModal');
    $session_title = $('#updateSessionInfoTitle')
    //$userId = $('#txtUpdateUserId');
    $session_firstName = $('#txtUpdateSessionFirstName');
    $session_lastName = $('#txtUpdateSessionLastName');
    $session_email = $('#txtUpdateSessionEmail');
    $session_location = $('#updateSessionLocationDDL');
    $session_department = $('#updateSessionDepartmentDDL');
    $session_empno = $('#updateSessionEmpNo');
    $session_alert = $('#editSessionLearnerError');
    $session_updateInfo = $('#btnUpdateSessionLearnerInfo');

    $session_profileBtn = $('.user-profile-btn');

    $uploadProfilePicBtn = $("#btn-upload-pic");
    $removeProfilePicBtn = $("#btn-remove-pic");
    $alertProfilePic = $('#msg-profile-pic');
    $uploadProfileFile = $('#wizard-picture');
    var defaultPic = "../Content/img/no-pic.png";
    var isProfilPicChanged = 0;
    var removedProfilePic = 0;

    var session_learnerId = 0;
    var session_isSessionUser = false;

    $session_profileBtn.click(function () {

        UTILS.Alert.hide($session_alert);
        var session_learnerId = this.id.split('-').pop();
        getSessionUserInfo(session_learnerId);
    });

    //function to update user info
    function getSessionUserInfo(session_learnerId) {
        session_isSessionUser = true;
        var data = {
            UserID: session_learnerId
        }
        var url = hdnBaseUrl + 'UserManagement/GetSessionLearnerInfo';
        UTILS.makeAjaxCall(url, data, function (info) {
            session_populateData(info);
        }, function (status) {
            console.log(status);
        });
    }

    //to render info in the pop up
    function session_populateData(info) {
        UTILS.Alert.hide($session_alert);
        // render drop downs
        renderLocations(info.locationList);
        renderDepartments(info.departmentList);

        $session_title.html('Update - ' + info.learnerInfo.FirstName + ' ' + info.learnerInfo.LastName);
        //$userId.val(info.learnerInfo.EmployeeNumber);
        $session_firstName.val(info.learnerInfo.FirstName);
        $session_lastName.val(info.learnerInfo.LastName);
        $session_email.val(info.learnerInfo.EmailId);
        $session_location.val(info.learnerInfo.LocationID);
        $session_department.val(info.learnerInfo.DepartmentID);
        $session_empno.val(info.learnerInfo.EmployeeNumber);
        $sessionInfomodal.modal('show');

        UTILS.Alert.hide($alertProfilePic);
        $('#wizardPicturePreview').attr('src', initialProfilePic);
        isProfilPicChanged = 0;
        removedProfilePic = 0;

        if (initialProfilePic.includes('/Content/img/no-pic.png')) {
            $removeProfilePicBtn.hide()
        } else {
            $removeProfilePicBtn.show()
        }
    }

    function renderLocations(locationList) {
        $session_location.empty();
        $session_location.append($('<option/>', { value: '0', text: 'Select' }));
        $.each(locationList, function (index, item) {
            $session_location.append($('<option/>', {
                value: item.LocationId,
                text: item.LocationName
            }))
        });
    }

    function renderDepartments(departmentList) {
        $session_department.empty();
        $session_department.append($('<option/>', { value: '0', text: 'Select' }));
        $.each(departmentList, function (index, item) {
            $session_department.append($('<option/>', {
                value: item.DepartmentId,
                text: item.DepartmentName
            }))
        });
    }

    // populate department drop down on location change
    $session_location.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });

    //function to render list of all departments for location
    function renderDepartmentDropDown(locationId) {
        $session_department.empty();
        $session_department.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllDepartments(locationId, function (data) {
            if (data && data.departmentList != null) {
                $.each(data.departmentList, function (index, item) {
                    $session_department.append($('<option/>', {
                        value: item.DepartmentId,
                        text: item.DepartmentName
                    }))
                });
            }
        })
    }

    $session_updateInfo.click(function (e) {
        if (!session_isSessionUser) {
            return false;
        }
        e.preventDefault();
        UTILS.disableButton($session_updateInfo);


        //upload image if profile pic is  changed 
        if (isProfilPicChanged)
            uploadProfilePic();

        var url = hdnBaseUrl + 'UserManagement/UpdateSessionLearnerInfo';
        var learner = {
            UserID: session_learnerId,
            EmployeeNumber: $session_empno.val(),
            FirstName: $session_firstName.val(),
            LastName: $session_lastName.val(),
            EmailId: $session_email.val(),
            LocationID: $session_location.val(),
            DepartmentID: $session_department.val()
        }
        UTILS.makeAjaxCall(url, learner, function (result) {
            if (result.success == 1) {
                UTILS.Alert.show($session_alert, "success", 'User Info updated successfully !');
                UTILS.resetButton($session_updateInfo);
            }
            else if (result.success == 2) {
                UTILS.Alert.show($session_alert, "error", 'Email address is registered with another user. Please try another valid email address')
                UTILS.resetButton($session_updateInfo);
            }
            else {
                UTILS.Alert.show($session_alert, "error", 'Failed to update the user.');
                UTILS.resetButton($session_updateInfo);
            }
        }, function (status) {
            console.log(status);
            UTILS.Alert.show($session_alert, "error", 'Something went wrong. Please try agin later');
            UTILS.resetButton($session_updateInfo);
        });
    });

    $uploadProfileFile.change(function () {
        readURL(this);
    });

    function readURL(input) {
        if (input.files && input.files[0]) {

            //validate file size and format
            const fileSize = input.files[0].size / 1024 / 1024; // in MiB
            const fileName = input.files[0].name;
            const fileExtension = fileName.split('.')[fileName.split('.').length - 1].toLowerCase();

            //valid file extension = .jpg,jpeg,png,gif

            if (!(fileExtension === "jpg" ||
                fileExtension === "jpeg" ||
                fileExtension === "png" ||
                fileExtension === "gif") || fileSize > 1) {
                UTILS.Alert.show($alertProfilePic, "error", "Invalid image format/size.");
                input.value = ''; // clear file 
            }
            else {
                var reader = new FileReader();
                reader.onload = function (e) {
                    $('#wizardPicturePreview').attr('src', e.target.result).fadeIn('slow');
                }
                reader.readAsDataURL(input.files[0]);
                isProfilPicChanged = 1;
            }
        }
    }


    function uploadProfilePic() {
        if ($uploadProfileFile[0].files.length > 0) {
            var imageData = new FormData();
            imageData.append($uploadProfileFile[0].files[0].name, $uploadProfileFile[0].files[0]);

            $.ajax({
                url: hdnBaseUrl + "UserManagement/UploadProfilePic/",
                data: imageData,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data 
                success: function (result) {
                    if (result.uploaded > 0) {
                        //UTILS.Alert.show($alertProfilePic, "success", "Profile picture uploaded successfully.");
                        initialProfilePic = $('#wizardPicturePreview').attr('src');
                        $('#imgLearnerProfile').attr('src', initialProfilePic);
                    } else {
                        UTILS.Alert.show($alertProfilePic, "error", "Failed to upload Profile picture.");
                    }
                },
                error: function (err) {
                    console.log(err.statusText);
                    UTILS.Alert.show($alertProfilePic, "error", "Failed to upload profile picture.Please try again later.");
                }
            });

        }
        else if (removedProfilePic == 1) {

            $.ajax({
                url: hdnBaseUrl + "UserManagement/RemoveProfilePic/",
                type: "POST",
                success: function (result) {
                    if (result.uploaded > 0) {
                        //UTILS.Alert.show($alertProfilePic, "success", "Profile picture uploaded successfully.");
                        initialProfilePic = defaultPic;
                        $('#imgLearnerProfile').attr('src', initialProfilePic);
                    } else {
                        UTILS.Alert.show($alertProfilePic, "error", "Failed to remove Profile picture.");
                    }
                },
                error: function (err) {
                    console.log(err.statusText);
                    UTILS.Alert.show($alertProfilePic, "error", "Failed to remove profile picture.Please try again later.");
                }
            });
        }
    }


    function removeProfilePic() {
        $('#wizardPicturePreview').attr('src', defaultPic);
        removedProfilePic = 1;
        isProfilPicChanged = 1;
    }

    return {
        getSessionUserInfo: getSessionUserInfo,
        removeProfilePic: removeProfilePic
    }
})();