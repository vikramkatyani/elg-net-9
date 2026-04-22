$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-manage-module');
    //configureModuleHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

function getCourseNameFromSource(btn) {
    var $row = $(btn).closest('tr');
    if ($row.length > 0) {
        return $row.find('.td-moduleName').html();
    }
    var dataName = $(btn).attr('data-course-name');
    if (dataName) {
        return dataName;
    }
    return '';
}


var addSubModuleHandler = (function () {
    var $modal = $('#addSubModuleModal');
    var $title = $('#addSubModuleModal #addSubModuleTitle');
    var $docName = $('#txtDocName');
    var $docDesc = $('#txtDocDesc');
    var $alert = $('#addSubModuleMessage');
    var $createBtn = $('#btnAddNewSubModule');
    var $form = $('#formUploadSubModule');
    var xhr_request = null;

    var selectedCourseId = 0;

    function showAddSubModulePopUP(btn) {
        var courseName = getCourseNameFromSource(btn);
        $title.html(courseName);
        var courseId = btn.id.split('-').pop();
        selectedCourseId = courseId;
        $modal.modal('show');
    }

    $('#newDocFile').on('change', function () {
        //get the file name
        var fileName = document.getElementById("newDocFile").files[0].name; // $(this).val();
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    })

    $createBtn.on('click', function (e) {
        // Prevent the native form submit so we only use AJAX
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        if (validate()) {
            UTILS.disableButton($createBtn);
            //$form.submit();

            var docurl = hdnBaseUrl + "CourseManagement/CreateSubmodule"

            var fileUpload = $("#newDocFile").get(0);
            var files = fileUpload.files;
            // Create FormData object  
            var documentData = new FormData();

            // Add the file with the correct field name 'newDocFile'
            if (files.length > 0) {
                documentData.append('newDocFile', files[0]);
            }
            // Adding form data
            documentData.append('module.CourseId', selectedCourseId);
            documentData.append('module.SubModuleName', $docName.val());
            documentData.append('module.SubModuleDesc', $docDesc.val());

            $.ajax({
                url: docurl,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: documentData,
                success: function (result) {
                    debugger;
                    if (result.status > 0) {
                        UTILS.Alert.show($alert, "success", "Sub-module uploaded successfuly.");
                        configureModuleHandler.refreshDatatable();                        
                    }
                    else {
                        UTILS.Alert.show($alert, "error", "Failed to upload sub-module.")
                    }
                },
                error: function (err) {
                    console.log(err.statusText);
                    UTILS.Alert.show($alert, "error", "Failed to upload sub-module. Please try again later")
                }
            });
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {

        var fileUpload = $("#newDocFile").get(0);
        var files = fileUpload.files;
        if ($docName.val() == null || $docName.val().trim() == '') {
            UTILS.Alert.show($alert, "error", "Please enter the title.")
            return false
        }
        else if (!files.length > 0) {
            UTILS.Alert.show($alert, "error", "Please select a file to upload.")
            return false
        }
        else {
            return true;
        }
    }

    return {
        showAddSubModulePopUP: showAddSubModulePopUP
    }
})();

var editCourseHandler = (function () {
    var $modal = $('#editCourseModal');
    var $alert = $('#editCourseMessage');
    var $updateBtn = $('#btnUpdateCourse');
    var $form = $('#formEditCourse');
    var moduleTable = null;

    function showEditCoursePopup(btn) {
        var courseId = btn.id.split('-').pop();
        UTILS.Alert.hide($alert);
        
        // Get course data from DataTable
        moduleTable = $('#configureModuleList').DataTable();
        var rowData = moduleTable.row($(btn).closest('tr')).data();
        if (!rowData && typeof configureModuleHandler !== 'undefined' && configureModuleHandler.getCourseDataById) {
            rowData = configureModuleHandler.getCourseDataById(courseId);
        }
        if (!rowData) {
            UTILS.Alert.show($alert, "error", "Unable to load selected course details.");
            return;
        }
        
        // Populate form
        $('#editCourseId').val(rowData.ModuleID);
        $('#editCourseName').val(rowData.ModuleName);
        $('#editCourseDesc').val(rowData.ModuleDesc || '');
        $('#editOldThumbnailPath').val(rowData.CourseLogo || '');
        $('#editOldCoursePath').val(rowData.CoursePath || '');
        
        // Show current thumbnail if exists
        if (rowData.CourseLogo) {
            $('#currentThumbnail').show();
            $('#currentThumbnailImg').attr('src', rowData.CourseLogo);
        } else {
            $('#currentThumbnail').hide();
        }
        
        // Clear file inputs
        $('#editCourseThumbnail').val('');
        $('#editCoursePackage').val('');
        
        $modal.modal('show');
    }

    $updateBtn.click(function (e) {
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        
        if (validate()) {
            UTILS.disableButton($updateBtn);
            
            var url = hdnBaseUrl + "CourseManagement/UpdateCourse";
            var formData = new FormData();
            
            formData.append('CourseId', $('#editCourseId').val());
            formData.append('CourseName', $('#editCourseName').val());
            formData.append('CourseDesc', $('#editCourseDesc').val());
            formData.append('OldThumbnailPath', $('#editOldThumbnailPath').val());
            
            // Add thumbnail file if selected
            var thumbnailFile = $('#editCourseThumbnail')[0].files[0];
            if (thumbnailFile) {
                formData.append('ThumbnailFile', thumbnailFile);
            }
            
            // Add course package if selected
            var coursePackage = $('#editCoursePackage')[0].files[0];
            if (coursePackage) {
                formData.append('CoursePackage', coursePackage);
            }
            
            $.ajax({
                url: url,
                type: "POST",
                contentType: false,
                processData: false,
                data: formData,
                success: function (result) {
                    if (result.status > 0) {
                        UTILS.Alert.show($alert, "success", "Course updated successfully.");
                        setTimeout(function() {
                            configureModuleHandler.refreshDatatable();
                        }, 1500);
                    } else {
                        UTILS.Alert.show($alert, "error", result.message || "Failed to update course.");
                    }
                },
                error: function (err) {
                    console.log(err);
                    UTILS.Alert.show($alert, "error", "Failed to update course. Please try again later.");
                },
                complete: function() {
                    UTILS.resetButton($updateBtn);
                }
            });
        }
    });

    function validate() {
        var courseName = $('#editCourseName').val();
        
        if (!courseName || courseName.trim() === '') {
            UTILS.Alert.show($alert, "error", "Please enter the course title.");
            return false;
        }
        
        return true;
    }

    return {
        showEditCoursePopup: showEditCoursePopup
    }
})();

var mapRAHandler = (function () {
    var $modal = $('#mapRAPopup');
    var $title = $('#mapRAPopupTitle');
    var $alert = $('#mapRAPopupMessage');
    var selectedCourseId = 0;

    function showMapRAPopup(btn) {
        var courseName = getCourseNameFromSource(btn);
        UTILS.Alert.hide($alert);
        $title.html(courseName);
        selectedCourseId = btn.id.split('-').pop();
        $('#raListContainer_map').html('<div class="text-muted">Loading...</div>');
        $modal.modal('show');
        loadRiskAssessments('');
    }

    function renderRAList(res) {
        var container = $('#raListContainer_map');
        container.empty();
        if (res && res.data && res.data.length > 0) {
            res.data.forEach(function (r) {
                if (!r.Mapped) {
                    var item = '<div class="ra-item border-bottom py-2 clearfix">' +
                        '<span>' + r.ModuleName + '</span>' +
                        '<button class="btn btn-sm btn-primary float-end map-ra-btn" data-raid="' + r.ModuleID + '">Map</button>' +
                        '</div>';
                    container.append(item);
                }
            });
            if (container.children().length === 0) {
                container.html('<div class="text-muted">No unmapped risk assessments available.</div>');
            }
        } else {
            container.html('<div class="text-muted">No risk assessments found.</div>');
        }
    }

    function loadRiskAssessments(q) {
        if (!selectedCourseId) return;
        $.ajax({
            url: hdnBaseUrl + 'CourseManagement/SearchRiskAssessments',
            type: 'POST',
            data: { courseId: selectedCourseId, searchText: q },
            success: function (res) {
                // filter unmapped
                res.data = (res.data || []).filter(function (x) { return !x.Mapped; });
                renderRAList(res);
            },
            error: function () {
                $('#raListContainer_map').html('<div class="text-muted">Failed to load risk assessments.</div>');
            }
        });
    }

    function mapRAtoCourse(raid) {
        if (!selectedCourseId || !raid) return;
        $.ajax({
            url: hdnBaseUrl + 'CourseManagement/MapRiskAssessmentToCourse',
            type: 'POST',
            data: { courseId: selectedCourseId, raId: raid },
            success: function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Risk Assessment mapped successfully.");
                    configureModuleHandler.refreshDatatable();
                    $modal.modal('hide');
                } else {
                    UTILS.Alert.show($alert, "error", "Failed to map Risk Assessment.");
                }
            },
            error: function () {
                UTILS.Alert.show($alert, "error", "Failed to map Risk Assessment. Please try later.");
            }
        });
    }

    $('#btnSearchRA_map').on('click', function () {
        var q = $('#txtSearchRA_map').val();
        loadRiskAssessments(q);
    });

    $('#txtSearchRA_map').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            loadRiskAssessments($(this).val());
        }
    });

    $(document).on('click', '.map-ra-btn', function () {
        var raid = $(this).data('raid');
        mapRAtoCourse(raid);
    });

    return {
        showMapRAPopup: showMapRAPopup
    }
})();

var configureModuleHandler = (function () {

    var $course = $('#txtModule')
    var $searchBtn = $('#searchModuleBtn')
    var $clearSearchBtn = $('#clearSearchModuleBtn')
    var $listViewBtn = $('#courseViewListBtn')
    var $gridViewBtn = $('#courseViewGridBtn')
    var $gridContainer = $('#courseGridContainer')
    var $pillTotalCourses = $('#pillTotalCourses')
    var $pillNoAssignmentCourses = $('#pillNoAssignmentCourses')
    var $pillWithSubmoduleCourses = $('#pillWithSubmoduleCourses')
    var $courseStatsPills = $('.cm-stat-pill[data-course-filter]')
    var viewStorageKey = 'courseSetupViewMode';
    var moduleDataById = {};
    var selectedCourseStatsFilter = 0;
    var savedViewMode = localStorage.getItem(viewStorageKey);
    var currentViewMode = (savedViewMode === 'list' || savedViewMode === 'grid') ? savedViewMode : 'grid';

    var $courseSubModuleModal = $('#courseSubModuleModal');
    var $courseSubModuleTitle = $('#courseSubModuleTitle');  

    var mainCourseId = 0;

    function getDefaultSvg() {
        return 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22300%22 height=%22200%22%3E%3Crect fill=%22%23e0e0e0%22 width=%22300%22 height=%22200%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22Arial%22 font-size=%2214%22 fill=%22%23999%22%3ENo Thumbnail%3C/text%3E%3C/svg%3E';
    }

    function escapeHtml(text) {
        if (!text) return '';
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function updateCourseCache(rows) {
        moduleDataById = {};
        (rows || []).forEach(function (row) {
            moduleDataById[row.ModuleID] = row;
        });
    }

    function getActionMenuHtml(data, isGrid) {
        var id = data["ModuleID"];
        var dropdownId = (isGrid ? 'grid-actionDropdown-' : 'actionDropdown-') + id;
        var prefix = isGrid ? 'grid-' : '';
        var triggerClass = isGrid ? 'cm-row-actions-trigger cm-row-actions-trigger-card' : 'cm-row-actions-trigger rounded-circle';
        var safeCourseName = escapeHtml(data["ModuleName"] || '');
        return (
            '<div class="dropdown text-center cm-row-actions">' +
            '<button class="btn btn-sm ' + triggerClass + '" type="button" id="' + dropdownId + '" data-bs-toggle="dropdown" aria-expanded="false">' +
            '<i class="fa fa-ellipsis-h"></i>' +
            '</button>' +
            '<ul class="dropdown-menu dropdown-menu-end cm-row-actions-menu" aria-labelledby="' + dropdownId + '">' +
            '<li><a class="dropdown-item cm-row-actions-item" href="#" id="' + prefix + 'edit-course-' + id + '" data-course-name="' + safeCourseName + '" onclick="editCourseHandler.showEditCoursePopup(this)"><i class="fa fa-edit cm-row-actions-item-icon"></i><span>Edit course</span></a></li>' +
            '<li><a class="dropdown-item cm-row-actions-item" href="#" id="' + prefix + 'add-submodule-' + id + '" data-course-name="' + safeCourseName + '" onclick="addSubModuleHandler.showAddSubModulePopUP(this)"><i class="fa fa-plus cm-row-actions-item-icon"></i><span>Add sub-module</span></a></li>' +
            '<li><a class="dropdown-item cm-row-actions-item" href="#" id="' + prefix + 'create-ra-' + id + '" data-course-name="' + safeCourseName + '" onclick="createRAHandler.showCreateRAPopup(this)"><i class="fa fa-file-excel-o cm-row-actions-item-icon"></i><span>Add risk assessment</span></a></li>' +
            '</ul>' +
            '</div>'
        );
    }

    function renderGridCourses(rows) {
        if (!$gridContainer.length) {
            return;
        }
        var defaultSvg = getDefaultSvg();
        $gridContainer.empty();

        if (!rows || rows.length === 0) {
            $gridContainer.append('<div class="col-12"><div class="alert alert-light border">No record(s) found.</div></div>');
            return;
        }

        rows.forEach(function (row) {
            var safeCourseName = escapeHtml(row.ModuleName || '');
            var thumbUrl = row.CourseLogo ? row.CourseLogo : defaultSvg;
            var subModuleHtml = row.SubModuleCount > 0
                ? '<button type="button" id="grid-view-sm-' + row.ModuleID + '" data-course-name="' + safeCourseName + '" class="btn btn-primary btn-sm" onclick="configureModuleHandler.showCourseSubModules(this)">' + row.SubModuleCount + '</button>'
                : '<span class="badge bg-secondary">0</span>';

            var cardHtml =
                '<div class="col-12 col-sm-6 col-lg-4 col-xl-3">' +
                '<div class="cm-card">' +
                '<div class="cm-card-head">' +
                '<img src="' + thumbUrl + '" alt="Course Thumbnail" class="cm-card-thumb" onerror="this.src=\'' + defaultSvg + '\'">' +
                '</div>' +
                '<div class="cm-card-body">' +
                '<h3 class="cm-card-title">' + safeCourseName + '</h3>' +
                '<div class="cm-card-footer">' +
                '<div class="cm-card-submodule-group">' +
                '<span class="cm-card-submodule">Sub-Modules</span>' +
                subModuleHtml +
                '</div>' +
                '<div class="cm-card-actions">' +
                getActionMenuHtml(row, true) +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';

            $gridContainer.append(cardHtml);
        });
    }

    function updateViewToggleState() {
        if (currentViewMode === 'grid') {
            $gridViewBtn.addClass('is-active active btn-primary').removeClass('btn-outline-primary');
            $listViewBtn.removeClass('is-active active btn-primary').addClass('btn-outline-primary');
        } else {
            $listViewBtn.addClass('is-active active btn-primary').removeClass('btn-outline-primary');
            $gridViewBtn.removeClass('is-active active btn-primary').addClass('btn-outline-primary');
        }
    }

    function applyViewMode() {
        var $wrapper = $('#configureModuleList_wrapper');
        if (currentViewMode === 'grid') {
            $('#course-setup-container').addClass('d-none');
            $wrapper.addClass('d-none');
            $gridContainer.removeClass('d-none');
        } else {
            $('#course-setup-container').removeClass('d-none');
            $wrapper.removeClass('d-none');
            $gridContainer.addClass('d-none');
        }
        updateViewToggleState();
    }

    function setViewMode(mode) {
        currentViewMode = (mode === 'grid') ? 'grid' : 'list';
        localStorage.setItem(viewStorageKey, currentViewMode);
        applyViewMode();
    }

    function getCourseStatsFilter() {
        return selectedCourseStatsFilter;
    }

    function setActiveCourseStatsPill(filterValue) {
        if (!$courseStatsPills.length) {
            return;
        }

        $courseStatsPills.removeClass('is-selected');
        $courseStatsPills.filter('[data-course-filter="' + filterValue + '"]').addClass('is-selected');
    }

    function initCourseStatsPills() {
        if (!$courseStatsPills.length) {
            return;
        }

        setActiveCourseStatsPill(selectedCourseStatsFilter);

        $courseStatsPills.off('click keydown').on('click keydown', function (e) {
            if (e.type === 'keydown' && e.key !== 'Enter' && e.key !== ' ') {
                return;
            }

            if (e.type === 'keydown') {
                e.preventDefault();
            }

            selectedCourseStatsFilter = Number($(this).data('course-filter')) || 0;
            setActiveCourseStatsPill(selectedCourseStatsFilter);
            moduleTable.draw();
        });
    }

    function loadCourseQuickStats() {
        if (!$pillTotalCourses.length || !$pillNoAssignmentCourses.length || !$pillWithSubmoduleCourses.length) {
            return;
        }

        $pillTotalCourses.html('<i class="fa fa-spinner fa-spin"></i>');
        $pillNoAssignmentCourses.html('<i class="fa fa-spinner fa-spin"></i>');
        $pillWithSubmoduleCourses.html('<i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            type: 'get',
            url: hdnBaseUrl + 'CourseManagement/LoadCourseQuickStats',
            dataType: 'json',
            success: function (res) {
                if (res && res.success == 1) {
                    $pillTotalCourses.text(res.totalCourses);
                    $pillNoAssignmentCourses.text(res.coursesWithNoAssignments);
                    $pillWithSubmoduleCourses.text(res.coursesWithSubmodules);
                } else {
                    $pillTotalCourses.text('-');
                    $pillNoAssignmentCourses.text('-');
                    $pillWithSubmoduleCourses.text('-');
                }
            },
            error: function () {
                $pillTotalCourses.text('-');
                $pillNoAssignmentCourses.text('-');
                $pillWithSubmoduleCourses.text('-');
            }
        });
    }

    var moduleTable = $('#configureModuleList').DataTable({
        lengthChange: false,
        "processing": true,
        "scrollX": false,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "drawCallback": function () {
            var json = this.api().ajax.json();
            var rows = (json && json.data) ? json.data : [];
            updateCourseCache(rows);
            renderGridCourses(rows);
            applyViewMode();
        },
        "ajax": {
            "url": hdnBaseUrl + 'CourseManagement/LoadModuleData',
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $course.val();
                data.CourseStatsFilter = getCourseStatsFilter();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "CourseLogo", "name": "strCourseLogo", "autoWidth": true, "orderable": false },
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true },
            { "data": "SubModuleCount", "name": "SubModuleCount", "autoWidth": true, "orderable": false },
            { "data": "ModuleID", "name": "ModuleID", "autoWidth": true, "orderable": false },
            { "data": "ModuleDesc", "name": "strModuleDesc", "visible": false },
            { "data": "CoursePath", "name": "strCoursePath", "visible": false }
        ],
        columnDefs: [{
            // render course thumbnail
            targets: [0], orderable: false, render: function (a, b, data, d) {
                var thumbnailUrl = data["CourseLogo"];
                var defaultSvg = getDefaultSvg();
                var imgUrl = thumbnailUrl ? thumbnailUrl : defaultSvg;
                return '<img src="' + imgUrl + '" alt="Course Thumbnail" class="img-thumbnail" style="max-width: 100px; height: auto;" onerror="this.src=\'' + defaultSvg + '\'">';
            }
        }, {
            // render course name
            targets: [1], render: function (a, b, data, d) {
                return '<span class="td-moduleName">' + data["ModuleName"] + '</span>';
            }
        }, {
            // render submodule count
            targets: [2], render: function (a, b, data, d) {
                if (data["SubModuleCount"] > 0)
                    return '<button type="button" id="view-sm-' + data["ModuleID"] + '" data-course-name="' + escapeHtml(data["ModuleName"] || '') + '" class="btn btn-primary" onclick="configureModuleHandler.showCourseSubModules(this)"><span>' + data["SubModuleCount"] +'</span></button> ';
                else
                    return '<span>0</span>';
            }
        }, {
            // render action menu
            targets: [3], render: function (a, b, data, d) {
                return getActionMenuHtml(data, false);
            }
        }],
    });

    $listViewBtn.on('click', function () {
        setViewMode('list');
    });

    $gridViewBtn.on('click', function () {
        setViewMode('grid');
    });

    setViewMode(currentViewMode);

    //apply filters for search
    $searchBtn.on('click', function (e) {
        e.preventDefault();
        moduleTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $course.val('');
        selectedCourseStatsFilter = 0;
        setActiveCourseStatsPill(0);
        moduleTable.draw();
    });

    initCourseStatsPills();
    loadCourseQuickStats();

    //function to open modal for update score
    function showCourseSubModules(btn) {
        mainCourseId = btn.id.split('-').pop();
        var rowData = moduleTable.row($(btn).closest('tr')).data();
        if (!rowData) {
            rowData = moduleDataById[mainCourseId];
        }
        var moduleName = rowData ? rowData["ModuleName"] : getCourseNameFromSource(btn);
        $courseSubModuleTitle.html(moduleName);

        $('#courseSubModuleList').DataTable().destroy();
        $('#courseSubModuleList').DataTable({
            lengthChange: false,
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "paging": false,
            "filter": false,
            "orderMulti": true,
            "ajax": {
                "url": hdnBaseUrl + 'CourseManagement/LoadSubmoduleData',
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.courseId = mainCourseId;
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "columns": [
                { "data": "SubModuleName", "name": "c.strCourse", "autoWidth": true }
            ],
            columnDefs: [{
                // render action buttons in the last column
                targets: [0], render: function (a, b, data, d) {
                    return '<span data-toggle="tooltip" title="' + data["SubModuleDesc"] +'" class="td-moduleName">' + data["SubModuleName"] + '</span>'; 
                }
            }],
        });

        $courseSubModuleModal.modal('show');
    }

    function refreshDatatable() {
        $('#configureModuleList').DataTable().draw(); 
    }

    function getCourseDataById(courseId) {
        return moduleDataById[courseId] || null;
    }

    return {
        showCourseSubModules: showCourseSubModules,
        refreshDatatable: refreshDatatable,
        getCourseDataById: getCourseDataById
    }
})();

var createRAHandler = (function () {
    var $modal = $('#createRAPopup');
    var $title = $('#createRAPopupTitle');
    var $alert = $('#createRAPopupMessage');
    var $success = $('#createRAPopupSuccess');
    var $fileExcel = $('#fileCreateRAExcel');
    var $btnUpload = $('#btnUploadRA');
    var $btnPublish = $('#btnPublishRA');
    var $btnDiscard = $('#btnDiscardRA');
    var $txtTitle = $('#txtCreateRATitle');
    var $txtDesc = $('#txtCreateRADesc');
    var $uploadSection = $('#uploadSection');
    var $uploadFooter = $('#uploadFooter');
    var $previewSection = $('#previewSection');
    var $previewFooter = $('#previewFooter');
    var $previewContent = $('#previewContent');

    var selectedCourseId = 0;
    var draftKey = null;

    // Helper function to disable button with loading spinner
    function disableRAButton($btn) {
        $btn.attr('disabled', 'disabled').addClass('disabled');
        var originalHtml = $btn.html();
        $btn.data('original-html', originalHtml);
        $btn.html('<i class="fa fa-spinner fa-spin me-2"></i>Processing...');
    }

    // Helper function to reset button to original state
    function resetRAButton($btn) {
        var originalHtml = $btn.data('original-html') || $btn.html();
        $btn.removeAttr('disabled').removeClass('disabled');
        $btn.html(originalHtml);
    }

    function showCreateRAPopup(btn) {
        var courseName = getCourseNameFromSource(btn);
        selectedCourseId = btn.id.split('-').pop();
        resetModal();
        $title.text(courseName);
        $modal.modal('show');
    }

    function resetModal() {
        UTILS.Alert.hide($alert);
        UTILS.Alert.hide($success);
        $txtTitle.val('');
        $txtDesc.val('');
        $fileExcel.val('');
        draftKey = null;
        
        // Show upload section, hide preview
        $uploadSection.show();
        $uploadFooter.removeClass('collapse').show();
        $previewSection.addClass('collapse');
        $previewFooter.addClass('collapse');
        $previewContent.html('');
    }

    $fileExcel.on('change', function () {
        UTILS.Alert.hide($alert);
        var fileName = this.files && this.files[0] ? this.files[0].name : 'Choose File';
        $(this).next('.custom-file-label').html(fileName);
    });

    $btnUpload.on('click', function () {
        UTILS.Alert.hide($alert);
        var title = $txtTitle.val();
        if (!title || title.trim() === '') {
            UTILS.Alert.show($alert, 'error', 'Please enter a title.');
            return;
        }
        var f = $fileExcel[0].files && $fileExcel[0].files[0];
        if (!f) {
            UTILS.Alert.show($alert, 'error', 'Please select a file (.xlsx or .csv).');
            return;
        }
        if (f.size > 5 * 1024 * 1024) {
            UTILS.Alert.show($alert, 'error', 'Selected file exceeds 5 MB limit.');
            return;
        }
        disableRAButton($btnUpload);
        
        var fdDraft = new FormData();
        fdDraft.append('File', f);
        $.ajax({
            url: hdnBaseUrl + 'RiskAssessment/UploadDraft',
            type: 'POST',
            contentType: false,
            processData: false,
            data: fdDraft,
            success: function (res) {
                resetRAButton($btnUpload);
                if (res && res.success > 0 && res.draftKey && res.data) {
                    draftKey = res.draftKey;
                    renderPreview(res.data);
                    showPreviewMode();
                } else {
                    var errorMsg = (res && res.message) ? res.message : 'Failed to parse file.';
                    UTILS.Alert.show($alert, 'error', errorMsg);
                }
            },
            error: function () {
                resetRAButton($btnUpload);
                UTILS.Alert.show($alert, 'error', 'Failed to upload file. Please try again.');
            }
        });
    });

    function renderPreview(data) {
        var html = '<div>';
        if (!data.Sections || data.Sections.length === 0) {
            html += '<p class="text-muted">No questions found in the file.</p>';
        } else {
            data.Sections.forEach(function (section, sIdx) {
                html += '<div class="mb-5">';
                html += '<h5 class="fw-bold text-primary mb-3 pb-2 border-bottom"><i class="fa fa-folder me-2"></i>' + (section.Name || 'General') + '</h5>';
                
                if (section.Questions && section.Questions.length > 0) {
                    html += '<table class="table table-hover table-sm">';
                    html += '<thead class="table-light">';
                    html += '<tr>';
                    html += '<th style="width: 5%; text-align: center;">No.</th>';
                    html += '<th style="width: 35%;">Risk Factors</th>';
                    html += '<th style="width: 25%;">Things to consider</th>';
                    html += '<th style="width: 35%;">Response</th>';
                    html += '</tr>';
                    html += '</thead>';
                    html += '<tbody>';
                    
                    section.Questions.forEach(function (question, qIdx) {
                        html += '<tr>';
                        html += '<td style="text-align: center; vertical-align: middle;"><strong>' + (qIdx + 1) + '</strong></td>';
                        html += '<td style="vertical-align: middle;"><strong>' + escapeHtml(question.Question) + '</strong></td>';
                        html += '<td style="vertical-align: middle;"><small class="text-muted">' + (question.Instructions ? escapeHtml(question.Instructions) : '<em>No instructions</em>') + '</small></td>';
                        html += '<td>';
                        
                        if (question.Options && question.Options.length > 0) {
                            html += '<div class="form-check-list">';
                            question.Options.forEach(function (option, oIdx) {
                                var issueClass = option.Issue ? 'text-danger' : '';
                                var issueBadge = option.Issue ? '<span class="badge bg-danger ms-2">Issue</span>' : '';
                                html += '<div class="form-check ' + issueClass + '">';
                                html += '<input class="form-check-input" type="radio" name="option_' + sIdx + '_' + qIdx + '" id="option_' + sIdx + '_' + qIdx + '_' + oIdx + '" disabled>';
                                html += '<label class="form-check-label" for="option_' + sIdx + '_' + qIdx + '_' + oIdx + '">';
                                html += escapeHtml(option.Text) + issueBadge;
                                html += '</label>';
                                html += '</div>';
                            });
                            html += '</div>';
                        } else {
                            html += '<small class="text-muted"><em>No options</em></small>';
                        }
                        
                        html += '</td>';
                        html += '</tr>';
                    });
                    
                    html += '</tbody>';
                    html += '</table>';
                } else {
                    html += '<p class="text-muted"><em>No questions in this section.</em></p>';
                }
                
                html += '</div>';
            });
        }
        html += '</div>';
        $previewContent.html(html);
    }

    // Helper function to escape HTML special characters
    function escapeHtml(text) {
        if (!text) return '';
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }

    function showPreviewMode() {
        $uploadSection.hide();
        $uploadFooter.addClass('collapse').hide();
        $previewSection.removeClass('collapse').show();
        $previewFooter.removeClass('collapse').show();
    }

    $btnDiscard.on('click', function () {
        if (!draftKey) return;
        
        disableRAButton($btnDiscard);
        $.ajax({
            url: hdnBaseUrl + 'RiskAssessment/DiscardDraft',
            type: 'POST',
            data: { draftKey: draftKey },
            success: function (res) {
                resetRAButton($btnDiscard);
                if (res && res.success > 0) {
                    UTILS.Alert.show($alert, 'info', 'Draft discarded. You can upload another file.');
                    setTimeout(function () { resetModal(); }, 1500);
                } else {
                    UTILS.Alert.show($alert, 'error', 'Failed to discard draft.');
                }
            },
            error: function () {
                resetRAButton($btnDiscard);
                UTILS.Alert.show($alert, 'error', 'Failed to discard draft.');
            }
        });
    });

    $btnPublish.on('click', function () {
        if (!draftKey) {
            UTILS.Alert.show($alert, 'error', 'No draft to publish.');
            return;
        }
        
        var title = $txtTitle.val();
        if (!title || title.trim() === '') {
            UTILS.Alert.show($alert, 'error', 'Title is required.');
            return;
        }
        
        disableRAButton($btnPublish);
        var fd = new FormData();
        fd.append('draftKey', draftKey);
        fd.append('parentCourseId', selectedCourseId);
        fd.append('Title', title);
        fd.append('Description', $txtDesc.val() || '');
        
        $.ajax({
            url: hdnBaseUrl + 'RiskAssessment/PublishDraft',
            type: 'POST',
            contentType: false,
            processData: false,
            data: fd,
            success: function (res) {
                resetRAButton($btnPublish);
                if (res && res.success > 0) {
                    UTILS.Alert.show($success, 'success', 'Risk Assessment published successfully!');
                    configureModuleHandler.refreshDatatable();
                    setTimeout(function () { $modal.modal('hide'); }, 1500);
                } else {
                    var errorMsg = (res && res.message) ? res.message : 'Failed to publish risk assessment.';
                    UTILS.Alert.show($alert, 'error', errorMsg);
                }
            },
            error: function () {
                resetRAButton($btnPublish);
                UTILS.Alert.show($alert, 'error', 'Failed to publish risk assessment.');
            }
        });
    });

    return { showCreateRAPopup: showCreateRAPopup };
})();

