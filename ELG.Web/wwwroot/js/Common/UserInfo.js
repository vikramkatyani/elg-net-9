//$('#menu-knowledge').click(function (e) {
//    e.preventDefault();
//    var url = $(this).data('href')
//    var left = ($(window).width() / 2) - (900 / 2),
//        top = ($(window).height() / 2) - (600 / 2);
//    window.open(url + "/categories/200353451-General-Users-Trainees", "ATF_Knowledge_Base", "width=900, height=600, top=" + top + ", left=" + left);

//});
document.addEventListener('DOMContentLoaded', () => {
    UTILS.activateNavigationLink('settingsLink');
    UTILS.activateMenuNavigationLink('menu-user-profile');
    updateSessionLearnerHandler.getSessionUserInfo();
});

const _imgLearnerProfileEl = document.querySelector('#imgLearnerProfile');
let initialProfilePic = _imgLearnerProfileEl ? _imgLearnerProfileEl.src : '../Content/img/no-pic.png';

const unreadNotificationHandler = (() => {
    const announcementBtn = document.querySelector("#alertsDropdown");
    const container = document.querySelector('#divNotificationContainer');

    const ann_template = `<a class="dropdown-item d-flex align-items-center ann-link" href="${hdnBaseUrl}Announcement/ViewAnnouncement"><div class="mr-3"><div class="icon-circle bg-primary"><i class="fa fa-file-alt text-white"></i></div></div><div><div class="small text-gray-500">{{ann_pub_date}}</div><span class="font-weight-bold">{{ann_title}}</span></div></a>`;
    const no_ann = '<div class="alert aler-info">No unread announcement</div>';
    const loader = '<div class="text-center mb-4"><i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span></div>';

    if (announcementBtn) {
        announcementBtn.addEventListener('click', (event) => {
        container.innerHTML = '';
        container.innerHTML = loader;
        event.preventDefault();
        const url = `${hdnBaseUrl}announcement/LoadUnreadAnouncementData`;
        UTILS.makeAjaxCall(url, {}, (res) => {
            container.innerHTML = '';
            if (res.data.length > 0) {
                for (let i = 0; i < res.data.length; i++) {
                    let announcement = ann_template;
                    announcement = announcement.replace("{{ann_pub_date}}", res.data[i].PublishDate);
                    announcement = announcement.replace("{{ann_title}}", res.data[i].Title);
                    container.insertAdjacentHTML('beforeend', announcement);
                }
            } else {
                container.innerHTML = no_ann;
            }
        }, (err) => {
            console.log(err);
        });
        });
    }
})();


const updateSessionLearnerHandler = (() => {
    const sessionInfoModal = document.querySelector('#updateSessionLearnerInfoModal');
    const session_title = document.querySelector('#updateSessionInfoTitle');
    const session_firstName = document.querySelector('#txtUpdateSessionFirstName');
    const session_lastName = document.querySelector('#txtUpdateSessionLastName');
    const session_email = document.querySelector('#txtUpdateSessionEmail');
    const session_location = document.querySelector('#updateSessionLocationDDL');
    const session_department = document.querySelector('#updateSessionDepartmentDDL');
    const session_empno = document.querySelector('#updateSessionEmpNo');
    const session_alert = document.querySelector('#editSessionLearnerError');
    const session_updateInfo = document.querySelector('#btnUpdateSessionLearnerInfo');
    const session_profileBtn = document.querySelectorAll('.user-profile-btn');
    const uploadProfilePicBtn = document.querySelector("#btn-upload-pic");
    const removeProfilePicBtn = document.querySelector("#btn-remove-pic");
    const alertProfilePic = document.querySelector('#msg-profile-pic');
    const uploadProfileFile = document.querySelector('#wizard-picture');

    const defaultPic = "../Content/img/no-pic.png";
    let isProfilPicChanged = 0;
    let removedProfilePic = 0;
    let session_learnerId = 0;
    let session_isSessionUser = false;

    // Add click listeners to all profile buttons
    session_profileBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            UTILS.Alert.hide(session_alert);
            const learnerId = btn.id.split('-').pop();
            getSessionUserInfo(learnerId);
        });
    });

    /**
     * Get session user information
     * @param {number} session_learnerId - The learner ID
     */
    function getSessionUserInfo(session_learnerId) {
        session_isSessionUser = true;
        const data = { UserID: session_learnerId };
        const url = `${hdnBaseUrl}home/GetSessionLearnerInfo`;
        UTILS.makeAjaxCall(url, data, (info) => {
            session_populateData(info);
        }, (status) => {
            console.log(status);
        });
    }

    /**
     * Render information in the popup
     * @param {Object} info - User information object
     */
    function session_populateData(info) {
        UTILS.Alert.hide(session_alert);
        if (!info) return;
        renderLocations(info.locationList);
        renderDepartments(info.departmentList);
        if (session_title) session_title.innerHTML = `Update - ${info.learnerInfo.FirstName} ${info.learnerInfo.LastName}`;
        if (session_firstName) session_firstName.value = info.learnerInfo.FirstName || '';
        if (session_lastName) session_lastName.value = info.learnerInfo.LastName || '';
        if (session_email) session_email.value = info.learnerInfo.Email || '';
        if (session_location) session_location.value = info.learnerInfo.LocationID || '0';
        if (session_department) session_department.value = info.learnerInfo.DepartmentID || '0';
        if (session_empno) session_empno.value = info.learnerInfo.EmployeeNumber || '';

        if (sessionInfoModal) new bootstrap.Modal(sessionInfoModal).show();

        UTILS.Alert.hide(alertProfilePic);
        const wizardPreview = document.querySelector('#wizardPicturePreview');
        if (wizardPreview) wizardPreview.src = initialProfilePic;
        isProfilPicChanged = 0;
        removedProfilePic = 0;

        if (removeProfilePicBtn) {
            if (initialProfilePic.includes('/Content/img/no-pic.png')) {
                removeProfilePicBtn.style.display = 'none';
            } else {
                removeProfilePicBtn.style.display = 'block';
            }
        }
    }

    /**
     * Render locations in dropdown
     * @param {Array} locationList - List of locations
     */
    function renderLocations(locationList) {
        if (!session_location) return;
        session_location.innerHTML = '';
        const option = document.createElement('option');
        option.value = '0';
        option.textContent = 'Select';
        session_location.appendChild(option);

        const list = Array.isArray(locationList) ? locationList : [];
        list.forEach((item) => {
            const opt = document.createElement('option');
            opt.value = item.LocationId;
            opt.textContent = item.LocationName;
            session_location.appendChild(opt);
        });
    }

    /**
     * Render departments in dropdown
     * @param {Array} departmentList - List of departments
     */
    function renderDepartments(departmentList) {
        if (!session_department) return;
        session_department.innerHTML = '';
        const option = document.createElement('option');
        option.value = '0';
        option.textContent = 'Select';
        session_department.appendChild(option);

        const list = Array.isArray(departmentList) ? departmentList : [];
        list.forEach((item) => {
            const opt = document.createElement('option');
            opt.value = item.DepartmentId;
            opt.textContent = item.DepartmentName;
            session_department.appendChild(opt);
        });
    }

    // Populate department dropdown on location change
    if (session_location) {
        session_location.addEventListener('change', () => {
            const selectedLoc = session_location.value;
            renderDepartmentDropDown(selectedLoc);
        });
    }

    /**
     * Render departments for selected location
     * @param {number} locationId - The selected location ID
     */
    function renderDepartmentDropDown(locationId) {
        session_department.innerHTML = '';
        const option = document.createElement('option');
        option.value = '0';
        option.textContent = 'Select All';
        session_department.appendChild(option);

        UTILS.data.getAllDepartments(locationId, (data) => {
            if (data && data.departmentList !== null) {
                data.departmentList.forEach((item) => {
                    const opt = document.createElement('option');
                    opt.value = item.DepartmentId;
                    opt.textContent = item.DepartmentName;
                    session_department.appendChild(opt);
                });
            }
        });
    }

    // Handle update button click
    if (session_updateInfo) {
        session_updateInfo.addEventListener('click', (event) => {
            if (!session_isSessionUser) {
                return false;
            }
            event.preventDefault();
            UTILS.disableButton(session_updateInfo);

        // Upload image if profile pic is changed
        if (isProfilPicChanged) {
            uploadProfilePic();
        }

        const url = `${hdnBaseUrl}home/UpdateLearnerInfo`;
        const learner = {
            UserID: session_learnerId,
            EmployeeNumber: session_empno.value,
            FirstName: session_firstName.value,
            LastName: session_lastName.value,
            EmailId: session_email.value,
            LocationID: session_location.value,
            DepartmentID: session_department.value
        };

        UTILS.makeAjaxCall(url, learner, (result) => {
            if (result.success === 1) {
                UTILS.Alert.show(session_alert, "success", 'User Info updated successfully !');
                UTILS.resetButton(session_updateInfo);
            } else if (result.success === 2) {
                UTILS.Alert.show(session_alert, "error", 'Email address is registered with another user. Please try another valid email address');
                UTILS.resetButton(session_updateInfo);
            } else {
                UTILS.Alert.show(session_alert, "error", 'Failed to update the user.');
                UTILS.resetButton(session_updateInfo);
            }
        }, (status) => {
            console.log(status);
            UTILS.Alert.show(session_alert, "error", 'Something went wrong. Please try again later');
            UTILS.resetButton(session_updateInfo);
        });
        });
    }

    // Handle profile file change
    if (uploadProfileFile) {
        uploadProfileFile.addEventListener('change', function () {
            readURL(this);
        });
    }

    /**
     * Read and validate uploaded image file
     * @param {HTMLElement} input - The file input element
     */
    function readURL(input) {
        if (input.files && input.files[0]) {
            const fileSize = input.files[0].size / 1024 / 1024; // in MiB
            const fileName = input.files[0].name;
            const fileExtension = fileName.split('.')[fileName.split('.').length - 1].toLowerCase();

            // Valid file extensions: .jpg, .jpeg, .png, .gif
            const validExtensions = ["jpg", "jpeg", "png", "gif"];

            if (!validExtensions.includes(fileExtension) || fileSize > 1) {
                UTILS.Alert.show(alertProfilePic, "error", "Invalid image format/size.");
                input.value = ''; // Clear file
            } else {
                const reader = new FileReader();
                reader.onload = (event) => {
                    const preview = document.querySelector('#wizardPicturePreview');
                    preview.src = event.target.result;
                    preview.style.opacity = '0';
                    setTimeout(() => {
                        preview.style.opacity = '1';
                        preview.style.transition = 'opacity 0.5s ease-in';
                    }, 10);
                };
                reader.readAsDataURL(input.files[0]);
                isProfilPicChanged = 1;
            }
        }
    }

    /**
     * Upload profile picture to server
     */
    function uploadProfilePic() {
        if (uploadProfileFile.files.length > 0) {
            const imageData = new FormData();
            imageData.append(uploadProfileFile.files[0].name, uploadProfileFile.files[0]);

            fetch(`${hdnBaseUrl}home/UploadProfilePic/`, {
                method: "POST",
                body: imageData
            })
                .then(res => res.json())
                .then(result => {
                    if (result.uploaded > 0) {
                        initialProfilePic = document.querySelector('#wizardPicturePreview').src;
                        document.querySelector('#imgLearnerProfile').src = initialProfilePic;
                    } else {
                        UTILS.Alert.show(alertProfilePic, "error", "Failed to upload Profile picture.");
                    }
                })
                .catch(err => {
                    console.log(err);
                    UTILS.Alert.show(alertProfilePic, "error", "Failed to upload profile picture. Please try again later.");
                });
        } else if (removedProfilePic === 1) {
            fetch(`${hdnBaseUrl}home/RemoveProfilePic/`, {
                method: "POST"
            })
                .then(res => res.json())
                .then(result => {
                    if (result.uploaded > 0) {
                        initialProfilePic = defaultPic;
                        document.querySelector('#imgLearnerProfile').src = initialProfilePic;
                    } else {
                        UTILS.Alert.show(alertProfilePic, "error", "Failed to remove Profile picture.");
                    }
                })
                .catch(err => {
                    console.log(err);
                    UTILS.Alert.show(alertProfilePic, "error", "Failed to remove profile picture. Please try again later.");
                });
        }
    }

    /**
     * Remove profile picture
     */
    function removeProfilePic() {
        document.querySelector('#wizardPicturePreview').src = defaultPic;
        removedProfilePic = 1;
        isProfilPicChanged = 1;
    }

    return {
        getSessionUserInfo,
        removeProfilePic
    };
})();
