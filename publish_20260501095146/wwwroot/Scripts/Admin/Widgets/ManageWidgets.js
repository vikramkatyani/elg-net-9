$(function () {
    UTILS.activateNavigationLink('moduleLink');
    UTILS.activateMenuNavigationLink('menu-que-widget');
    $('[data-toggle="tooltip"]').tooltip();
    widgetCourseHandler.init();
});

var widgetCourseHandler = (function () {
    var $course = $('#txtCourse');
    var $searchBtn = $('#searchCourse')
    var $clearSearchBtn = $('#clearSearchCourse')
    var $createBtn = $('#btnAddCoursePopUp')
    var $alertCat = $('#divMessage_widgetcourse');

    var $modal = $('#updateCourseModal');
    var $courseName = $('#txtUpdateCourseName');
    var $courseDesc = $('#txtUpdateCourseDesc');
    var $alert = $('#updateCategoryMessage');
    var $updateInfo = $('#updateCourseMessage');

    var update_courseId = 0;

    function init() {
        $(document).on('click', '#btnAddCoursePopUp', function (e) {
            e.preventDefault();
        addWidgetCourseHandler.showCreatePopUp();
    });
    }

    var widgetCourseTable = $('#widgetCourseList').DataTable({
        processing: true,
        language: {
            processing:
                '<i class="fa fa-spinner fa-spin fa-2x fa-fw text-primary"></i><span class="sr-only">Loading...</span>',
            emptyTable: 'No record(s) found.'
        },
        serverSide: true,
        filter: false,
        orderMulti: false,
        lengthChange: false,
        ajax: {
            url: 'LoadWidgetCourseData',
            type: 'POST',
            datatype: 'json',
            data: function (data) {
                data.SearchText = $course.val();
            },
            error: function (xhr, error, code) {
                console.error(xhr);
                alert('Oops! Something went wrong. Please try again.');
            }
        },
        columns: [
            { data: 'CourseName', name: 'CourseName', autoWidth: true },
            { data: 'CourseWidgetCount', name: 'CourseWidgetCount', autoWidth: true },
            { data: null, name: 'Action', orderable: false, searchable: false } // placeholder for action
        ],
        columnDefs: [
            {
                targets: [0],
                render: function (data, type, row) {
                    return `<span class="course_name" style="cursor:pointer"
                     data-bs-toggle="tooltip"
                     title="${row.CourseDesc}">
                    ${row.CourseName}
                </span>`;
                }
            },
            {
                targets: [2],
                className: 'text-center',
                render: function (data, type, row) {
                    const courseGUID = row.CourseGUID;
                    return `
          <div class="dropdown">
            <button class="btn btn-sm rounded-circle"
                    type="button"
                    id="actionDropdown-${courseGUID}"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                    aria-label="Actions"
                    style="width:2.5rem;height:2.5rem;">
              <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${courseGUID}">
              <li>
                <a class="dropdown-item" href="#" id="show_widget_${courseGUID}"
                   onclick="widgetCourseHandler.showCourseWidgets(this)">
                   <i class="fa fa-eye me-2"></i>View Widgets
                </a>
              </li>
              <!-- Future Extension:
              <li>
                <a class="dropdown-item" href="#" id="global_js_${courseGUID}"
                   onclick="widgetCourseHandler.showCourseGlobalLink(this)">
                   <i class="fa fa-link me-2"></i>Global JS Reference
                </a>
              </li>
              -->
            </ul>
          </div>`;
                }
            }
        ],
        order: [[0, 'asc']]
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        widgetCourseTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $category.val('');
        widgetCourseTable.draw();
    });

    function showCourseWidgets(btn) {
        var courseId = btn.id.split('_').pop();
        var courseName = $(btn).closest('tr').find('.course_name').html();
        window.location.href = hdnBaseUrl + "QueWidget/CourseWidgets?id=" + courseId + "&c=" + courseName;
    }
    function showCourseGlobalLink(btn) {
        var courseId = btn.id.split('_').pop();
        var $globalLinkModal = $("#viewGlobalLinkModal");
        var $globalLinkBox = $("#txtGlobalLink");
        var tag = '<script src="' + hdnBaseUrl + 'Scripts/widget/widgetRender.js?c=' + courseId + '"></script>';
        $globalLinkBox.val(tag);
        $globalLinkModal.modal('show');
    }


    return {
        init: init,
        showCourseWidgets: showCourseWidgets,
        showCourseGlobalLink: showCourseGlobalLink
    }
})();

var addWidgetCourseHandler = (function () {
    var $modal = $('#addCourseModal');
    var $courseName = $('#addCourseModal #txtCourseName');
    var $courseDesc = $('#addCourseModal #txtCourseDesc');
    var $alert = $('#addCourseModal #addCourseMessage');
    var $createBtn = $('#addCourseModal #btnAddNewCourse');

    function showCreatePopUp() {
        $modal.modal('show')
    }

    $createBtn.click(function () {
        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "QueWidget/CreateCourse"
            var data = {
                CourseName: $courseName.val(),
                CourseDesc: $courseDesc.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Course created successfully");
                    $('#widgetCourseList').DataTable().draw();
                }
                else if (res.success == 0) {
                    UTILS.Alert.show($alert, "error", "Course already exists.")
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add course. Please try again later")
                }

                loading = false;
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "Course name can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($courseName.val() != null && $courseName.val().trim() != '') {
            return true
        }
        else {
            return false;
        }
    }
    return {
        showCreatePopUp: showCreatePopUp
    }
})();