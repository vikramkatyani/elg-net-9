$(function () {
    UTILS.activateNavigationLink('settingLink');
    UTILS.activateMenuNavigationLink('menu-manual-notification');
    learningProgressReportForNotificationHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var learningProgressReportForNotificationHandler = (function () {

    var $alertSel = $('#message_manual_notification');
    var $manualNotifacationContainer = $('#manual-notification-container')
    var $learner = $('#txtLearner');
    var $ddlLoc = $('#ddlLocation');
    var $ddlDep = $('#ddlDepartment');
    var $ddlCourse = $('#ddlCourse');
    var $searchBtn = $('#searchLearnerProgress');
    var $clearSearchBtn = $('#clearSearchProgressReport');
    var $allocateAllBtn = $('#btnSendNotificationToSelected');
    var $alert = $('#divMessage_ManualNotification');
    var recId = 0;
    var selRecords = [];
    var unSelectedRecords = [];

    //$.urlParam = function (name) {
    //    var trainee = '';
    //    var results = new RegExp('[\?&]' + name + '=([^&#]*)')
    //        .exec(window.location.href);
    //    if (results == null) {
    //        return 0;
    //    }
    //    $learner.val(results[1]);
    //    progressReport.draw();
    //}

    // function to initialise report page
    // bind drop down in search area
    function init() {
        //$.urlParam('trainee');
        //$txtRptFrom.val("");
        //$txtRptTo.val("");
        //$txtRptFrom.attr("disabled", "disabled");
        //$txtRptTo.attr("disabled", "disabled");

        var message = "Using the filters please select the criteria you need and click ‘Search’";
        UTILS.Alert.show($alertSel, 'info', message);
        $manualNotifacationContainer.hide();
        UTILS.Alert.hide($alert);
        selRecords = [];
        unSelectedRecords = [];
        renderCourseDropDown();
        renderLocationDropDown();
    }

    //function to render list of all locations in organisation
    function renderCourseDropDown() {
        $ddlCourse.empty();
        $ddlCourse.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllCourses(function (data) {
            if (data && data.courseList != null) {
                $.each(data.courseList, function (index, item) {
                    $ddlCourse.append($('<option/>', {
                        value: item.CourseId,
                        text: item.CourseName
                    }))
                });
            }
        })
    }

    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $ddlLoc.empty();
        $ddlLoc.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                $.each(data.locationList, function (index, item) {
                    $ddlLoc.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }

    //function to render list of all departments for location
    function renderDepartmentDropDown(locationId) {
        $ddlDep.empty();
        $ddlDep.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllDepartments(locationId, function (data) {
            if (data && data.departmentList != null) {
                $.each(data.departmentList, function (index, item) {
                    $ddlDep.append($('<option/>', {
                        value: item.DepartmentId,
                        text: item.DepartmentName
                    }))
                });
            }
        })
    }
    
    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alertSel);
        $manualNotifacationContainer.show();
        $('#learningProgressReportForNotification').DataTable({
            "processing": true,
            "language": {
                "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                "emptyTable": "No record(s) found."
            },
            "serverSide": true,
            "filter": false,
            "orderMulti": true,
            "stateSave": true,
            "ajax": {
                "url": hdnBaseUrl + "Report/LoadLearningProgressNotification",
                "type": "POST",
                "datatype": "json",
                "data": function (data) {
                    data.SearchText = $learner.val();
                    data.Location = $ddlLoc.val();
                    data.Department = $ddlDep.val();
                    data.Course = $ddlCourse.val();
                },
                "error": function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            "drawCallback": function (settings) {
                learningProgressReportForNotificationHandler.selectAllRows();
            },
            "columns": [
                { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
                { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
                { "data": "Location", "name": "l.strLocation", "autoWidth": true },
                { "data": "Department", "name": "d.strDepartment", "autoWidth": true },
                { "data": "CourseName", "name": "co.strCourse", "autoWidth": true },
                { "data": "AssignedOn", "name": "pd.dateAssignedOn", "autoWidth": true },
                { "data": "CourseStatus", "name": "pd.strStatus", "autoWidth": true },
                { "data": "LatestReminderOn", "name": "latestReminderOn", "autoWidth": true }
            ],
            columnDefs: [{
                // render checkbox in first column
                orderable: false,
                targets: [0], render: function (a, b, data, d) {
                    return '<input type="checkbox" onchange="learningProgressReportForNotificationHandler.selectRecords(this)" class="chk-rec" id="chk-rec-' + data["RecordId"] + '" name="chk-rec-' + data["RecordId"] + '" value="' + $('<div/>').text(data).html() + '">';
                }
            }, {
                // render learner name
                targets: [1], render: function (a, b, data, d) {
                    return '<span class="spn-usr-name">' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                targets: [2], render: function (a, b, data, d) {
                    return '<span class="spn-usr-email">' + data["EmailId"] + '</span>'
                }
            }, {
                targets: [3], render: function (a, b, data, d) {
                    return '<span>' + data["Location"] + '</span>'
                }
            }, {
                targets: [4], render: function (a, b, data, d) {
                    return '<span>' + data["Department"] + '</span>'
                }
            }, {
                targets: [5], render: function (a, b, data, d) {
                    return '<span class="spn-usr-course">' + data["CourseName"] + '</span>'
                }
            }, {
                targets: [6], render: function (a, b, data, d) {
                    return '<span>' + data["AssignedOn"] + '</span>'
                }
            }, {
                targets: [7], render: function (a, b, data, d) {
                    if (data["CourseStatus"] == 'incomplete')
                        return "<span>In-progress</span>";
                    else
                        return "<span>" + data["CourseStatus"] + "</span>";
                }
            }, {
                targets: [8], render: function (a, b, data, d) {
                    return "<span>" + data["LatestReminderOn"] + "</span>";
                }
            }, {
                targets: [9], render: function (a, b, data, d) {
                    var btn = '<button type="button"id="notification-record-' + data["RecordId"] + '" class="btn btn-sm btn-dark mb-1" data-loading-text="Please wait..."  onclick="learningProgressReportForNotificationHandler.sendManualNotification(this)"><i class="fa fa-fw fa-share"></i><span>Send Reminder</span></button> '
                    return btn;
                }
            }]
        });
        //progressReport.draw();
    });
    
    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        $ddlCourse.val('0');
        $('#learningProgressReportForNotification').DataTable().draw();
    });

    // populate department drop down on location change
    $ddlLoc.change(function () {
        var selectedLoc = $(this).val();
        renderDepartmentDropDown(selectedLoc);
    });


    // function to check learner checkbox click
   // $('#learningProgressReportForNotification tbody').on('change', 'input[type="checkbox"]', function () {
    function selectRecords(chk) {    
        // push values in selected or unselected arrays respectively to maintain state
        var id = chk.id.split('-').pop();

        //push in selected array
        if (chk.checked) {

            //allow only 10 selections
            if (selRecords.length > 9) {
                alert('You can select up to 10 records at a time for sending instant reminders');
                chk.checked = false;
                return false;
            }

            selRecords.push(id);
            // enable allocate to all button
            $allocateAllBtn.removeClass('disabled');
            $allocateAllBtn.attr("onclick", "learningProgressReportForNotificationHandler.sendNotificationToSelected()");
            $allocateAllBtn.attr("title", "Send reminders to Selected");

            // remove id if exist in unselected array
            if (unSelectedRecords.length > 0) {
                for (i = 0; i < unSelectedRecords.length; i++) {
                    if (unSelectedRecords[i] == id) {
                        unSelectedRecords.splice(i, 1);
                        break;
                    }
                }
            }
        }
        else {

            //push in unselected array
            unSelectedRecords.push(id);

            // remove id if exist in selected array
            if (selRecords.length > 0) {
                for (i = 0; i < selRecords.length; i++) {
                    if (selRecords[i] == id) {
                        selRecords.splice(i, 1);
                        break;
                    }
                }
            }
        }
    }
    //);

    //send notification
    var sendingMail = false;
    function sendManualNotification(btn) {
        if (!sendingMail) {
            sendingMail = true;
            var id = btn.id.split('-').pop();
            var $notificationBtn = $(btn);
            var row = $notificationBtn.closest('tr');
            var userName = row.find('.spn-usr-name').html();
            var email = row.find('.spn-usr-email').html();
            var courseName = row.find('.spn-usr-course').html();

            UTILS.disableButton($notificationBtn);
            if (confirm("Are you sure you want to send instant reminder for " + courseName + " to " + userName + "?")) {
                selRecords.push(id)
                var url = hdnBaseUrl + "Report/SendReminderNotificationToLearner";
                var data = {
                    selectedRecordList: selRecords,
                    //unselectedRecordList: unselectedRecordList
                }

                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alert, 'success', 'Instant Reminder sent successfully to ' + userName + '.')
                        UTILS.resetButton($notificationBtn);
                        selRecords = [];
                        unSelectedRecords = [];
                        $('#learningProgressReportForNotification').DataTable().draw();
                    }
                    else {
                        UTILS.Alert.show($alert, 'error', 'Failed to send Instant Reminder to ' + userName + '.')
                        UTILS.resetButton($notificationBtn);
                    }
                    sendingMail = false;
                }, function (err) {
                    console.log(err);
                        UTILS.Alert.show($updateRecordMessage, 'error', 'Failed to send Instant Reminder to ' + userName + '. Please try again later.');
                    UTILS.resetButton($notificationBtn);
                    sendingMail = false;
                })
            }
        }
    }

    function sendNotificationToSelected() {
        if (selRecords.length > 0) {
            if (confirm("Are you sure you want to send Instant Reminder for all selected records?")) {
                // allocate functionality
                UTILS.disableButton($allocateAllBtn);
                var url = hdnBaseUrl + "Report/SendReminderNotificationToLearner";
                var data = {
                    selectedRecordList: selRecords,
                    //unselectedRecordList: unselectedRecordList
                }
                UTILS.makeAjaxCall(url, data, function (result) {
                    if (result.success > 0) {
                        UTILS.Alert.show($alert, 'success', result.success +' Instant Reminder(s) sent successfully.')
                        resetSelections()
                        $('#learningProgressReportForNotification').DataTable().draw();
                    }
                    else
                        UTILS.Alert.show($alert, 'error', 'Failed to send Instant Reminder(s).')
                    UTILS.resetButton($allocateAllBtn);
                }, function (status) {
                    console.log(status);
                        UTILS.Alert.show($alert, 'error', 'Failed to send Instant Reminder(s). Please try again later');
                    UTILS.resetButton($allocateAllBtn);
                })

            }
            else {
                return false;
            }
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please select record(s) to send Instant Reminder');
            resetSelections()
        }

    }
    function resetSelections() {
        selRecords = [];
        unSelectedRecords = [];
        $allocateAllBtn.addClass('disabled');
        $allocateAllBtn.removeAttr("onclick");
        $allocateAllBtn.attr("title", "Select some records");
    }

    // function to render rows on page change
    function selectAllRows() {
        // Get all rows with search applied
        var rows = $('#learningProgressReportForNotification').DataTable().rows({ 'search': 'applied' }).nodes();
        
        //check each learner and render check box as per selected/unselected array
        $('input[type="checkbox"].chk-rec').each(function () {
            var $chkBox = $(this);
            var chkid = this.id.split('-').pop();
            if (selRecords.length > 0) {
                for (i = 0; i < selRecords.length; i++) {
                    if (selRecords[i] == chkid) {
                        $chkBox.prop('checked', true);
                        break;
                    }
                }
            }
            if (unSelectedRecords.length > 0) {
                for (j = 0; j < unSelectedRecords.length; j++) {
                    if (unSelectedRecords[j] == chkid) {
                        $chkBox.prop('checked', false);
                        break;
                    }
                }
            }

        })
    }

    //var progressReport = 

    return {
        init: init,
        sendManualNotification: sendManualNotification,
        sendNotificationToSelected: sendNotificationToSelected,
        selectAllRows: selectAllRows,
        selectRecords: selectRecords
    }
})();