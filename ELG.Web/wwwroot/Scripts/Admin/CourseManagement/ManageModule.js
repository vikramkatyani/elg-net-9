$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-manage-module');
    //configureModuleHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});


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
        var courseName = $(btn).closest('tr').find('.td-moduleName').html();
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
        var courseName = $(btn).closest('tr').find('.td-moduleName').html();
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

    var $courseSubModuleModal = $('#courseSubModuleModal');
    var $courseSubModuleTitle = $('#courseSubModuleTitle');  

    var mainCourseId = 0;

    var moduleTable = $('#configureModuleList').DataTable({
        lengthChange: false,
        "processing": true,
        "scrollX": true, 
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + 'CourseManagement/LoadModuleData',
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $course.val();
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
                var defaultSvg = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22300%22 height=%22200%22%3E%3Crect fill=%22%23e0e0e0%22 width=%22300%22 height=%22200%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22Arial%22 font-size=%2214%22 fill=%22%23999%22%3ENo Thumbnail%3C/text%3E%3C/svg%3E';
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
                    return '<button type="button" id="view-sm-' + data["ModuleID"] + '" class="btn btn-primary" onclick="configureModuleHandler.showCourseSubModules(this)"><i class="fa fa-fw fa-eye"></i><span>' + data["SubModuleCount"] +'</span></button> ';
                else
                    return '<span>0</span>';
            }
        }, {
            // render action menu
            targets: [3], render: function (a, b, data, d) {
                var id = data["ModuleID"];
                return (
                    '<div class="dropdown text-center">' +
                    '<button class="btn btn-sm rounded-circle" type="button" id="actionDropdown-' + id + '" data-bs-toggle="dropdown" aria-expanded="false" style="width:2.5rem;height:2.5rem;">' +
                    '<i class="fa fa-ellipsis-v"></i>' +
                    '</button>' +
                    '<ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-' + id + '">' +
                    '<li><a class="dropdown-item" href="#" id="edit-course-' + id + '" onclick="editCourseHandler.showEditCoursePopup(this)"><i class="fa fa-edit me-2"></i>Edit course</a></li>' +
                    '<li><a class="dropdown-item" href="#" id="add-submodule-' + id + '" onclick="addSubModuleHandler.showAddSubModulePopUP(this)"><i class="fa fa-plus me-2"></i>Add sub-module</a></li>' +
                    '<li><a class="dropdown-item" href="#" id="create-ra-' + id + '" onclick="createRAHandler.showCreateRAPopup(this)"><i class="fa fa-file-excel-o me-2"></i>Add risk assessment</a></li>' +
                    //'<li><a class="dropdown-item" href="#" id="map-ra-' + id + '" onclick="mapRAHandler.showMapRAPopup(this)"><i class="fa fa-link me-2"></i>Map existing risk assessment</a></li>' +
                    '</ul>' +
                    '</div>'
                );
            }
        }],
    });

    //apply filters for search
    $searchBtn.on('click', function (e) {
        e.preventDefault();
        moduleTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $course.val('');
        moduleTable.draw();
    });

    //function to open modal for update score
    function showCourseSubModules(btn) {
        mainCourseId = btn.id.split('-').pop();
        var moduleName = moduleTable.row(btn.closest('tr')).data()["ModuleName"];
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
    return {
        showCourseSubModules: showCourseSubModules,
        refreshDatatable: refreshDatatable
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
        var courseName = $(btn).closest('tr').find('.td-moduleName').html();
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

