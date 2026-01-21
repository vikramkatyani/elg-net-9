$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-docgroup-report');
    $('[data-toggle="tooltip"]').tooltip();
    docGroupReportHandler.init();
});

var docGroupReportHandler = (function () {


    var $ddlLoc = $('#ddlGroupLocation')

    var $searchGroup = $('#txtDoc');
    var $searchBtn = $('#searchDoc');
    var $clearSearchBtn = $('#clearSearchDoc');
    var $createBtn = $('#btnShowDocGroupPopUp');
    var $alert = $('#messageDocumentGroup');

    var $modal = $('#updateDocGroupModal');
    var $u_grpName = $('#txtUpdateDocGroupName');
    var $u_grpDesc = $('#txtUpdateDocGroupDesc');
    var $u_alert = $('#updateDocGroupMessage');
    var $updateInfo = $('#btnUpdateDocGroup');

    var update_grp = 0;

    var $reportContainer = $("#div-report-container");
    function showDefaultMessage() {
        $reportContainer.hide();
        $('#registeredAdminLearnerList').DataTable().destroy();
        var message = '<div ><b>How to use this page:</b><ul>' +
            '<li>Use the input field to search for a document group.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> Search to view matching records.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li>' +
            '<li>Click <i class="fa fa-plus-circle me-1"></i> Add Group to add new document group.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alert, 'default', message);
    }

    $createBtn.click(function () {
        addDocGroupHandler.showCreatePopUp();
    });

    $searchBtn.click(function (e) {
        e.preventDefault();
        $reportContainer.show();
        UTILS.Alert.hide($alert);
        $('#orgDocumentGroupList').DataTable().destroy();
        $('#orgDocumentGroupList').DataTable({
            processing: true,
            serverSide: true,
            filter: false,
            orderMulti: false,
            lengthChange: false,
            language: {
                //processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
                emptyTable: "No record(s) found."
            },
            ajax: {
                url: "LoadDocGroupData",
                type: "POST",
                datatype: "json",
                data: function (data) {
                    data.SearchText = $searchGroup.val();
                },
                error: function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            drawCallback: function (respData) {
                if (respData.json.listFor == 8) $('.btn-del-cat').remove();
                if (respData.json.listFor == 3 || respData.json.listFor == 8) $('.btn-map-cat').remove();
            },
            columns: [
                { data: "GroupName", name: "strGroupName", width: "40%" },
                { data: "GroupLocationName", name: "strLocation", width: "40%" },
                { data: "MappedDocuments", name: "DocumentCount", width: "10%" },
                { data: null, orderable: false, searchable: false, width: "10%" }
            ],
            columnDefs: [
                {
                    targets: 0,
                    render: function (a, b, data, d) {
                        let html = `<span data-bs-toggle="tooltip" title="${data.GroupDesc}">${data.GroupName}</span>`;
                        return html;
                    }
                },
                {
                    targets: 3,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        const id = data.GroupId;
                        return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            <li><a class="dropdown-item" href="#" id="edit-docGroup-${id}" onclick="docGroupReportHandler.showUpdateDocGroupPopUP(this)">
                                <i class="fa fa-edit me-2"></i>Edit</a></li>
                            <li><a class="dropdown-item btn-del-grp" href="#" id="delete-docGroup-${id}" onclick="docGroupReportHandler.deleteDocGroup(this)">
                                <i class="fa fa-trash me-2"></i>Delete</a></li>
                            <li><a class="dropdown-item" href="#" id="map-docGroup-${id}" onclick="docGroupReportHandler.showMapDocGroup(this)">
                                <i class="fa fa-plus-circle me-2"></i>Map Document</a></li>
                            <li><a class="dropdown-item btn-map-grp" href="#" id="map-user-docGroup-${id}" onclick="docGroupReportHandler.mapDocGroupToUser(this)">
                                <i class="fa fa-link me-2"></i>Assign to Users</a></li>
                        </ul>
                    </div>`;
                    }
                }
            ]
        });
    });

    function showMapDocGroup(btn) {
        const groupId = btn.id.split('-').pop();
        const groupName = $('#orgDocumentGroupList').DataTable().row(btn.closest('tr')).data()["GroupName"];
        window.location.href = hdnBaseUrl + 'Document/MapGroupDocuments?id=' + groupId + '&group=' + groupName;
    }

    function mapDocGroupToUser(btn) {
        const groupId = btn.id.split('-').pop();
        const groupName = $('#orgDocumentGroupList').DataTable().row(btn.closest('tr')).data()["GroupName"];
        window.location.href = hdnBaseUrl + 'Document/DocGroupUserAllocation?id=' + groupId + '&group=' + groupName;
    }

    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $searchGroup.val('');
        showDefaultMessage();
    });

    function showUpdateCategoryPopUP(btn) {
        UTILS.Alert.hide($alert);
        update_catId = btn.id.split('-').pop();
        const rowData = categoryTable.row(btn.closest('tr')).data();
        $catName.val(rowData.CategoryName);
        $catDesc.val(rowData.CategoryDesc);
        $modal.modal('show');
    }

    $updateInfo.click(function () {
        if (validate()) {
            const url = hdnBaseUrl + "Document/UpdateCategory";
            const data = {
                CategoryId: update_catId,
                CategoryName: $catName.val(),
                CategoryDesc: $catDesc.val()
            };
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', "Category updated successfully");
                    categoryTable.draw();
                } else {
                    UTILS.Alert.show($alert, 'error', "Failed to update category.");
                }
            }, function () {
                UTILS.Alert.show($alert, 'error', "Failed to update category. Please try again later.");
            });
        }
    });

    function validate() {
        if ($catName.val() && $catName.val().trim() !== '') {
            return true;
        } else {
            UTILS.Alert.show($alert, 'error', 'Please enter category name.');
            return false;
        }
    }


    function init() {
        showDefaultMessage();
        renderLocationDropDown();
    }
    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $ddlLoc.empty();

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                if (data.locationList.count > 1) {
                    $ddlLoc.append($('<option/>', { value: '0', text: 'Select All' }));
                }
                $.each(data.locationList, function (index, item) {
                    $ddlLoc.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }

    return {
        showUpdateCategoryPopUP,
        init,
        showMapDocGroup,
        mapDocGroupToUser
    };
})();


var addDocGroupHandler = (function () {
    var $modal = $('#addDocumentGroupModal');
    var $grpName = $('#addDocumentGroupModal #txtDocGroupName');
    var $grpDesc = $('#addDocumentGroupModal #txtDocGroupDesc');
    var $grpLoc = $('#addDocumentGroupModal #ddlGroupLocation');
    var $alert = $('#addDocumentGroupModal #addDocGroupMessage');
    var $createBtn = $('#addDocumentGroupModal #btnAddDocGroup');

    function showCreatePopUp() {
        $modal.modal('show')
    }

    $createBtn.click(function () {
        var valid = validate();
        if (valid == 1) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "Document/CreateDocGroup"
            var data = {
                GroupLocationId: $grpLoc.val(),
                GroupName: $grpName.val(),
                GroupDesc: $grpDesc.val(),
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Document group created successfully");
                    $('#orgDocumentGroupList').DataTable().draw();
                }
                else if (res.success == -1) {
                    UTILS.Alert.show($alert, "error", "Document group already exists.")
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add document group. Please try again later")
                }

                loading = false;
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                })
        }
        else if(valid == -1) {
            UTILS.Alert.show($alert, "error", "Group name can't be empty.")
            return;
        }
        else if (valid == -2) {
            UTILS.Alert.show($alert, "error", "Please select Location.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($grpName.val() == null || $grpName.val().trim() == '') {
            return -1
        }
        else if ($grpLoc.val() == null || $grpLoc.val() <= 0) {
            return -2
        }
        else {
            return 1;
        }
    }
    return{
        showCreatePopUp: showCreatePopUp
    }
})();

