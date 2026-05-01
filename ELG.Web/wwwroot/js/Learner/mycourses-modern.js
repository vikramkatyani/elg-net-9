document.addEventListener('DOMContentLoaded', function () {
    UTILS.activateNavigationLink('myCourseLink');
    UTILS.activateMenuNavigationLink('menu-my-courses');
    myCoursesModernHandler.init();
});

var learnerHomeBaseUrl = (typeof window.learnerHomeBaseUrl !== 'undefined' && window.learnerHomeBaseUrl)
    ? window.learnerHomeBaseUrl
    : '/Learner/Home/';

var myCoursesModernHandler = (function () {
    var allCourses = [];
    var filteredCourses = [];
    var currentViewMode = 'grid';

    var $searchInput = $('#txtSearchCourseModern');
    var $searchBtn = $('#btnSearchCourseModern');
    var $clearBtn = $('#btnClearSearchCourseModern');
    var $gridBtn = $('#lmGridViewBtn');
    var $listBtn = $('#lmListViewBtn');

    var $gridContainer = $('#lmCourseGridContainer');
    var $listContainer = $('#lmCourseListContainer');

    var $statAssigned = $('#lmTotalAssigned');
    var $statCompleted = $('#lmTotalCompleted');
    var $statInProgress = $('#lmTotalInProgress');

    var $confirmLaunchBtn = $('#confirmLaunchBtn');
    var launchState = { courseId: 0, url: '' };

    function init() {
        bindEvents();
        applyViewMode();
        getCourses();
    }

    function bindEvents() {
        $searchBtn.on('click', function (e) {
            e.preventDefault();
            applySearch();
        });

        $clearBtn.on('click', function (e) {
            e.preventDefault();
            $searchInput.val('');
            applySearch();
        });

        $searchInput.on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                applySearch();
            }
        });

        $gridBtn.on('click', function () {
            setViewMode('grid');
        });

        $listBtn.on('click', function () {
            setViewMode('list');
        });

        $confirmLaunchBtn.on('click', function (e) {
            e.preventDefault();
            launchModulePopUp(launchState.courseId, launchState.url);
        });

        $(document).on('click', '.lm-desc-toggle', function (e) {
            e.preventDefault();
            toggleDescription($(this));
        });

        $(document).on('click', '.lm-launch-course', function () {
            var id = Number($(this).data('course-id')) || 0;
            var url = $(this).data('url') || '';
            var status = ($(this).data('status') || '').toString().toLowerCase();
            var resetOn = $(this).data('reset-on') || '';

            if (!url || id <= 0) {
                return;
            }

            if (status === 'passed') {
                showLaunchConfirmationPopUp(id, url, resetOn);
                return;
            }

            launchModulePopUp(id, url);
        });

        $(document).on('click', '.lm-toggle-submodules', function (e) {
            e.preventDefault();
            var courseId = Number($(this).data('course-id')) || 0;
            if (courseId > 0) {
                showSubmodulesModal(courseId);
            }
        });

        $(document).on('click', '.lm-action-certificate', function (e) {
            e.preventDefault();
            createCertificate($(this).data('record-id'));
        });

        $(document).on('click', '.lm-action-history', function (e) {
            e.preventDefault();
            var courseId = Number($(this).data('course-id')) || 0;
            if (courseId > 0) {
                progressHistoryHandler.showCourseHistory(courseId);
            }
        });

        $(document).on('click', '.lm-action-reset', function (e) {
            e.preventDefault();
            resetProgress($(this).data('record-id'), $(this).data('course-id'));
        });

        $(document).on('click', '.lm-submodule-launch', function (e) {
            e.preventDefault();
            var subModuleId = Number($(this).data('submodule-id')) || 0;
            var path = $(this).data('path') || '';
            var raid = Number($(this).data('raid')) || 0;
            var courseId = Number($(this).data('course-id')) || 0;

            if (raid > 0) {
                showRAReport(courseId, subModuleId);
                return;
            }

            launchSubModule(subModuleId, path);
        });
    }

    function showSubmodulesModal(courseId) {
        var course = allCourses.find(function (c) { return c.CourseId === courseId; });
        if (!course || !course.SubModuleList || !course.SubModuleList.length) {
            alert('No submodules found for this course.');
            return;
        }

        var modalTitle = document.getElementById('submodulesModalLabel');
        var modalBody = document.getElementById('submodulesModalBody');

        modalTitle.textContent = course.CourseName;

        var html = '<div class="lm-modal-submodules">';
        var courseStatusText = normalizeStatus(course.ProgressStatus).label.toUpperCase();
        var courseLaunchData = 'data-course-id="' + course.CourseId + '" ' +
            'data-url="' + escapeAttr(course.CoursePath || '') + '" ' +
            'data-status="' + escapeAttr(course.ProgressStatus || '') + '" ' +
            'data-reset-on="' + escapeAttr(course.CourseResetOn || '') + '"';

        html += '<div class="lm-modal-submodule-item is-main-package">' +
            '<div class="lm-modal-submodule-info">' +
            '<div class="lm-modal-submodule-name">' +
            escapeHtml(course.CourseName || 'Course Package') +
            '</div>' +
            '<div class="lm-modal-submodule-status">' + escapeHtml(courseStatusText) + '</div>' +
            '</div>' +
            '<div class="lm-modal-submodule-action">' +
            '<button class="btn lm-modal-launch-btn lm-launch-course" ' + courseLaunchData + '>' +
            '<i class="fa fa-play"></i> Launch' +
            '</button>' +
            '</div>' +
            '</div>';

        course.SubModuleList.forEach(function (sm) {
            var statusText = (sm.SubModuleStatus || 'Not accessed').toUpperCase();
            var actionLabel = (sm.RAID && sm.RAID > 0) ? 'View' : 'Launch';
            var icon = (sm.RAID && sm.RAID > 0) ? 'fa-eye' : 'fa-play';

            html += '<div class="lm-modal-submodule-item">' +
                '<div class="lm-modal-submodule-info">' +
                '<div class="lm-modal-submodule-name">' + escapeHtml(sm.SubModuleName || 'Submodule') + '</div>' +
                '<div class="lm-modal-submodule-status">' + escapeHtml(statusText) + '</div>' +
                '</div>' +
                '<div class="lm-modal-submodule-action">' +
                '<button class="btn lm-modal-launch-btn lm-submodule-launch" ' +
                'data-course-id="' + courseId + '" ' +
                'data-submodule-id="' + sm.SubModuleID + '" ' +
                'data-raid="' + (sm.RAID || 0) + '" ' +
                'data-path="' + escapeAttr(sm.SubModulePath || '') + '">' +
                '<i class="fa ' + icon + '"></i> ' + actionLabel +
                '</button>' +
                '</div>' +
                '</div>';
        });
        html += '</div>';

        modalBody.innerHTML = html;

        var modal = new bootstrap.Modal(document.getElementById('submodulesModal'), {
            keyboard: true,
            backdrop: 'static'
        });
        modal.show();
    }

    function setViewMode(mode) {
        currentViewMode = mode === 'list' ? 'list' : 'grid';
        applyViewMode();
    }

    function applyViewMode() {
        if (currentViewMode === 'list') {
            $listContainer.removeClass('d-none');
            $gridContainer.addClass('d-none');
            $listBtn.addClass('is-active');
            $gridBtn.removeClass('is-active');
        } else {
            $gridContainer.removeClass('d-none');
            $listContainer.addClass('d-none');
            $gridBtn.addClass('is-active');
            $listBtn.removeClass('is-active');
        }
    }

    function getCourses() {
        var url = learnerHomeBaseUrl + 'GetCourses';
        UTILS.makeAjaxCall(url, { course: '', sort: '0' }, function (res) {
            allCourses = (res && Array.isArray(res.courses)) ? res.courses : [];
            applySearch();
        }, function () {
            allCourses = [];
            filteredCourses = [];
            render();
            updateStats();
        });
    }

    function applySearch() {
        var q = ($searchInput.val() || '').toString().trim().toLowerCase();
        if (!q) {
            filteredCourses = allCourses.slice();
        } else {
            filteredCourses = allCourses.filter(function (x) {
                var name = (x.CourseName || '').toLowerCase();
                var desc = (x.CourseDesc || '').toLowerCase();
                return name.indexOf(q) >= 0 || desc.indexOf(q) >= 0;
            });
        }

        updateStats();
        render();
    }

    function updateStats() {
        var total = filteredCourses.length;
        var completed = filteredCourses.filter(function (x) {
            var s = (x.ProgressStatus || '').toString().toLowerCase();
            return s === 'passed' || s === 'completed';
        }).length;

        var inProgress = filteredCourses.filter(function (x) {
            var s = (x.ProgressStatus || '').toString().toLowerCase();
            return s && s !== 'passed' && s !== 'completed' && s !== 'not started' && s !== 'notstarted';
        }).length;

        $statAssigned.text(total);
        $statCompleted.text(completed);
        $statInProgress.text(inProgress);
    }

    function render() {
        renderGrid();
        renderList();
    }

    function renderGrid() {
        if (!filteredCourses.length) {
            $gridContainer.html('<div class="alert alert-light border">No courses found.</div>');
            return;
        }

        var html = filteredCourses.map(function (item) {
            return renderCard(item);
        }).join('');

        $gridContainer.html(html);
    }

    function renderList() {
        if (!filteredCourses.length) {
            $listContainer.html('<div class="alert alert-light border">No courses found.</div>');
            return;
        }

        var rows = filteredCourses.map(function (item) {
            return renderListTableRow(item);
        }).join('');

        var html =
            '<div class="table-responsive">' +
            '<table class="lm-list-table table table-hover align-middle">' +
            '<thead class="lm-list-thead"><tr>' +
            '<th style="width:90px;"></th>' +
            '<th>Course</th>' +
            '<th class="text-center" style="width:110px;">Sub-Modules</th>' +
            '<th style="width:130px;">Launch</th>' +
            '<th class="text-center" style="width:52px;"></th>' +
            '</tr></thead>' +
            '<tbody>' + rows + '</tbody>' +
            '</table></div>';
        $listContainer.html(html);
    }

    function renderListTableRow(item) {
        var defaultSvg = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22300%22 height=%22200%22%3E%3Crect fill=%22%23e0e0e0%22 width=%22300%22 height=%22200%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22Arial%22 font-size=%2214%22 fill=%22%23999%22%3ENo Thumbnail%3C/text%3E%3C/svg%3E';
        var logo = item.CourseLogo || defaultSvg;
        var status = normalizeStatus(item.ProgressStatus);
        var statusClass = getStatusClass(status.key);
        var lastAccess = item.LastAccessedOn || 'Never';
        var safeTitle = escapeHtml(item.CourseName || 'Untitled Course');

        var subModuleCell = item.SubModuleCount > 0
            ? '<span class="badge bg-primary rounded-pill">' + item.SubModuleCount + '</span>'
            : '<span class="badge bg-secondary rounded-pill">0</span>';

        var launchBtnHtml;
        if (item.SubModuleCount > 0) {
            launchBtnHtml = '<button class="lm-launch-btn lm-toggle-submodules" data-course-id="' + item.CourseId + '"><i class="fa fa-eye"></i> View</button>';
        } else {
            var launchData = 'data-course-id="' + item.CourseId + '" data-url="' + escapeAttr(item.CoursePath || '') + '" data-status="' + escapeAttr(item.ProgressStatus || '') + '" data-reset-on="' + escapeAttr(item.CourseResetOn || '') + '"';
            launchBtnHtml = '<button class="lm-launch-btn lm-launch-course" ' + launchData + '><i class="fa fa-play"></i> Launch</button>';
        }

        var actions = renderActions(item);
        var subModules = renderSubModules(item);

        return '<tr class="lm-list-table-row">' +
            '<td><div class="lm-list-thumb-wrap"><img src="' + logo + '" class="lm-list-thumb" onerror="this.src=\'' + defaultSvg + '\'"></div></td>' +
            '<td>' +
                '<div class="lm-course-title mb-1">' + safeTitle + '</div>' +
                '<div class="lm-course-meta">' +
                    '<span class="lm-status-chip ' + statusClass + '">' + status.label + '</span>' +
                    '<span class="ms-1">Last accessed: ' + escapeHtml(lastAccess) + '</span>' +
                '</div>' +
                subModules +
            '</td>' +
            '<td class="text-center">' + subModuleCell + '</td>' +
            '<td>' + launchBtnHtml + '</td>' +
            '<td class="text-center">' + actions + '</td>' +
            '</tr>';
    }

    function renderCard(item) {

        var defaultSvg = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22300%22 height=%22200%22%3E%3Crect fill=%22%23e0e0e0%22 width=%22300%22 height=%22200%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22Arial%22 font-size=%2214%22 fill=%22%23999%22%3ENo Thumbnail%3C/text%3E%3C/svg%3E';
        var logo = item.CourseLogo || defaultSvg;
        var status = normalizeStatus(item.ProgressStatus);
        var statusClass = getStatusClass(status.key);
        var lastAccess = item.LastAccessedOn || 'Never';
        var safeTitle = escapeHtml(item.CourseName || 'Untitled Course');
        var safeDesc = escapeHtml(item.CourseDesc || 'No description available.');

        var launchLabel = item.SubModuleCount > 0 ? 'View' : 'Launch';
        var launchClass = item.SubModuleCount > 0 ? 'lm-toggle-submodules' : 'lm-launch-course';
        var launchIcon = item.SubModuleCount > 0 ? 'fa-eye' : 'fa-play';
        var launchData = item.SubModuleCount > 0
            ? 'data-course-id="' + item.CourseId + '"'
            : 'data-course-id="' + item.CourseId + '" data-url="' + escapeAttr(item.CoursePath || '') + '" data-status="' + escapeAttr(item.ProgressStatus || '') + '" data-reset-on="' + escapeAttr(item.CourseResetOn || '') + '"';

        var actions = renderActions(item);
        var subModules = renderSubModules(item);

        return '<article class="lm-course-card">' +
            '<img src="' + logo + '" class="lm-course-banner" onerror="this.src=\'' + defaultSvg + '\'">' +
            '<div class="lm-course-body">' +
            '<h3 class="lm-course-title">' + safeTitle + '</h3>' +
            '<div class="lm-course-meta"><span class="lm-status-chip ' + statusClass + '">' + status.label + '</span><span>Last accessed: ' + escapeHtml(lastAccess) + '</span></div>' +
            '<p class="lm-course-desc is-clamped">' + safeDesc + '</p>' +
            '<button class="lm-desc-toggle" data-expanded="0">Show more</button>' +
            '<div class="lm-course-footer">' +
            '<button class="lm-launch-btn ' + launchClass + '" ' + launchData + '><i class="fa ' + launchIcon + '"></i> ' + launchLabel + '</button>' +
            actions +
            '</div>' + subModules +
            '</div>' +
            '</article>';
    }

    function renderSubModules(item) {
        if (!item.SubModuleCount || !Array.isArray(item.SubModuleList) || !item.SubModuleList.length) {
            return '';
        }

        var rows = item.SubModuleList.map(function (sm) {
            var statusText = (sm.SubModuleStatus || 'Not accessed').toUpperCase();
            var actionLabel = (sm.RAID && sm.RAID > 0) ? 'View' : 'Launch';
            var icon = (sm.RAID && sm.RAID > 0) ? 'fa-eye' : 'fa-play';

            return '<div class="lm-submodule-item">' +
                '<div>' +
                '<div class="lm-submodule-name">' + escapeHtml(sm.SubModuleName || 'Submodule') + '</div>' +
                '<div class="lm-submodule-status">' + escapeHtml(statusText) + '</div>' +
                '</div>' +
                '<button class="lm-submodule-launch" data-course-id="' + item.CourseId + '" data-submodule-id="' + sm.SubModuleID + '" data-raid="' + (sm.RAID || 0) + '" data-path="' + escapeAttr(sm.SubModulePath || '') + '"><i class="fa ' + icon + '"></i> ' + actionLabel + '</button>' +
                '</div>';
        }).join('');

        return '<div class="lm-submodules" data-course-id="' + item.CourseId + '">' + rows + '</div>';
    }

    function renderActions(item) {
        var canCertificate = isCompleted(item.ProgressStatus);
        var canReset = !!item.SelfCourseResetEnabled;

        var cert = canCertificate
            ? '<li><a href="#" class="dropdown-item lm-action-certificate" data-record-id="' + item.ProgressRecordId + '"><i class="fa fa-print"></i> Certificate</a></li>'
            : '<li><a href="#" class="dropdown-item disabled"><i class="fa fa-print"></i> Certificate</a></li>';

        var reset = canReset
            ? '<li><a href="#" class="dropdown-item lm-action-reset" data-record-id="' + item.ProgressRecordId + '" data-course-id="' + item.CourseId + '"><i class="fa fa-refresh"></i> Reset Progress</a></li>'
            : '';

        return '<div class="dropdown">' +
            '<button class="lm-action-trigger" type="button" data-bs-toggle="dropdown" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>' +
            '<ul class="dropdown-menu dropdown-menu-end">' +
            cert +
            '<li><a href="#" class="dropdown-item lm-action-history" data-course-id="' + item.CourseId + '"><i class="fa fa-history"></i> History</a></li>' +
            reset +
            '</ul>' +
            '</div>';
    }

    function toggleDescription($btn) {
        var expanded = $btn.attr('data-expanded') === '1';
        var $desc = $btn.siblings('.lm-course-desc');

        if (expanded) {
            $desc.addClass('is-clamped');
            $btn.attr('data-expanded', '0').text('Show more');
        } else {
            $desc.removeClass('is-clamped');
            $btn.attr('data-expanded', '1').text('Show less');
        }
    }

    function isCompleted(status) {
        var s = (status || '').toString().toLowerCase();
        return s === 'passed' || s === 'completed';
    }

    function normalizeStatus(status) {
        var s = (status || '').toString().toLowerCase();
        if (s === 'passed' || s === 'completed') {
            return { key: 'passed', label: 'Completed' };
        }

        if (!s || s === 'not started' || s === 'notstarted') {
            return { key: 'notstarted', label: 'Not Started' };
        }

        return { key: 'inprogress', label: 'In Progress' };
    }

    function getStatusClass(key) {
        if (key === 'passed') return 'lm-status-passed';
        if (key === 'inprogress') return 'lm-status-inprogress';
        return 'lm-status-notstarted';
    }

    function showLaunchConfirmationPopUp(courseId, url, resetOn) {
        launchState.courseId = courseId;
        launchState.url = url;

        var message = '<p>Course already completed. You can relaunch to review content at any time. To track new progress, click <strong>Reset</strong> (if available) or contact your <strong>Line Manager</strong>.</p>';

        $('#div-confrm-msg').html(message);
        new bootstrap.Modal(document.getElementById('courseLaunchConfirmationModal')).show();
    }

    function createCertificate(recordId) {
        var baseUrl = learnerHomeBaseUrl.replace('/Home/', '/');
        var url = baseUrl + 'Certificate/GetCertificate/' + recordId;
        window.open(url, '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes');
    }

    function resetProgress(recordId, courseId) {
        if (!confirm('Are you sure you want to reset progress for this course?')) {
            return;
        }

        UTILS.makeAjaxCall(learnerHomeBaseUrl + 'ResetProgress', { Course: courseId, RecordId: recordId }, function (resp) {
            if (resp && resp.reset === 1) {
                getCourses();
            }
        }, function () { });
    }

    function launchSubModule(subModuleId, path) {
        if (subModuleId <= 0) {
            return;
        }

        UTILS.makeAjaxCall(learnerHomeBaseUrl + 'LaunchSubModule', { subModuleId: subModuleId }, function () {
            getCourses();
            if (path) {
                window.open(path, '_blank');
            }
        }, function () {
            if (path) {
                window.open(path, '_blank');
            }
        });
    }

    function showRAReport(courseId, subModuleId) {
        var riskAssessmentBaseUrl = (typeof learnerRiskAssessmentBaseUrl !== 'undefined' && learnerRiskAssessmentBaseUrl)
            ? learnerRiskAssessmentBaseUrl
            : (learnerHomeBaseUrl || '/Learner/Home/').replace(/Home\/?$/i, '') + 'RiskAssessment/';
        var url = riskAssessmentBaseUrl + 'LoadRAReport/' + encodeURIComponent(courseId) + '/' + encodeURIComponent(subModuleId);
        window.location.href = url;
    }

    function showTrainerInfoPopUp() {
        new bootstrap.Modal(document.getElementById('modalTrainerInfo')).show();
        UTILS.makeAjaxCall(learnerHomeBaseUrl + 'Common/GetCompanyTrainerDetails', {}, function (res) {
            if (!res || !res.trainer) {
                return;
            }
            $('#modalTrainerInfo #spnCompanyName').text(res.trainer.Company || '');
            $('#modalTrainerInfo #spnTrainer').text(res.trainer.Trainer || '');
            $('#modalTrainerInfo #spnTrainerEmail').text(res.trainer.Email || '');
            $('#modalTrainerInfo #spnTrainerPhone').text(res.trainer.Phone || '');
        }, function () { });
    }

    function escapeHtml(value) {
        return (value || '')
            .toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function escapeAttr(value) {
        return escapeHtml(value).replace(/`/g, '&#096;');
    }

    return {
        init: init
    };
})();
