$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-document-accesss');
    documentReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

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
    var $docVersion = $('#txtUpdateDocVersion');
    var $docDtPublish = $('#txtUpdateDocDatePublished');
    var $docDtReview = $('#txtUpdateDocDtReview');
    var $alert = $('#updateDocumentMessage');
    var update_docid = 0;
    var $reportContainer = $("#div-report-container");
    function showDefaultMessage() {
        $reportContainer.hide();
        $('#orgDocumentList').DataTable().destroy();
        var message = '<div ><b>How to use this page:</b><ul>' +
            '<li>Use the <strong>Document</strong> field to search by document name.</li>' +
            '<li>Select a <strong>Category</strong> and <strong>Sub Category</strong> to narrow down your results.</li>' +
            '<li>Click <i class="fa fa-search me-1"></i> <strong>Search</strong> to view matching documents.</li>' +
            '<li>Click <i class="fa fa-times me-1"></i> <strong>Clear</strong> to reset all filters and start a new search.</li>' +
            '<li>Use the <i class="fa fa-ellipsis-v me-1"></i> action menu in each row to manage access for that document.</li>' +
            '</ul></div>';
        UTILS.Alert.show($alertDoc, 'default', message);
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        $reportContainer.show();
        UTILS.Alert.hide($alertDoc);
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
                    data.IsActive = 1;
                },
                error: function (xhr, error, code) {
                    console.log(xhr);
                    console.log(code);
                    alert("Oops! Something went wrong please try again later.");
                }
            },
            drawCallback: function (respData) {
                if (respData.json.listFor == 3 || respData.json.listFor == 8) {
                    $('.btn-doc-auto').remove();
                }
            },
            columns: [
                { data: "DocumentName", name: "d.TX_NAME", width: "25%" },
                { data: "CategoryName", name: "c.TX_NAME", width: "15%" },
                { data: "SubCategoryName", name: "s.strSubCategory", width: "15%" },
                { data: "Version", name: "d.version", width: "10%" },
                { data: "DateOfPublish", name: "d.dateOfPublish", width: "15%" },
                { data: "DateOfReview", name: "d.dateOfReview", width: "15%" },
                { data: null, orderable: false, searchable: false, width: "5%" }
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
                        return `
                <div class="dropdown">
                    <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                            id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                            style="width: 2.5rem; height: 2.5rem;">
                        <i class="fa fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                        <li>
                            <a class="dropdown-item btn-doc-auto" href="#" id="doc-auto-${id}" onclick="documentReportHandler.editDocumentVisibility(this)">
                                <i class="fa fa-link me-2"></i>Auto Assign
                            </a>
                        </li>
                        <li>
                            <a class="dropdown-item" href="#" id="doc-indv-${id}" onclick="documentReportHandler.editIndividualDocVisibility(this)">
                                <i class="fa fa-user me-2"></i>Individual Assign
                            </a>
                        </li>
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

    function editDocumentVisibility(btn) {
        var updatedociD = btn.id.split('-').pop();
        var docName = $('#orgDocumentList').DataTable().row(btn.closest('tr')).data()["DocumentName"];
        window.location.href = hdnBaseUrl + 'Document/DocumentAllocation?id=' + updatedociD+'&doc=' + docName;
    }

    function editIndividualDocVisibility(btn) {
        var updatedociD = btn.id.split('-').pop();
        var docName = $('#orgDocumentList').DataTable().row(btn.closest('tr')).data()["DocumentName"];
        window.location.href = hdnBaseUrl + 'Document/DocumentAllocationIndividual?id=' + updatedociD+'&doc=' + docName;
    }

    return {
        init: init,
        renderCategoryDropDown: renderCategoryDropDown,
        editDocumentVisibility: editDocumentVisibility,
        editIndividualDocVisibility: editIndividualDocVisibility
    }
})();

