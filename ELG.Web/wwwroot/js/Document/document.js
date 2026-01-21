document.addEventListener('DOMContentLoaded', function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-document-report');
    documentReportHandler.init();
    
    // Initialize Bootstrap 5 tooltips
    document.querySelectorAll('[data-toggle="tooltip"]').forEach(tooltip => {
        new bootstrap.Tooltip(tooltip);
    });
});

let docWindow = null;

var documentReportHandler = (function () {
    const documentInput = document.querySelector('#txtDoc');
    const categorySelect = document.querySelector('#ddlCategory');
    const statusSelect = document.querySelector('#ddlDocStatus');
    const searchBtn = document.querySelector('#searchDoc');
    const clearSearchBtn = document.querySelector('#clearSearchDoc');
    const alertElement = document.querySelector('#messageDocument');

    const btnShowDocPopUp = document.querySelector('#btnShowDocPopUp');
    if (btnShowDocPopUp) {
        btnShowDocPopUp.addEventListener('click', function () {
            addDocHandler.showAddDocPopUP();
        });
    }

    const documentTable = new DataTable('#orgDocumentList', {
        processing: true,
        language: {
            processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            emptyTable: "No record(s) found."
        },
        serverSide: true,
        filter: false,
        ordering: false,
        ajax: {
            url: "LoadDocumentData",
            type: "POST",
            datatype: "json",
            data: function (data) {
                    data.SearchText = documentInput ? (documentInput.value || '') : '';
                    data.Category = categorySelect ? (categorySelect.value || '0') : '0';
                    data.IsActive = statusSelect ? (statusSelect.value || '0') : '0';
            },
                error: function (xhr, error, code) {
                alert("Oops! Something went wrong please try again later.");
                window.location.reload();
            }
        },
        columns: [
            { data: "DocumentName", name: "DocumentName", width: "35%" },
            { data: "CategoryName", name: "CategoryName", width: "25%" },
            { data: null, name: "Action", width: "20%", orderable: false },
            { data: null, name: "Status", width: "20%", orderable: false }
        ],
        columnDefs: [{
            // render document name with description
            targets: [0], render: function (a, b, data, d) {
                return ' <span style="pointer:cursor" data-toggle="tooltip" title="' + data["DocumentDesc"] + '">' + data["DocumentName"] + '</span>'
            }   
        }, {
            // render action buttons in the last column
            targets: [2], render: function (a, b, data, d) {
                return `<div class="text-center">
                    <button onclick="documentReportHandler.markDocumentRead(this)" 
                            id="preview-doc-${data["DocumentID"]}" 
                            class="btn btn-sm btn-primary" 
                            title="Preview Document">
                        <i class="fa fa-eye me-1"></i>Preview
                    </button>
                </div>`;
            }
        }, {
            // render status column
                targets: [3], render: function (a, b, data, d) {
                    if (!data.DocumentViewed) {
                        return '<span>Not viewed</span>';
                    } else {
                        return '<span>' + (data.DocumentStatus ? data.DocumentStatus : '') + '</span>';
                    }
                }
        }],
    });

    // Apply filters for search
    if (searchBtn) {
        searchBtn.addEventListener('click', function (e) {
            e.preventDefault();
            documentTable.draw();
        });
    }

    // Clear search filters
    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (documentInput) documentInput.value = '';
            if (categorySelect) categorySelect.value = '0';
            documentTable.draw();
        });
    }

    // Function to initialize the view, bind all drop downs
    function init() {
        renderCategoryDropDown();
    }

    // Function to render list of all document categories
    function renderCategoryDropDown() {
        if (!categorySelect) return;
        categorySelect.innerHTML = '';
        const defaultOption = document.createElement('option');
        defaultOption.value = '0';
        defaultOption.textContent = 'Select All';
        categorySelect.appendChild(defaultOption);

        UTILS.data.getAllDocumentCategory(function (data) {
            if (data && data.catList != null) {
                data.catList.forEach(function (item) {
                    const dropdowns = document.querySelectorAll('.ddlCat');
                    dropdowns.forEach(function (dropdown) {
                        const option = document.createElement('option');
                        option.value = item.CategoryId;
                        option.textContent = item.CategoryName;
                        dropdown.appendChild(option);
                    });
                });
            }
        });
    }

    // Mark document as read
    function markDocumentRead(dropdown) {
        const docId = dropdown.id.split('-').pop();
        openPreviewDoc(docId);
        const url = hdnBaseUrl + "MarkDocumentRead";
        const data = {
            DocumentId: docId
        };
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success === 1) {
                documentTable.draw();
            }
        }, function (err) {
        });
    }

    // Set status for the document
    function setDocStatus(dropdown) {
        const docId = dropdown.id.split('-').pop();
        const statusDropdown = document.querySelector(`#ddl-status-${docId}`);
        const status = statusDropdown ? statusDropdown.value : '';
        if (confirm('Are you sure you want to update the status?')) {
            const url = hdnBaseUrl + "SetDocumentStatus";
            const data = {
                DocumentId: docId,
                Status: status
            };
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success === 1) {
                    UTILS.Alert.show(alertElement, 'success', 'Document status updated successfully.');
                    documentTable.draw();
                } else {
                    UTILS.Alert.show(alertElement, 'error', 'Failed to update document status.');
                }
            }, function (err) {
                UTILS.Alert.show(alertElement, 'error', 'Failed to update document status. Please try again later.');
            });
        }
    }

    function openPreviewDoc(docID) {
        docWindow = window.open(`${hdnBaseUrl}Preview/${docID}`);
    }

    return {
        init: init,
        renderCategoryDropDown: renderCategoryDropDown,
        setDocStatus: setDocStatus,
        markDocumentRead: markDocumentRead 
    }
})();
