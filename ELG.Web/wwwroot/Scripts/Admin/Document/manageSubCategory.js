
var subcategoryReportHandler = (function () {

    var $modal = $('#updateSubCategoryModal');
    var $subcatName = $('#txtSubUpdateCategoryName');
    var $subcatDesc = $('#txtUpdateSubCategoryDesc');
    var $alert = $('#updateSubCategoryMessage');
    var $alertCat = $('#messageCategory');
    var $updateInfo = $('#btnUpdateSubCategory');

    var update_catId = 0;
       
    //function to open modal for update category
    function showUpdateSubCategoryPopUP(btn) {
        UTILS.Alert.hide($alert)

        update_catId = btn.id.split('-').pop();
        var el = $(btn).closest('tr').find('.sub-desc-info')
        var catName = el.html();
        var catDesc = el.attr('title');
        $subcatName.val(catName);
        $subcatDesc.val(catDesc);

        $modal.modal('show');
    }

    //fuction to update category
    $updateInfo.click(function () {
        if (validate()) {
            var url = hdnBaseUrl + "Document/UpdateSubCategory";
            var data = {
                SubCategoryId: update_catId,
                SubCategoryName: $subcatName.val(),
                SubCategoryDesc: $subcatDesc.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', "Sub Category updated successfully");
                    $('#docCategoryList').DataTable().draw();
                } else {
                    UTILS.Alert.show($alert, 'error', "Failed to update sub category.")
                }
            }, function (err) {
                    UTILS.Alert.show($alert, 'error', "Failed to update sub category. Please try again later.")
                })
        }
    });

    //function to validate
    function validate() {
        if ($subcatName.val() != null && $subcatName.val().trim() != '') {
            return true;
        }
        else {
            UTILS.Alert.show($alert, 'error', 'Please enter sub category name.')
            return false;
        }
    }

    //function to delete category and refresh table
    function deleteSubCategory(btn) {
        if (confirm("Are you sure you want to delete this sub category?")) {
            var cat_id = btn.id.split('-').pop();
            //selector
            $.ajax({
                type: 'post',
                url: hdnBaseUrl +'Document/DeleteSubCategory',
                dataType: 'json',
                data: { SubCategoryId: cat_id },
                success: function (result) {
                    if (result.success == 1) {
                        UTILS.Alert.show($alertCat, 'success', 'Sub Category deleted successfully.');
                        $(btn).closest('tr').remove();
                    }
                    else if (result.success == 0) {
                        UTILS.Alert.show($alertCat,'success', 'Can\'t delete Sub Category there are documents in the sub category.');
                    }
                    else
                        UTILS.Alert.show($alertCat, 'error', 'Failed to delete sub category.');
                },
                error: function (status) {
                    UTILS.Alert.show($alertCat, 'error', 'Failed to delete sub category. Please try again later');
                }
            });
        }
    }


    return {
        showUpdateSubCategoryPopUP: showUpdateSubCategoryPopUP,
        deleteSubCategory: deleteSubCategory
    }
})();


var addSubCategoryHandler = (function () {
    var $modal = $('#addSubCategoryModal');
    var $catName = $('#txtSubCategoryName');
    var $catDesc = $('#txtSubCategoryDesc');
    var $alert = $('#addSubCategoryMessage');
    var $createBtn = $('#btnAddNewSubCategory');
    var $catid = 0;

    function showCreateSubCategoryPopUp(btn) {
        $catid = btn.id.split('-').pop();
        $modal.modal('show')
    }

    $createBtn.click(function () {
        if ($catid == null || $catid == NaN || $catid < 1) {
            UTILS.Alert.show($alert, "error", "Failed to add sub-category. Please try again later");
            return false;
        }
        else if (validate()) {
                UTILS.disableButton($createBtn);
                var url = hdnBaseUrl + "Document/CreateSubCategory"
                var data = {
                    CategoryId: $catid,
                    SubCategoryName: $catName.val(),
                    SubCategoryDesc: $catDesc.val()
                }
                UTILS.makeAjaxCall(url, data, function (res) {
                    if (res.success > 0) {
                        UTILS.Alert.show($alert, "success", "Sub-Category created successfully");
                        $catName.val('');
                        $catDesc.val('');
                        $('#docCategoryList').DataTable().draw();
                    }
                    else if (res.success == 0) {
                        UTILS.Alert.show($alert, "error", "Sub-Category already exists.");
                    }
                    else {
                        UTILS.Alert.show($alert, "error", "Failed to add sub-category. Please try again later");
                    }

                    loading = false;
                    UTILS.resetButton($createBtn);
                },
                    function (err) {
                        console.log(err)
                    })
        }
        else {
            UTILS.Alert.show($alert, "error", "Sub-Category name can't be empty.")
            return;
        }
        UTILS.resetButton($createBtn);
    });

    function validate() {
        if ($catName.val() != null && $catName.val().trim() != '') {
            return true;
        }
        else {
            return false;
        }
    }
    return{
        showCreateSubCategoryPopUp: showCreateSubCategoryPopUp
    }
})();
