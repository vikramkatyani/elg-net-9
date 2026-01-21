document.addEventListener('DOMContentLoaded', function () {
    UTILS.activateNavigationLink('myCourseLink');
    UTILS.activateMenuNavigationLink('menu-my-courses');
    //myCoursesHandler.checkForAutoCoursesWithNoLicences();
    myCoursesHandler.getCourses();
    
    // Initialize Bootstrap 5 tooltips
    document.querySelectorAll('[data-toggle="tooltip"]').forEach(tooltip => {
        new bootstrap.Tooltip(tooltip);
    });
});

var myCoursesHandler = (function () {
    const container = document.querySelector("#divMyCourseHolder");
    const searchInput = document.querySelector('#txtSearchCoure');
    const sortSelect = document.querySelector('#ddlSort');
    const searchBtn = document.querySelector('#btnSeachCourse');
    const clearSearchBtn = document.querySelector('#btnClearSeachCourse');

    const infoModal = document.querySelector("#courseDescriptionModal");
    const infoModalTitle = document.querySelector("#courseDescriptionModal .modal-title");
    const infoModalResetDate = document.querySelector("#courseDescriptionModal #spnStatusResetDate");
    const courseDescContainer = document.querySelector("#course_desc_container");

    const confirmationModal = document.querySelector("#courseLaunchConfirmationModal");
    const overDueDate = document.querySelector("#courseLaunchConfirmationModal #spnOverDueDate");
    const confirmLaunchBtn = document.querySelector("#courseLaunchConfirmationModal #confirmLaunchBtn");
    let confirmedLaunchModule = 0;
    let confirmedLaunchURL = "";


    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', function () {
            if (searchInput) searchInput.value = '';
            if (typeof getCourses === 'function') getCourses();
        });
    }

    if (searchBtn) {
        searchBtn.addEventListener('click', function () {
            if (sortSelect) sortSelect.value = '0';
            if (typeof getCourses === 'function') getCourses();
        });
    }

    if (sortSelect) {
        sortSelect.addEventListener('change', function () {
            if (searchInput) searchInput.value = '';
            if (typeof getCourses === 'function') getCourses();
        });
    }

    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            const key = e.which || e.keyCode;
            if (key === 13) { // the enter key code
                e.preventDefault();
                if (sortSelect) sortSelect.value = '0';
                if (typeof getCourses === 'function') getCourses();
            }
        });
    }

    var template = "";

    template += '<div class="card">';
    template += '    <div class="card-content">';
    template += '        <h2>{{course.CourseName}}</h2>';
    template += '        <p class="txt-status"><span class="txt-{{course.ProgressStatus}}">{{course.ProgressStatus}}</span> | Last accessed: {{course.LastAccessedOn}}</p>';
    template += '        <p>{{course.CourseDesc}}</p>';

    template += '        <div class="sub-module-accordion d-none mt-4 p-6" id="accordion_{{course.CourseId}}">';
    template += '           <div id="collapse_{{course.CourseId}}">';
    template += '               {{course.subModuleList}}';
    template += '           </div>';
    template += '       </div>';

    template += '        <div class="card-button-container">';
    template += '            {{course.LaunchButton}}';
    template += '            {{course.CertificateButton}}';
    template += '            {{course.HistoryButton}}';
    template += '            {{course.ResetProgressButton}}';
    template += '        </div>';

    template += '    </div>';
    template += '    <div class="card-image">';
    template += '        <img src="{{course.CourseLogo}}" alt="{{course.CourseName}}" onerror="this.src=\'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22300%22 height=%22200%22%3E%3Crect fill=%22%23e0e0e0%22 width=%22300%22 height=%22200%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22Arial%22 font-size=%2214%22 fill=%22%23999%22%3ENo Thumbnail%3C/text%3E%3C/svg%3E\'">';
    template += '    </div>';
    template += '</div>';

    // Get all assigned courses
    function getCourses() {
        if (!container) return; // nothing to render into
        container.innerHTML = '';
        container.innerHTML = UTILS.LOADER;

        const url = hdnBaseUrl + "GetCourses";
        const searchTxt = searchInput ? (searchInput.value || '') : '';
        const data = {
            course: searchTxt,
            sort: sortSelect ? (sortSelect.value || '0') : '0'
        };
        UTILS.makeAjaxCall(url, data, function (data) {
            if (data !== null && data.courses !== null && data.courses !== undefined && data.courses.length > 0) {
                container.innerHTML = '';
                data.courses.forEach(renderTemplate);
            } else {
                container.innerHTML = '';
                container.innerHTML = UTILS.NORECORD;
            }
        }, function (err) {
            console.log(err);
        });
    }


    // // Check for auto courses with no licenses
    // function checkForAutoCoursesWithNoLicences() {
    //     const url = hdnBaseUrl + "home/CheckForAutoCoursesWithNoLicences";
    //     const data = {};
    //     UTILS.makeAjaxCall(url, data, function (data) {
    //         if (data !== null && data.courseWithNoLicences) {
    //             UTILS.Alert.show(document.querySelector("#divNoLicenceCourses"), "warning", "<b>Important notice</b> - Please note we have sent a note to your training administrator as there are not enough available licenses to issue you all of the courses that you require. ");
    //         } else {
    //             UTILS.Alert.hide(document.querySelector("#divNoLicenceCourses"));
    //         }
    //     }, function (err) {
    //         console.log(err);
    //     });
    // }

    function renderTemplate(item, index) {
        let currentCourse = template;
        // Defensive fallback for all variables
        const safe = (v) => (v === undefined || v === null ? '' : v);
        // Use PascalCase property names from API response
        currentCourse = currentCourse.replace(/{{course.CoursePath}}/g, safe(item.CoursePath));
        currentCourse = currentCourse.replace(/{{course.CourseLogo}}/g, safe(item.CourseLogo));
        currentCourse = currentCourse.replace(/{{course.ProgressStatus}}/g, safe(item.ProgressStatus ? item.ProgressStatus.toUpperCase() : ''));
        currentCourse = currentCourse.replace(/{{course.CourseId}}/g, safe(item.CourseId));
        currentCourse = currentCourse.replace(/{{course.CourseName}}/g, safe(item.CourseName));
        currentCourse = currentCourse.replace(/{{course.CourseDesc}}/g, safe(item.CourseDesc));
        currentCourse = currentCourse.replace(/{{course.LastAccessedOn}}/g, safe(item.LastAccessedOn));
        currentCourse = currentCourse.replace(/{{course.courseid}}/g, safe(item.CourseId));
        currentCourse = currentCourse.replace(/{{course.CourseResetDate}}/g, safe(item.CourseResetOn));

        if (item.SubModuleCount == 0) {
            if (item.IsExpired == 0) {
                if (item.ProgressStatus == 'passed' || item.ProgressStatus == 'PASSED')
                    currentCourse = currentCourse.replace('{{course.LaunchButton}}', '<button class="elg-btn elg-course-card-primary"  onclick = "myCoursesHandler.showLaunchConfirmationPopUp(' + item.CourseId + ', \'' + item.CoursePath + '\', \'' + item.CourseResetOn + '\')"><i class="fa fa-play"></i> Launch </button>');
                else
                    currentCourse = currentCourse.replace('{{course.LaunchButton}}', '<button class="elg-btn elg-course-card-primary"  onclick = "launchModulePopUp(' + item.CourseId + ', \'' + item.CoursePath + '\')"><i class="fa fa-play"></i> Launch </button>');
            }
            else
                currentCourse = currentCourse.replace('{{course.LaunchButton}}', '');
        } else {
            var subModulesHtml = "<div class='submodule-container'>";

            // Start table with headers
            subModulesHtml += "<table class='table table-bordered submodule-table'><thead><tr><th style='width: 40% !important;'>Module</th><th style='width: 30%;'>Status</th><th style='width: 30%;'>Action</th></tr></thead><tbody>";

            // Insert course-level launch button as the first row
            if (item.IsExpired == 0) {
                if (item.ProgressStatus.toLowerCase() === 'passed') {
                    subModulesHtml += "<tr><td>" + item.CourseName + "</td><td>" + (item.ProgressStatus ? item.ProgressStatus.toUpperCase() : "") + "</td><td><button class='elg-btn elg-course-card-primary' onclick=\"myCoursesHandler.showLaunchConfirmationPopUp(" + item.CourseId + ", '" + item.CoursePath + "', '" + item.CourseResetOn + "')\"><i class='fa fa-play'></i> Launch</button></td></tr>";
                } else {
                    subModulesHtml += "<tr><td>" + item.CourseName + "</td><td>" + (item.ProgressStatus ? item.ProgressStatus.toUpperCase() : "") + "</td><td><button class='elg-btn elg-course-card-primary' onclick=\"launchModulePopUp(" + item.CourseId + ", '" + item.CoursePath + "')\"><i class='fa fa-play'></i> Launch</button></td></tr>";
                }
            } else {
                subModulesHtml += "<tr><td>" + item.CourseName + "</td><td>" + (item.ProgressStatus ? item.ProgressStatus.toUpperCase() : "") +"</td><td><button class='elg-btn elg-course-card-secondary disabled'><i class='fa fa-play'></i> Launch</button></td></tr>";
            }

            // Render submodules
            if (Array.isArray(item.SubModuleList)) {
                item.SubModuleList.forEach(function (subModule) {
                subModulesHtml += "<tr>";
                subModulesHtml += "<td>" + subModule.SubModuleName;
                if (subModule.SubModuleDesc) {
                    subModulesHtml += "<br/><small>" + subModule.SubModuleDesc + "</small>";
                }
                subModulesHtml += "</td>";
                subModulesHtml += "<td>" + (subModule.SubModuleStatus ? (subModule.SubModuleStatus).toUpperCase() : ('Not accessed').toUpperCase()) + "</td>";
                if (subModule.RAID != null && subModule.RAID > 0) {
                    // when RAID exists (View)
                    subModulesHtml += "<td><button id='btn_" + subModule.SubModuleID + "' data-course='" + item.CourseId + "' class='elg-btn elg-course-card-secondary disabled' onclick='myCoursesHandler.showRAReport(this)'><i class='fa fa-eye'></i> View</button></td>";
                } else {
                    subModulesHtml += "<td><button id='btn_" + subModule.SubModuleID + "' class='elg-btn elg-course-card-secondary' data-path='" + subModule.SubModulePath + "' onclick='myCoursesHandler.launchSubModule(this)'><i class='fa fa-play'></i> Launch</button></td>";
                }
                subModulesHtml += "</tr>";
                });
            }

            subModulesHtml += "</tbody></table></div>";

            // Inject the updated HTML
            currentCourse = currentCourse.replace('{{course.subModuleList}}', subModulesHtml);
            currentCourse = currentCourse.replace('{{course.LaunchButton}}', `<button class="elg-btn elg-course-card-primary accordion-toggle-button" onclick="myCoursesHandler.openSubModuleAccordion(this)"><i class="fa fa-eye"></i> View Course </button>`);
        }

        if (item.ProgressStatus === 'passed' || item.ProgressStatus === 'PASSED') {
            currentCourse = currentCourse.replace('{{course.CertificateButton}}', '<button class="elg-btn elg-course-card-secondary"  data-toggle="tooltip" title="Print Certificate" id="cert-rec-' + item.ProgressRecordId + '" onclick = "myCoursesHandler.createCertificate(this)"> <i class="fa fa-print"></i> Certificate </button>');
            currentCourse = currentCourse.replace(/{{course_completion_text}}/g, 'Next due after');
            currentCourse = currentCourse.replace(/{{course.CourseCompleteBy}}/g, item.CourseResetOn);
        } else {
            currentCourse = currentCourse.replace('{{course.CertificateButton}}', '<button class="elg-btn elg-course-card-secondary disabled"  data-toggle="tooltip" title="Print Certificate" id="cert-rec-' + item.ProgressRecordId + '"> <i class="fa fa-print"></i> Certificate </button>');
            currentCourse = currentCourse.replace(/{{course_completion_text}}/g, 'Complete by');
            currentCourse = currentCourse.replace(/{{course.CourseCompleteBy}}/g, item.CourseCompleteBy);
        }

        if (item.CourseResetOn === null || item.CourseResetOn === '') {
            currentCourse = currentCourse.replace(/{{course_display_type}}/g, 'display:none');
        } else {
            currentCourse = currentCourse.replace(/{{course_display_type}}/g, 'display:block');
        }

        currentCourse = currentCourse.replace('{{course.HistoryButton}}', '<button class="elg-btn elg-course-card-secondary"  data-toggle="tooltip" title="Progress History"  id="history-rec-' + item.ProgressRecordId + '" onclick = "progressHistoryHandler.showCourseHistory(' + item.CourseId + ')" > <i class="fa fa-history"></i> History </button>');
        if (item.SelfCourseResetEnabled)
            currentCourse = currentCourse.replace('{{course.ResetProgressButton}}', '<button class="elg-btn elg-course-card-secondary" id="reset-progress-' + item.ProgressRecordId + '" onclick = "myCoursesHandler.resetProgress(this)" data-course="' + item.CourseId + '" > <i class="fa fa-refresh"></i> Reset </button>');
        else
            currentCourse = currentCourse.replace('{{course.ResetProgressButton}}', '');
        container.innerHTML += currentCourse;
    }

    // Function to create certificate
    function createCertificate(btn) {
        const recordId = btn.id.split("-").pop();
        const baseUrl = hdnBaseUrl.replace('/Home/', '/');
        const url = baseUrl + "Certificate/GetCertificate/" + recordId;
        window.open(url, '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes');
    }

    // Function to show course description in the pop up
    function showMoreInfo(infoBtn) {
        infoModalTitle.innerHTML = "";
        courseDescContainer.innerHTML = "";
        const descEl = infoBtn?.parentElement?.parentElement?.querySelector('.course_desc_text');
        const titleEl = infoBtn?.parentElement?.parentElement?.querySelector('.card-title');
        const resetDateEl = infoBtn?.parentElement?.querySelector('.hdnResetDate');
        const desc = descEl ? descEl.innerHTML : '';
        const title = titleEl ? titleEl.innerHTML : '';
        const resetDate = resetDateEl ? resetDateEl.value : '';
        infoModalTitle.innerHTML = title;
        courseDescContainer.innerHTML = desc;
        infoModalResetDate.innerHTML = resetDate;
        new bootstrap.Modal(infoModal).show();
    }

    function showLaunchConfirmationPopUp(courseid, url, od) {
        confirmedLaunchModule = courseid;
        confirmedLaunchURL = url;

        let msg = '';
        if (!od) {
            msg = '<p>Feel free to launch it again if you\'d like to revisit any topic.</p>' +
                '<h4>If you\'re returning for a <b>refresher</b>, we recommend resetting your course status to start fresh.</h4>' +
                '<p>You can do this by clicking the <b>Reset</b> button (if available).</p>' +
                '<p>If the Reset button is not visible, your <b>Admin has chosen to manage resets manually</b>.</p>'+
            '<p>In that case, please contact your <b>Line Manager</b> to request a reset.</p>';
        } else {
            msg = '<h4>You have already completed this course for the current training period.</h4><br />' +
                '<p>This course will be next available for completion after <b><span>' + od + '</span></b>.</p>' +
                '<p>If you are revisiting for refresher training, please wait until the due date when status of the course auto resets to "not started". <b>Please click "Close" to close this message</b>.</p>' +
                '<p>If you wish to revisit this course to take some references, please click "Launch Course" to launch the course in a new pop-up window.</p>';
        }

        document.querySelector('#div-confrm-msg').innerHTML = msg;

        new bootstrap.Modal(confirmationModal).show();
    }
    function resetProgress(btn) {
        const recordId = btn.id.split('-').pop();
        const courseId = btn.dataset.course;
        if (confirm('Are you sure you want to continue?\nThis action will assign course as fresh and move the current progress to history.')) {
            const url = hdnBaseUrl + "ResetProgress";
            const data = {
                "Course": courseId,
                "RecordId": recordId
            };
            UTILS.makeAjaxCall(url, data, function (resp) {
                if (resp !== null && resp.reset === 1) {
                    myCoursesHandler.getCourses();
                } else {
                    // Show error message
                }
            }, function (err) {
                console.log(err);
            });
        }
    }

    function showTrainerInfoPopUp() {
        new bootstrap.Modal(document.querySelector("#modalTrainerInfo")).show();
        UTILS.makeAjaxCall(hdnBaseUrl + "Common/GetCompanyTrainerDetails", {}, function (res) {
            document.querySelector('#modalTrainerInfo #spnCompanyName').innerHTML = res.trainer.Company;
            document.querySelector('#modalTrainerInfo #spnTrainer').innerHTML = res.trainer.Trainer;
            document.querySelector('#modalTrainerInfo #spnTrainerEmail').innerHTML = res.trainer.Email;
            document.querySelector('#modalTrainerInfo #spnTrainerPhone').innerHTML = res.trainer.Phone;
        });
    }

    // Launch module on confirmation
    confirmLaunchBtn.addEventListener('click', function (e) {
        e.preventDefault();
        launchModulePopUp(confirmedLaunchModule, confirmedLaunchURL);
    });

    function openSubModuleAccordion(btn) {
        const accordion = btn.parentElement.parentElement.querySelector('.sub-module-accordion');
        accordion.classList.toggle('d-none');
        if (accordion.classList.contains('d-none')) {
            btn.innerHTML = '<i class="fa fa-eye"></i> View Course';
        } else {
            btn.innerHTML = '<i class="fa fa-eye-slash"></i> Hide Course';
        }
    }

    function keepOpenSubModuleAccordion(btn) {
        const accordion = btn.parentElement.parentElement.querySelector('.sub-module-accordion');
        accordion.classList.remove('d-none');
        btn.innerHTML = '<i class="fa fa-eye-slash"></i> Hide Course';
    }
    function launchSubModule(btn) {
        const cardButtonContainer = btn.closest('.card-button-container');
        const showSubModulesBtn = cardButtonContainer ? cardButtonContainer.querySelector('.accordion-toggle-button') : null;
        const subModuleId = btn.id.split('_').pop();
        const path = btn.dataset.path;
        const url = hdnBaseUrl + "LaunchSubModule";
        const data = { subModuleId };

        UTILS.makeAjaxCall(url, data, function (response) {
            if (response && response.success === 1) {
                getCourses(); // Reloads the current page
            } else {
                console.warn("Submodule launch failed or returned unexpected response:", response);
            }
        }, function (err) {
            console.log(err);
        });
        // Ensure the parent accordion toggle remains open if present
        if (showSubModulesBtn) {
            try {
                keepOpenSubModuleAccordion(showSubModulesBtn);
            } catch (e) {
                console.warn('Could not keep submodule accordion open:', e);
            }
        }
        if (path) {
            window.open(path, '_blank');
        } else {
            console.warn("No path specified in data-path attribute.");
        }


    }

    function showRAReport(btn) {
        const subModuleId = btn.id.split('_').pop();
        const courseId = btn.getAttribute('data-course') || btn.dataset.course;
        const url = hdnBaseUrl + "RiskAssessment/LoadRAReport/"
            + encodeURIComponent(courseId) + "/" + encodeURIComponent(subModuleId);
        window.location.href = url;
    }

    return {
        getCourses: getCourses,
        createCertificate: createCertificate,
        showMoreInfo: showMoreInfo,
        showLaunchConfirmationPopUp: showLaunchConfirmationPopUp,
        //checkForAutoCoursesWithNoLicences: checkForAutoCoursesWithNoLicences,
        showTrainerInfoPopUp: showTrainerInfoPopUp,
        resetProgress: resetProgress,
        openSubModuleAccordion: openSubModuleAccordion,
        launchSubModule: launchSubModule,
        keepOpenSubModuleAccordion: keepOpenSubModuleAccordion,
        showRAReport: showRAReport
    }

})();
