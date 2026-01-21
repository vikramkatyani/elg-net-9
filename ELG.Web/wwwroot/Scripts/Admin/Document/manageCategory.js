$(function () {
    UTILS.activateNavigationLink('docLink');
    UTILS.activateMenuNavigationLink('menu-category-report');
    $('[data-toggle="tooltip"]').tooltip();
});

var categoryReportHandler = (function () {
    var $category = $('#txtDocCategory');
    var $searchBtn = $('#searchDocCategory');
    var $clearSearchBtn = $('#clearSearchDocCategory');
    var $createBtn = $('#btnShowCategoryPopUp');
    var $alertCat = $('#messageCategory');

    var $modal = $('#updateCategoryModal');
    var $catName = $('#txtUpdateCategoryName');
    var $catDesc = $('#txtUpdateCategoryDesc');
    var $alert = $('#updateCategoryMessage');
    var $updateInfo = $('#btnUpdateCategory');

    var update_catId = 0;

    $createBtn.click(function () {
        addCategoryHandler.showCreatePopUp();
    });

    var categoryTable = $('#docCategoryList').DataTable({
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
            url: "LoadDocCategoryData",
            type: "POST",
            datatype: "json",
            data: function (data) {
                data.SearchText = $category.val();
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
            { data: "CategoryName", name: "TX_NAME", width: "75%" },
            { data: "CreatedOn", name: "DT_CREATEDON", width: "15%" },
            { data: null, orderable: false, searchable: false, width: "10%" }
        ],
        columnDefs: [
            {
                targets: 0,
                render: function (a, b, data, d) {
                    let html = `<span data-bs-toggle="tooltip" title="${data.CategoryDesc}">${data.CategoryName}</span>`;
                    if (data.SubCategoryList.length > 0) {
                        html += '<div class="mt-3"><table class="table table-sm table-borderless mb-0">';
                        data.SubCategoryList.forEach(sub => {
                            html += `<tr><td><span class="sub-desc-info" data-bs-toggle="tooltip" title="${sub.SubCategoryDesc}">${sub.SubCategoryName}</span></td><td class="text-end">`;
                            html += `<button type="button" class="btn btn-sm me-1" onclick="subcategoryReportHandler.showUpdateSubCategoryPopUP(this)" id="edit-sub-category-${sub.SubCategoryId}"><i class="fa fa-edit"></i></button>`;
                            html += `<button type="button" class="btn btn-sm btn-del-cat" onclick="subcategoryReportHandler.deleteSubCategory(this)" id="delete-sub-category-${sub.SubCategoryId}"><i class="fa fa-trash"></i></button>`;
                            html += '</td></tr>';
                        });
                        html += '</table></div>';
                    }
                    return html;
                }
            },
            {
                targets: 2,
                className: "text-center align-middle",
                render: function (a, b, data, d) {
                    const id = data.CategoryId;
                    return `
                    <div class="dropdown">
                        <button class="btn btn-sm border-0 p-2 rounded-circle" type="button"
                                id="actionDropdown-${id}" data-bs-toggle="dropdown" aria-expanded="false"
                                style="width: 2.5rem; height: 2.5rem;">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="actionDropdown-${id}">
                            <li><a class="dropdown-item" href="#" id="edit-category-${id}" onclick="categoryReportHandler.showUpdateCategoryPopUP(this)">
                                <i class="fa fa-edit me-2"></i>Edit</a></li>
                            <li><a class="dropdown-item btn-del-cat" href="#" id="delete-category-${id}" onclick="categoryReportHandler.deleteCategory(this)">
                                <i class="fa fa-trash me-2"></i>Delete</a></li>
                            <li><a class="dropdown-item" href="#" id="add-sub-category-${id}" onclick="addSubCategoryHandler.showCreateSubCategoryPopUp(this)">
                                <i class="fa fa-plus-circle me-2"></i>Add Sub Category</a></li>
                            <li><a class="dropdown-item btn-map-cat" href="#" id="map-loc-category-${id}" onclick="categoryReportHandler.mapLocationCategory(this)">
                                <i class="fa fa-link me-2"></i>Assign Location</a></li>
                        </ul>
                    </div>`;
                }
            }
        ]
    });

    $searchBtn.click(function (e) {
        e.preventDefault();
        categoryTable.draw();
    });

    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $category.val('');
        categoryTable.draw();
    });

    function mapLocationCategory(btn) {
        const catID = btn.id.split('-').pop();
        const catName = categoryTable.row(btn.closest('tr')).data()["CategoryName"];
        window.location.href = hdnBaseUrl + 'Document/CategoryLocationMap?id=' + catID + '&category=' + catName;
    }

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

    function deleteCategory(btn) {
        if (confirm("Are you sure you want to delete this category? This will also delete all the documents in the category")) {
            const cat_id = btn.id.split('-').pop();
            $.ajax({
                type: 'post',
                url: hdnBaseUrl + 'Document/DeleteCategory',
                dataType: 'json',
                data: { CategoryId: cat_id },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alertCat, 'success', 'Category deleted successfully.');
                        categoryTable.row($(btn).parents('tr')).remove().draw();
                    } else if (result.success == 0) {
                        UTILS.Alert.show($alertCat, 'success', 'Can\'t delete Category. There are documents in the category.');
                    } else {
                        UTILS.Alert.show($alertCat, 'error', 'Failed to delete category.');
                    }
                },
                error: function () {
                    UTILS.Alert.show($alertCat, 'error', 'Failed to delete category. Please try again later.');
                }
            });
        }
    }

    return {
        showUpdateCategoryPopUP,
        deleteCategory,
        mapLocationCategory
    };
})();


var addCategoryHandler = (function () {
    var $modal = $('#addCategoryModal');
    var $catName = $('#txtCategoryName');
    var $catDesc = $('#txtCategoryDesc');
    var $alert = $('#addCategoryMessage');
    var $createBtn = $('#btnAddNewCategory');

    function showCreatePopUp() {
        $modal.modal('show')
    }

    $createBtn.click(function () {
        if (validate()) {
            UTILS.disableButton($createBtn);
            var url = hdnBaseUrl + "Document/CreateCategory"
            var data = {
                CategoryName: $catName.val(),
                CategoryDesc: $catDesc.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success > 0) {
                    UTILS.Alert.show($alert, "success", "Category created successfully");
                    $('#docCategoryList').DataTable().draw();
                }
                else if (res.success == 0) {
                    UTILS.Alert.show($alert, "error", "Category already exists.")
                }
                else {
                    UTILS.Alert.show($alert, "error", "Failed to add category. Please try again later")
                }

                loading = false;
                UTILS.resetButton($createBtn);
            },
                function (err) {
                    console.log(err)
                })
        }
        else {
            UTILS.Alert.show($alert, "error", "Category name can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($catName.val() != null && $catName.val().trim() != '') {
            return true
        }
        else {
            return false;
        }
    }
    return{
        showCreatePopUp: showCreatePopUp
    }
})();
