var raQuestionHandler = (function () {

    var $modal = $("#addRAQModal");
    var $title = $("#addRAQTitle");
    var $alert = $("#addRAQMessage");
    var $groupList = $("#ddladdRAQGroup");
    var $que = $("#txtRAQ");
    var $addQueBtn = $("#btnAddNewRAQuestion");
    var $optionsTable = $("#tableQuestionOptions");
    var $addOptionBtn = $("#btnAddOption");
    var $optionOrder = $("#txtOptionOrder")
    var $option = $("#txtOption");
    var $optionIssue = $("#chkOptionIssue");
    var $optionList = $("#questionOptionList");
    var optionCount = 0;
    var queId = 0

    function showAddQuePopUp() {
        $modal.modal({
            backdrop: 'static',
            keyboard: true
        });
    }

    $modal.on('show.bs.modal', function (e) {
        UTILS.Alert.hide($alert);
        $optionsTable.hide();
        renderGroupDropDown();
        $title.val('');
        $groupList.val('0');

    });

    //function to render list of all groups
    function renderGroupDropDown() {
        $groupList.empty();
        $groupList.append($('<option/>', { value: '0', text: 'Select' }));

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

    //add new question button click
    $addQueBtn.click(function (e) {
        e.preventDefault();
        if (valid()) {

            var url = hdnBaseUrl + "RiskAssessment/CreateRAQuestion";
            var question = {
                GroupId: $groupList.val(),
                Group: $("#ddladdRAQGroup option:selected").text(),
                Question: $que.val()
            }

            UTILS.makeAjaxCall(url, question, function (res) {
                if (res.success > 0) {
                    queId = res.success;
                    UTILS.Alert.show($alert, "success", "Question created successfully");
                    $optionsTable.show();
                    $addQueBtn.hide();
                    editRiskAssessmentHandler.init();
                }
                else{
                    UTILS.Alert.show($alert, "error", "Failed to create new question.")
                }
            }, function (err) {
                console.log(err);
                UTILS.Alert.show($alert, "error", "Failed to create new question. Please try again later.");
            })
        } else {
            return false;
        }
    })

    //add new option
    $addOptionBtn.click(function (e) {
        e.preventDefault();
        optionCount++;
        var order = $optionOrder.val();
        var option = $option.val();
        var issue = $optionIssue.prop('checked');

        var url = hdnBaseUrl + "RiskAssessment/CreateRAQuestionOption";
        var data = {
            QuestionId: queId,
            Option: option,
            Issue: issue,
            Order: order
        }

        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success > 0) {
                if (issue)
                    $('#questionOptionList').append("<tr><td>" + order + "</td><td>" + option + "</td><td><input type='checkbox' checked='" + issue + "' /></td></tr>");
                else
                    $('#questionOptionList').append("<tr><td>" + order + "</td><td>" + option + "</td><td><input type='checkbox' /></td></tr>");

                $optionOrder.val((optionCount + 1) * 10);
                $option.val('');
                $optionIssue.prop('checked', false);
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

    // validate function
    function valid() {
        if ($que.val() == null || $que.val().trim() == '') {
            UTILS.Alert.show($alert, "error", "Question field can't be empty.");
            return false;
        } else if ($groupList.val() == undefined || $groupList.val() == null || $groupList.val() == 0) {
            UTILS.Alert.show($alert, "error", "Please select a group for question.");
            return false;
        }
        else {
            return true;
        }
    }

    return {
        showAddQuePopUp: showAddQuePopUp
    }

})();