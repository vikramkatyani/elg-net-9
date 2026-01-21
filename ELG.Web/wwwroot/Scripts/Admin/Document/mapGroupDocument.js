$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-docgroup-report');
    documentReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var selectedGroupId = 0;

var documentReportHandler = (function () {
    var $document = $('#txtDoc')
    var $category = $('#ddlCategory')
    var $subcategory = $('#ddlSubCategory')
    var $searchBtn = $('#searchDoc')
    var $clearSearchBtn = $('#clearSearchDoc')
    var $alertDoc = $('#messageDocument');

    var $modal = $('#updateDocumentModal');
    var $docName = $('#txtUpdateDocName');
    var $docDesc = $('#txtUpdateDocDesc');
    var $docCategory = $('#ddlCategory_updatedoc');
    var $docSubCategory = $('#ddlSubCategory_updatedoc');
    var $docStatus = $('#ddlDocStatus');
    var $docVersion = $('#txtUpdateDocVersion');
    var $docDtPublish = $('#txtUpdateDocDatePublished');
    var $docDtReview = $('#txtUpdateDocDtReview');
    var $alert = $('#updateDocumentMessage');
    var $updateInfo = $('#btnUpdateDoc');
    var update_docid = 0;
    var $reportContainer = $('#div-report-container')

    function showDefaultMessage() {
        $reportContainer.hide();
        $('#registeredAdminLearnerList').DataTable().destroy();
        var message = '<div > <b>How to use this page:</b> <ul>' +
            '<li>Use the input field to search for a document.</li>' +
            '<li>Select category from the dropdown to filter results.</li>' +
            '<li>Select sub category from the dropdown to filter results.</li>' +
            '<li>Select status from the dropdown to filter results.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> Search to view matching documents.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> Clear to reset all filters and start a new search.</li>' +
            '<li>Click <i class="fa fa-plus-circle me-1"></i> Add Document to create a new document.</li>' +
            '<li>Use the <i class="fa fa-ellipsis-v me-1"></i> menu in each row to assign or revoke document individually.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alertDoc, 'default', message);
    };

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        UTILS.Alert.hide($alertDoc);
        $reportContainer.show();
        $('#orgDocumentList').DataTable().destroy();
        $('#orgDocumentList').DataTable({
            processing: true,
            serverSide: true,
            filter: false,
            orderMulti: false,
            lengthChange: false,
            language: {
                processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span>',
                emptyTable: "No record(s) found."
            },
            ajax: {
                url: "LoadGroupDocumentData",
                type: "POST",
                datatype: "json",
                data: function (data) {
                    data.SearchText = $document.val();
                    data.Category = $category.val();
                    data.SubCategory = $subcategory.val();
                    data.IsActive = 1;
                    data.groupId = selectedGroupId;
                },
                error: function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            createdRow: function (row, data, dataIndex) {
                if (data.Mapped == 1) {
                    $(row).addClass("table-success");
                }
            },

            drawCallback: function (respData) {
                if (respData.json.listFor == 8) $('.btn-del-doc').remove();
            },
            columns: [
                { data: "DocumentName", name: "docName", width: "30%" },
                { data: "CategoryName", name: "catName", width: "15%" },
                { data: "SubCategoryName", name: "subCategory", width: "15%" },
                { data: "Version", name: "d.version", width: "10%" },
                { data: "DateOfPublish", name: "d.dateOfPublish", width: "10%" },
                { data: "DateOfReview", name: "d.dateOfReview", width: "10%" },
                { data: null, orderable: false, searchable: false, width: "10%" }
            ],
            columnDefs: [
                {
                    targets: 0,
                    render: function (a, b, data, d) {
                        return `<span data-bs-toggle="tooltip" title="${data.DocumentDesc}">${data.DocumentName}</span>`;
                    }
                },
                {
                    targets: 6,
                    className: "text-center align-middle",
                    render: function (a, b, data, d) {
                        const id = data.DocumentID;
                        const isMapped = data.Mapped == 1;
                        const assignBtn = isMapped
                            ? `<li><a class="dropdown-item" href="#" id="remove-doc-${id}" onclick="documentReportHandler.removeDocumentFromGroup(this)">
                            <i class="fa fa-times me-2"></i>Remove</a></li>`
                            : `<li><a class="dropdown-item" href="#" id="assign-doc-${id}" onclick="documentReportHandler.assignDocumentToGroup(this)">
                            <i class="fa fa-plus-circle me-2"></i>Assign</a></li>`;

                        return `
                <div class="dropdown">
                    <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                            id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                            style="width: 2.5rem; height: 2.5rem;">
                        <i class="fa fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                        ${assignBtn}
                        <li><a class="dropdown-item" href="${data.DocumentPath}" target="_blank" id="preview-doc-${id}">
                            <i class="fa fa-eye me-2"></i>Preview</a></li>
                    </ul>
                </div>`;
                    }
                }
            ]
        });
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $document.val('');
        $category.val('0');
        $subcategory.val('0');
        showDefaultMessage();
    });
    
    //function to initialize the view, bind all drop downs
    function init() {
        renderCategoryDropDown();
        showDefaultMessage();
        selectedGroupId = $.urlParam('id');
        var groupName = decodeURIComponent($.urlParam('group'));
        $("#spnGroupName").html(groupName);
        $searchBtn.click();
    }

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)')
            .exec(window.location.href);
        if (results == null) {
            return 0;
        }
        return (results[1]);
    }

    //function to render list of all locations in organisation
    function renderCategoryDropDown() {
        $category.empty();
        $category.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllDocumentCategory(function (data) {
            if (data && data.catList != null) {
                $.each(data.catList, function (index, item) {
                    $('.ddlCat').append($('<option/>', {
                        value: item.CategoryId,
                        text: item.CategoryName
                    }))
                });
            }
        })
    }

    $category.change(function (e) {
        e.preventDefault();
        renderSubCategoryDropDown_main()
    });


    //function to render sub category list for selected category
    function renderSubCategoryDropDown_main() {
        var selCategoryId = $category.val();
        $subcategory.empty();
        $subcategory.append($('<option/>', { value: '0', text: 'Select' }));

        if (selCategoryId > 0) {
            UTILS.data.getAllDocumentSubCategoryForCategory(selCategoryId, function (data) {
                if (data && data.subCategoryList != null) {
                    $.each(data.subCategoryList, function (index, item) {
                        $subcategory.append($('<option/>', {
                            value: item.SubCategoryId,
                            text: item.SubCategoryName
                        }))
                    });
                }
            });
        }
    }
    //function to render sub category list for selected category
    function renderSubCategoryDropDown_popup() {
        var selCategoryId = $docCategory.val();
        $docSubCategory.empty();
        $docSubCategory.append($('<option/>', { value: '0', text: 'Select' }));

        if (selCategoryId > 0) {
            UTILS.data.getAllDocumentSubCategoryForCategory(selCategoryId, function (data) {
                if (data && data.subCategoryList != null) {
                    $.each(data.subCategoryList, function (index, item) {
                        $docSubCategory.append($('<option/>', {
                            value: item.SubCategoryId,
                            text: item.SubCategoryName
                        }))
                    });
                }
            });
        }
    }


    $docCategory.change(function (e) {
        e.preventDefault();
        renderSubCategoryDropDown_popup()
    });

    function assignDocumentToGroup(element) {
        const docId = $(element).attr("id").split("assign-doc-")[1];
        if (!docId || !selectedGroupId) {
            alert("Missing document ID or group ID.");
            return;
        }

        $.ajax({
            type: "POST",
            url: hdnBaseUrl + "Document/AssignDocumentToGroup",
            data: {
                docIds: docId,
                groupId: selectedGroupId
            },
            success: function (response) {
                if (response && response.success === 1) {
                    UTILS.Alert.show($alertDoc, 'success',"Document assigned successfully.")
                    $('#orgDocumentList').DataTable().ajax.reload(null, false); 
                } else {
                    UTILS.Alert.show($alertDoc, 'error', "Unable to assign document.");
                }
            },
            error: function (xhr, error, code) {
                console.error(xhr);
                UTILS.Alert.show($alertDoc, 'error', "Something went wrong while assigning the document.");
            }
        });
    }
    function removeDocumentFromGroup(element) {
        const docId = $(element).attr("id").split("remove-doc-")[1];
        if (!docId || !selectedGroupId) {
            alert("Missing document ID or group ID.");
            return;
        }

        $.ajax({
            type: "POST",
            url: hdnBaseUrl + "Document/RemoveDocumentFromGroup",
            data: {
                docId: docId,
                groupId: selectedGroupId
            },
            success: function (response) {
                if (response && response.success === 1) {
                    UTILS.Alert.show($alertDoc, 'success', "Document removed successfully.");
                    $('#orgDocumentList').DataTable().ajax.reload(null, false);
                } else {
                    UTILS.Alert.show($alertDoc, 'error', "Unable to remove document.");
                }
            },
            error: function (xhr, error, code) {
                console.error(xhr);
                UTILS.Alert.show($alertDoc, 'error', "Something went wrong while removing the document.");
            }
        });
    }


    return {
        init: init,
        renderCategoryDropDown: renderCategoryDropDown,
        assignDocumentToGroup: assignDocumentToGroup,
        removeDocumentFromGroup: removeDocumentFromGroup
    }
})();

