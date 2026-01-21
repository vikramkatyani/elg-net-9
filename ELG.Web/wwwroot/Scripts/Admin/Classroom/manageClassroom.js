$(function () {
    UTILS.activateNavigationLink('classroomLink');
    UTILS.activateMenuNavigationLink('menu-class-report');
    classroomReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});


var addClassroomHandler = (function () {
    var $modal = $('#addClassroomModal');
    var $className = $('#txtClassroomName');
    var $classDesc = $('#txtClassroomDesc');
    var $alert = $('#addClassroomMessage');
    var $createBtn = $('#btnCreateClass');

    function showAddClassroomPopUP() {
        UTILS.Alert.hide($alert);
        $className.val('');
        $classDesc.val('');
        $modal.modal('show')
    }

    $createBtn.click(function (e) {
        e.preventDefault();
        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "Classroom/CreateClassroom"
            var data = {
                ClassroomName: $className.val(),
                ClassDesc: $classDesc.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Classroom created successfully");
                    $('#orgClassroomList').DataTable().draw();
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add classroom. Please try again later")
                }
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "Class name can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($className.val() != null && $className.val().trim() != '') {
            return true
        }
        else {
            return false;
        }
    }
    return {
        showAddClassroomPopUP: showAddClassroomPopUP
    }
})();

var classroomReportHandler = (function () {
    var $classroom = $('#txtClassroom')
    var $searchBtn = $('#searchClassroom')
    var $clearSearchBtn = $('#clearSearchClassroom')
    var $status = $('#classroomStatus')

    $('#btnAddClassroom').click(function () {
        addClassroomHandler.showAddClassroomPopUP();
    });

    var classroomTable = $('#orgClassroomList').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "ajax": {
            "url": "LoadClassroomData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $classroom.val();
                data.Status = $status.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "createdRow": function (row, data, dataIndex) {
            if (data["Active"] != true) {
                $(row).addClass('table-danger');
            }
        },
        "columns": [
            { "data": "ClassroomName", "name": "ClassroomName", "autoWidth": true },
            { "data": "ClassDesc", "name": "ClassDesc", "autoWidth": true },
            { "data": "CreatedOn", "name": "CreatedOn", "autoWidth": true }
        ],
        columnDefs: [ {
            // render action buttons in the last column
            targets: [3], render: function (a, b, data, d) {
                if (data["Active"] == true) {
                    return '<button type="button"id="edit-class-' + data["ClassroomId"] + '" class="btn btn-sm btn-dark mb-1" onclick="updateClassroomHandler.updateClass(this)"><i class="fa fa-fw fa-edit"></i><span>Edit</span></button> '
                        + '<button type="button" id="delete-class-' + data["ClassroomId"] + '" class="btn btn-sm btn-dark mb-1" onclick="classroomReportHandler.deleteClassroom(this)"><i class="fa fa-fw fa-trash"></i><span>Archive</span></button> '
                }
                else {
                    return '<button type="button"id="edit-class-' + data["ClassroomId"] + '" class="btn btn-sm btn-dark mb-1" onclick="updateClassroomHandler.updateClass(this)"><i class="fa fa-fw fa-edit"></i><span>Edit</span></button> '
                        + '<button type="button" id="delete-class-' + data["ClassroomId"] + '" class="btn btn-sm btn-dark mb-1 disabled"><i class="fa fa-fw fa-trash"></i><span>Archived</span></button> '
                }
            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        classroomTable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $classroom.val('');
        classroomTable.draw();
    });

    function init() {

    }

    //function to delete learner and refresh table
    function deleteClassroom(btn) {
        if (confirm("Are you sure you want to archive this Class?")) {
            var classId = btn.id.split('-').pop();
            var classTable = $('#orgClassroomList').DataTable();
            //selector
            $.ajax({
                type: 'post',
                url: 'ArchiveClassroom',
                dataType: 'json',
                data: { ClassroomId: classId },
                success: function (result) {
                    if (result.success == 1)
                        classTable.row($(btn).parents('tr')).remove().draw();
                    else
                        alert('Failed to delete the class.')
                },
                error: function (status) {
                    console.log(status);
                }
            });
        }
    }

    return {
        init: init,
        deleteClassroom: deleteClassroom
    }
})();

var updateClassroomHandler = (function () {
    $modal = $('#updateClassroomModal');
    $title = $('#updateClassroomTitle')
    $name = $('#txtEditClassroomName');
    $desc = $('#txtEditClassroomDesc');
    $alert = $('#editClassroomMessage');
    $updateInfo = $('#btnEditClass');
    var classId = 0;

    //function to update user info
    function updateClass(btn) {
        classId = btn.id.split('-').pop();
        $.ajax({
            type: 'get',
            url: hdnBaseUrl + "Classroom/GetClassInfo",
            dataType: 'json',
            data: { ClassroomId: classId },
            success: function (info) {
                populateData(info);
            },
            error: function (status) {
                console.log(status);
            }
        });
    }

    //to render info in the pop up
    function populateData(info) {
        $title.html('Update - ' + info.classroomInfo.ClassroomName);
        $name.val(info.classroomInfo.ClassroomName);
        $desc.val(info.classroomInfo.ClassDesc);
        $modal.modal('show');
        UTILS.Alert.hide($alert);
    }

    $updateInfo.click(function (e) {
        e.preventDefault();
        var cTable = $('#orgClassroomList').DataTable();
        var classroom = {
            ClassroomId: classId,
            ClassroomName: $name.val(),
            ClassDesc: $desc.val()
        }
        $.ajax({
            type: 'post',
            url: hdnBaseUrl + "Classroom/UpdateClassroom",
            dataType: 'json',
            data: classroom,
            success: function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($alert, "success", 'Class Info updated successfully.');
                    cTable.draw();
                }
                else {
                    UTILS.Alert.show($alert, "error", 'Failed to update class info.');
                }
            },
            error: function (status) {
                console.log(status);
                UTILS.Alert.show($alert, "error", 'Something went wrong. Please try agin later');
            }
        });
    });

    return {
        updateClass: updateClass
    }
})();