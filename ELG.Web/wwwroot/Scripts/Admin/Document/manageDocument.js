$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-document-report');
    documentReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var addDocHandler = (function () {
    var $modal = $('#addDocumentModal');
    var $catId = $('#ddlCategory_adddoc');
    var $subCatddl = $('#ddlSubCategory_adddoc');
    var $docName = $('#txtDocName');
    var $docDesc = $('#txtDocDesc');
    var $docVersion = $('#txtDocVersion');
    var $docDtPublish = $('#txtDocDatePublished');
    var $docDtReview = $('#txtDocDtReview');
    var $alert = $('#addDocumentMessage');
    var $createBtn = $('#btnAddNewDoc');
    var $form = $('#formUploadDocument');
    var xhr_request = null;


    $docDtPublish.datepicker({ dateFormat: 'yy-mm-dd' });
    $docDtReview.datepicker({ dateFormat: 'yy-mm-dd' });

    function showAddDocPopUP() {
        var docCounts = $('#orgDocumentList').DataTable().data().count();
        var enabledDocUpload = $('#hdnDocUpload').val();
        var allowedDocUpload = $('#hdnDocUploadCount').val();

        if (allowedDocUpload == undefined || allowedDocUpload == null)
            allowedDocUpload = 2;

        //if (docCounts >= allowedDocUpload && (enabledDocUpload == "0")) {
        if ((docCounts >= allowedDocUpload && (enabledDocUpload == "1")) || (docCounts >= 2 && (enabledDocUpload == "0"))) {
            var msg = '<div class="alert alert-warning">You can upload a maximum of <b>' + allowedDocUpload + '</b> documents. To access additional uploads, please reach out to <b>ELEARNINGATE</b></div>'
            $('#addDocumentModal .modal-body').html(msg);
            $createBtn.hide();
        }

        $modal.modal('show');
        renderSubCategoryDropDown();
    }

    $catId.change(function (e) {
        e.preventDefault();
        renderSubCategoryDropDown();
    });

    //function to render sub category list for selected category
    function renderSubCategoryDropDown() {
        var selCategoryId = $catId.val();
        $subCatddl.empty();
        $subCatddl.append($('<option/>', { value: '0', text: 'Select' }));

        if (selCategoryId > 0) {
            UTILS.data.getAllDocumentSubCategoryForCategory(selCategoryId, function (data) {
                if (data && data.subCategoryList != null) {
                    $.each(data.subCategoryList, function (index, item) {
                        $subCatddl.append($('<option/>', {
                            value: item.SubCategoryId,
                            text: item.SubCategoryName
                        }))
                    });
                }
            });
        }
    }

    $('#newDocFile').on('change', function () {
        //get the file name
        var fileName = document.getElementById("newDocFile").files[0].name; // $(this).val();
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    })

    $createBtn.click(function () {
        if (validate()) {
            UTILS.disableButton($createBtn);
            //$form.submit();

            var url = hdnBaseUrl + "Document/CreateDocument"

            var fileUpload = $("#newDocFile").get(0);
            var files = fileUpload.files;
            // Create FormData object  
            var documentData = new FormData();

            // Looping over all files and add it to FormData object  
            for (var i = 0; i < files.length; i++) {
                documentData.append(files[i].name, files[i]);
            }
            // Adding one more key to FormData object  
            documentData.append('CategoryId', $catId.val());
            documentData.append('SubCategoryId', $subCatddl.val());
            documentData.append('DocumentName', $docName.val());
            documentData.append('DocumentDesc', $docDesc.val());
            documentData.append('Version', $docVersion.val());
            documentData.append('DateOfPublish', $docDtPublish.val());
            documentData.append('DateOfReview', $docDtReview.val());

            $.ajax({
                url: url,
                type: "POST",
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: documentData,
                success: function (result) {
                    if (result.status > 0) {
                        UTILS.Alert.show($alert, "success", "Document uploaded successfuly. <a href='#' onclick='addDocHandler.editDocumentVisibility(" + result.status + ")'>Click here</a> to set visibility of the document.");
                        $('#orgDocumentList').DataTable().draw();
                    }
                    else {
                        UTILS.Alert.show($alert, "error", "Failed to upload document.")
                    }
                },
                error: function (err) {
                    console.log(err.statusText);
                    UTILS.Alert.show($alert, "error", "Failed to upload document. Please try again later")
                }
            });
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {

        var fileUpload = $("#newDocFile").get(0);
        var files = fileUpload.files;

        if ($catId.val() == null || $catId.val().trim() == '' || $catId.val() == '0') {
            UTILS.Alert.show($alert, "error", "Please select a category.")
            return false
        }
        else if ($docName.val() == null || $docName.val().trim() == '') {
            UTILS.Alert.show($alert, "error", "Please enter title for document.")
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

    function editDocumentVisibility(updatedociD) {
        window.location.href = hdnBaseUrl + 'Document/DocumentAllocation?id=' + updatedociD + '&doc=' + $docName.val();
    }

    return {
        showAddDocPopUP: showAddDocPopUP,
        editDocumentVisibility: editDocumentVisibility
    }
})();

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
            '</ul></div>';
        UTILS.Alert.show($alertDoc, 'default', message);
    };

    $('#btnShowDocPopUp').click(function () {
        addDocHandler.showAddDocPopUP();
    });

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
                url: "LoadDocumentData",
                type: "POST",
                datatype: "json",
                data: function (data) {
                    data.SearchText = $document.val();
                    data.Category = $category.val();
                    data.SubCategory = $subcategory.val();
                    data.IsActive = $docStatus.val();
                },
                error: function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            drawCallback: function (respData) {
                if (respData.json.listFor == 8) $('.btn-del-doc').remove();
            },
            columns: [
                { data: "DocumentName", name: "d.TX_NAME", width: "30%" },
                { data: "CategoryName", name: "c.TX_NAME", width: "15%" },
                { data: "SubCategoryName", name: "s.strSubCategory", width: "15%" },
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
                        const isArchived = data.DocIsArchived == 1;
                        const archiveBtn = isArchived
                            ? `<li><a class="dropdown-item" href="#" id="activate-doc-${id}" onclick="documentReportHandler.activateDocument(this)">
                            <i class="fa fa-sync me-2"></i>Activate</a></li>`
                            : `<li><a class="dropdown-item" href="#" id="archive-doc-${id}" onclick="documentReportHandler.archiveDocument(this)">
                            <i class="fa fa-power-off me-2"></i>Archive</a></li>`;

                        return `
                <div class="dropdown">
                    <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                            id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                            style="width: 2.5rem; height: 2.5rem;">
                        <i class="fa fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                        <li><a class="dropdown-item" href="#" id="edit-doc-${id}" onclick="documentReportHandler.showUpdateDocumentPopUP(this)">
                            <i class="fa fa-edit me-2"></i>Edit</a></li>
                        <li><a class="dropdown-item" href="${data.DocumentPath}" target="_blank" id="preview-doc-${id}">
                            <i class="fa fa-eye me-2"></i>Preview</a></li>
                        <li><a class="dropdown-item btn-del-doc" href="#" id="delete-doc-${id}" onclick="documentReportHandler.deleteDocument(this)">
                            <i class="fa fa-trash me-2"></i>Delete</a></li>
                        ${archiveBtn}
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

    //function to open modal for update category
    function showUpdateDocumentPopUP(btn) {
        UTILS.Alert.hide($alert)

        let documentTable = $('#orgDocumentList').DataTable();

        update_docid = btn.id.split('-').pop();
        var docName = documentTable.row(btn.closest('tr')).data()["DocumentName"];
        var docDesc = documentTable.row(btn.closest('tr')).data()["DocumentDesc"];
        var docCat = documentTable.row(btn.closest('tr')).data()["CategoryId"];
        var docSubCat = documentTable.row(btn.closest('tr')).data()["SubCategoryId"];
        var docVer = documentTable.row(btn.closest('tr')).data()["Version"];
        var docPublishDt = documentTable.row(btn.closest('tr')).data()["DateOfPublish"];
        var docReviewDt = documentTable.row(btn.closest('tr')).data()["DateOfReview"];
        $docName.val(docName);
        $docDesc.val(docDesc);
        $docCategory.val(docCat);
        renderSubCategoryDropDown_popup()
        $docSubCategory.val(docSubCat);
        $docVersion.val(docVer);
        $docDtPublish.val(docPublishDt);
        $docDtReview.val(docReviewDt);

        $modal.modal('show');        
    }

    $docCategory.change(function (e) {
        e.preventDefault();
        renderSubCategoryDropDown_popup()
    });

    //fuction to update document
    $updateInfo.click(function () {
        if (validate()) {
            var url = hdnBaseUrl + "Document/UpdateDocument";
            var data = {
                DocumentName: $docName.val(),
                DocumentDesc: $docDesc.val(),
                CategoryId: $docCategory.val(),
                SubCategoryId: $docSubCategory.val(),
                Version: $docVersion.val(),
                DateOfPublish: $docDtPublish.val(),
                DateOfReview: $docDtReview.val(),
                DocumentID: update_docid
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', "Document updated successfully");
                    $('#orgDocumentList').DataTable().draw();
                } else {
                    UTILS.Alert.show($alert, 'error', "Failed to update document.")
                }
            }, function (err) {
                UTILS.Alert.show($alert, 'error', "Failed to update document. Please try again later.")
            })
        }
    });

    //functio to validate
    function validate() {
        if ($docName.val() == null || $docName.val().trim() == '') {
            UTILS.Alert.show($alert, 'error', 'Please enter document name.')
            return false;
        }
        else if ($docCategory.val() == null || $docCategory.val() == undefined || $docCategory.val() == '0') {
            UTILS.Alert.show($alert, 'error', 'Please select a category.')
            return false;
        }
        else {
            return true;
        }
    }

    //function to delete document and refresh table
    function deleteDocument(btn) {
        if (confirm("Warning\nAre you sure you want to delete this document?\nDeleting the document will also instantly remove all of its reporting history!\nTo continue and accept this deletion click OK")) {
            var doc_id = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + 'Document/DeleteDocument',
                dataType: 'json',
                data: { DocumentID: doc_id },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alertDoc, 'success', 'Document deleted successfully.');
                        $('#orgDocumentList').DataTable().row($(btn).parents('tr')).remove().draw();
                    }
                    else
                        UTILS.Alert.show($alertDoc, 'error', 'Failed to delete document.');
                },
                error: function (status) {
                    UTILS.Alert.show($alertDoc, 'error', 'Failed to delete document. Please try again later');
                }
            });
        }
    }

    //function to delete document and refresh table
    function archiveDocument(btn) {
        if (confirm("Are you sure you want to archive this document?")) {
            var doc_id = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + 'Document/ArchiveDocument',
                dataType: 'json',
                data: { DocumentID: doc_id },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alertDoc, 'success', 'Document archived successfully.');
                        $('#orgDocumentList').DataTable().row($(btn).parents('tr')).remove().draw();
                    }
                    else
                        UTILS.Alert.show($alertDoc, 'error', 'Failed to archive document.');
                },
                error: function (status) {
                    UTILS.Alert.show($alertDoc, 'error', 'Failed to archive document. Please try again later');
                }
            });
        }
    }

    //function to activate document and refresh table
    function activateDocument(btn) {
        if (confirm("Are you sure you want to activate this document?")) {
            var doc_id = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + 'Document/ActivateDocument',
                dataType: 'json',
                data: { DocumentID: doc_id },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alertDoc, 'success', 'Document activated successfully.');
                        $('#orgDocumentList').DataTable().row($(btn).parents('tr')).remove().draw();
                    }
                    else
                        UTILS.Alert.show($alertDoc, 'error', 'Failed to activate document.');
                },
                error: function (status) {
                    UTILS.Alert.show($alertDoc, 'error', 'Failed to activate document. Please try again later');
                }
            });
        }
    }

    //function to preview document
    function previewDocument(btn) {

        let documentTable = $('#orgDocumentList').DataTable();
        var obj = {
            docPath: documentTable.row(btn.closest('tr')).data()["DocumentPath"]
        }
        
        $.ajax({
            type: "POST",
            url: hdnBaseUrl + 'Document/GetCloudDocumentURLString',
            data: JSON.stringify(obj),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response) {
                    window.open(response, '_blank');
                }
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    }

    function editDocumentVisibility(btn) {

        let documentTable = $('#orgDocumentList').DataTable();
        var updatedociD = btn.id.split('-').pop();
        var docName = documentTable.row(btn.closest('tr')).data()["DocumentName"];
        window.location.href = hdnBaseUrl + 'Document/DocumentAllocation?id=' + updatedociD+'&doc=' + docName;
    }

    return {
        init: init,
        deleteDocument: deleteDocument,
        renderCategoryDropDown: renderCategoryDropDown,
        showUpdateDocumentPopUP: showUpdateDocumentPopUP,
        previewDocument: previewDocument,
        editDocumentVisibility: editDocumentVisibility,
        activateDocument: activateDocument,
        archiveDocument: archiveDocument
    }
})();

