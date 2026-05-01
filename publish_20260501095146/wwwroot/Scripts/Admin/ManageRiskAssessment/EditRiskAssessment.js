$(function () {
    UTILS.activateMenuNavigationLink('manageRALink');
    editRiskAssessmentHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var editRiskAssessmentHandler = (function () {
    var table = $('#riskAssessmentQuestionList');
    table.html('');
    table.append('<thead> <tr> <th>Order</th><th>Group</th><th>Question</th> <th>Options</th> <th>Action</th></tr> </thead>');

    function init() {
        var url = hdnBaseUrl + "RiskAssessment/GetRiskAssessmentQuestion";
        var data = {
        }
        UTILS.makeAjaxCall(url, data, function (res) {
            populateData(res);
        }, function (err) {
            console.log(err)
        })
    }

    function populateData(data) {
        if (data != null && data.quesList.length > 0) {
            $('#spnRACourseName').html(data.quesList[0].CourseName);
            for (var ques in data.quesList) {
                var row = '<tr> ' +
                    '<td>' + data.quesList[ques].Order + '</td>' +
                    '<td> ' + data.quesList[ques].Group + '</td>' +
                    ' <td> ' + data.quesList[ques].QuestionText + '</td>' +
                    ' <td>' + data.quesList[ques].OptionCount + '</td>' +
                    ' <td> ' +
                    '<button type="button" id="update-raq-' + data.quesList[ques].QuestionId + '" class="btn btn-sm btn-dark mb-1"  onclick="editRAQuestionHandler.showRAQModal(this)" > <i class="fa fa-fw fa-edit"></i> <span> Edit</span></button >' +
                    ' <button type="button" id="remove-raq-' + data.quesList[ques].QuestionId + '" class="btn btn-sm btn-dark mb-1" onclick="manageRAHandler.deleteRAQuestions(this)" > <i class="fa fa-fw fa-trash"></i> <span> Remove</span></button >' +
                    ' </td >' +
                    '</tr> ';

                table.append(row);
            }
        }
        else {
            table.parent().append('<div class="alert alert-info">No record(s) found.</div>');
        }
    }

    return {
        init : init
    }

})()


var manageRAHandler = (function () {

    $alert = $("#messageRAQues");

    function deleteRAQuestions(btn) {
        var que = btn.id.split('-').pop();
        if (confirm('Are you sure you want to delete this question from the risk assessment')) {
            var url = hdnBaseUrl + "RiskAssessment/RemoveRAQuestion";
            var data = {
                QuestionId: que
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Question removed successfully');
                    editRiskAssessmentHandler.init();
                } else {
                    UTILS.Alert.show($alert, 'error', 'Failed to remove question');
                }
            }, function (err) {
                console.log(err);
                UTILS.Alert.show($alert, 'error', 'Failed to remove question. Please try again later');
            });
        }
    }

    return {
        deleteRAQuestions: deleteRAQuestions
    }

})();

var editRAQuestionHandler = (function () {

    var $modal = $("#editRAQModal");
    var $alert = $("#messageRAQEdit");
    var $groupList = $("#ddlEditRAQGroup");
    var $question = $("#txtEditRAQusetion");
    var $order = $("#txtEditRAQOrder");
    var $updateInfo = $('#btnAddNewDoc');
    var queId = 0;

    var $newOptionOrder = $('#editRAQModal #txtNewOptionOrder');
    var $newOption = $("#editRAQModal #txtNewOption");
    var $newOptionIssue = $("#editRAQModal #chkNewOptionIssue");
    var $btnAddNewOption = $("#editRAQModal #btnAddNewOption");

    var optionsTable = $('#edit_raQuestionOptionList').DataTable({
        lengthChange: false,
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "ordering": false,
        "paging": false,
        "info": false,
        "ajax": {
            "url": hdnBaseUrl+"RiskAssessment/GetRiskAssessmentQuestionOptions",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.Course = queId;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "Order", "name": "Order", "autoWidth": true },
            { "data": "Option", "name": "Option", "autoWidth": true },
            { "data": "Issue", "name": "Issue", "autoWidth": true }
        ],
        columnDefs: [{
            // render order text box
            targets: [0], render: function (a, b, data, d) {
                return "<input type='text' class='form-control option-order' value='" + data["Order"] + "' />"
            }
        }, {
            // render order text box
            targets: [1], render: function (a, b, data, d) {
                return "<input type='text' class='form-control option-txt' value='" + data["OptionText"] + "' />"
            }
        }, {
            // render check box
            targets: [2], render: function (a, b, data, d) {
                if (data["Issue"])
                    return "<input type='checkbox' class='option-chk' checked />"
                else
                    return "<input type='checkbox' class='option-chk' />"
            }
        }, {
            // render action buttons in the last column
            targets: [3], render: function (a, b, data, d) {
                return '<button type="button"id="update-option-' + data["QuestionOptionId"] + '" class="btn btn-sm btn-dark mb-1" onclick="editRAQuestionHandler.updateRAQuestionOption(this)"><i class="fa fa-fw fa-edit"></i><span>Update</span></button> '
                    + '<button type="button" id="delete-option-' + data["QuestionOptionId"] + '" class="btn btn-sm btn-dark mb-1" onclick="editRAQuestionHandler.deleteRAQuestionOption(this)"><i class="fa fa-fw fa-trash"></i><span>Delete</span></button> '

            }
        }]
    }); 

    function showRAQModal(btn) {
        queId = btn.id.split('-').pop();
        renderGroupDropDown();
        $modal.modal({
            backdrop: 'static',
            keyboard: true
        });
    }

    $modal.on('show.bs.modal', function (e) {
        UTILS.Alert.hide($alert);
        optionsTable.draw();
        getQuestionInfo();
    });

    //function get question details
    function getQuestionInfo() {
        var url = hdnBaseUrl + "RiskAssessment/GetRAQuestionInfo";
        var data = {
            QuestionId: queId
        }
        UTILS.makeAsyncAjaxCall(url, data, function (que) {
            $groupList.val(que.que.GroupId);
            $question.val(que.que.QuestionText);
            $order.val(que.que.Order);
            //optionsTable.draw();

            var newOrder = (optionsTable.data().count() + 1) * 10;
            $newOptionOrder.val(newOrder);

        }, function () {
            UTILS.Alert.show($alert,'error', "Something went wrong. Please try again later")
        })
    }

    //add new option
    $btnAddNewOption.click(function (e) {
        e.preventDefault();

        var order = $newOptionOrder.val();
        var option = $newOption.val();
        var issue = $newOptionIssue.prop('checked');

        var url = hdnBaseUrl + "RiskAssessment/CreateRAQuestionOption";
        var data = {
            QuestionId: queId,
            Option: option,
            Issue: issue,
            Order: order
        }

        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success > 0) {
                optionsTable.draw();
                $newOptionOrder.val((optionsTable.data().count() + 1) * 10);
                $newOption.val('');
                $newOptionIssue.prop('checked', false);
                UTILS.Alert.show($alert, "success", "Option added successfully.")
            }
            else {
                UTILS.Alert.show($alert, "error", "Failed to create new option.")
            }

        }, function (err) {
            console.log(err);
            UTILS.Alert.show($alert, "error", "Failed to create new option. Please try again later.");
        })

    });

    //function to render list of all groups
    function renderGroupDropDown() {
        $groupList.empty();
        //$groupList.append($('<option/>', { value: '0', text: 'Select' }));

        UTILS.data.getAllRAGroups(function (data) {
            if (data && data.groupList != null) {
                $.each(data.groupList, function (index, item) {
                    $groupList.append($('<option/>', {
                        value: item.GroupID,
                        text: item.GroupName
                    }))
                });
            }
        })
    }

    //delete option
    function deleteRAQuestionOption(btn) {
        if (confirm('Are you sure you want to delete this option?')) {
            var optionid = btn.id.split('-').pop();
            var url = hdnBaseUrl + "RiskAssessment/DeleteOption";
            var data = {
                OptionId: optionid
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Option deleted successfully.');
                    optionsTable.draw();
                    editRiskAssessmentHandler.init();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to delete option.')
            }, function (err) {
                UTILS.Alert.show($alert, 'error', 'Failed to delete option. Please try again later.')
            })
        }
    }

    //update option info
    function updateRAQuestionOption(btn) {
            var optionid = btn.id.split('-').pop();
            var ord = $(btn).closest('tr').find('.option-order').val();
            var opt = $(btn).closest('tr').find('.option-txt').val();
            var iss = $(btn).closest('tr').find('.option-chk').prop('checked');
        if (confirm('Are you sure you want to update this option?')) {

            var url = hdnBaseUrl + "RiskAssessment/UpdateOption";
            var data = {
                QuestionOptionId: optionid,
                OptionText: opt,
                Issue: iss,
                Order: ord
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1)
                    UTILS.Alert.show($alert, 'success', 'Option updated successfully.')
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to update option.')
            }, function (err) {
                UTILS.Alert.show($alert, 'error', 'Failed to update option. Please try again later.')
            })
        }
    }
    //update question info
    $updateInfo.click(function (e) {
        e.preventDefault();
        if (confirm('Are you sure you want to update this question?')) {

            var url = hdnBaseUrl + "RiskAssessment/UpdateRAQuestion";
            var data = {
                QuestionId: queId,
                GroupId: $groupList.val(),
                Group: $("#ddlEditRAQGroup option:selected").text(),
                QuestionText: $question.val(),
                Order: $order.val()
            }
            UTILS.makeAjaxCall(url, data, function (res) {
                if (res.success == 1) {
                    UTILS.Alert.show($alert, 'success', 'Question updated successfully.')
                    editRiskAssessmentHandler.init();
                }
                else
                    UTILS.Alert.show($alert, 'error', 'Failed to update question.')
            }, function (err) {
                UTILS.Alert.show($alert, 'error', 'Failed to update question. Please try again later.')
            })
        }
    });
    

    return {
        showRAQModal: showRAQModal,
        deleteRAQuestionOption: deleteRAQuestionOption,
        updateRAQuestionOption: updateRAQuestionOption
    }
})()