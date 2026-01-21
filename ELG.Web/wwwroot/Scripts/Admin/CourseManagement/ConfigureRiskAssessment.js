$(function () {
    UTILS.activateNavigationLink('manageRALink');
    UTILS.activateMenuNavigationLink('menu-configure-ra');
    $('[data-toggle="tooltip"]').tooltip();
});

var configureRAFrequencyHandler = (function () {

    var $course = $('#txtRiskAssessment')
    var $searchBtn = $('#searchRiskAssessmentBtn')
    var $clearSearchBtn = $('#clearSearchRABtn')

    var $updateFrequencyModal = $('#updateRiskAssessmentFrequencyModal');
    var $updateFreqTitle = $('#updateRAModalLabel');
    var $freqBox = $('#txtUpdateRAFrequency');
    var $completionDaysBox = $('#txtUpdateRACompletionDays');
    var $updateFreqBtn = $('#btnUpdateRAFrequency');
    var $updateFreqMessage = $('#updateRAMessage');

    var raId = 0;

    var configRATable = $('#configureRAList').DataTable({
        lengthChange: false,
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "orderMulti": true,
        "ajax": {
            "url": hdnBaseUrl + 'RiskAssessment/LoadRAModuleList',
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $course.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "ModuleName", "name": "c.strCourse", "autoWidth": true }
        ],
        columnDefs: [{
            // render completion days
            targets: [1], render: function (a, b, data, d) {
                if (data["CompletionDays"] > 0)
                    return '<span>' + data["CompletionDays"] + ' days</span>';
                else
                    return '<span>Never</span>';
            }
        },{
            // render reset frequency
            targets: [2], render: function (a, b, data, d) {
                if (data["RAFrequency"] > 0)
                    return '<span>' + data["RAFrequency"] + ' days</span>';
                else
                    return '<span>Never</span>';
            }
        }, {
            // render action buttons in the last column
            targets: [3], render: function (a, b, data, d) {
                return '<button type="button" id="update-freq-' + data["ModuleID"] + '" class="btn btn-sm btn-dark mb-1" onclick="configureRAFrequencyHandler.showUpdateFreqPopUP(this)"><i class="fa fa-fw fa-wrench"></i><span>Configure</span></button> '
            }
        }],
    });

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        configRATable.draw();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $course.val('');
        configRATable.draw();
    });

    //function to open modal for update score
    function showUpdateFreqPopUP(btn) {
        UTILS.Alert.hide($updateFreqMessage)

        raId = btn.id.split('-').pop();
        var moduleName = configRATable.row(btn.closest('tr')).data()["ModuleName"];
        var cmpDays = configRATable.row(btn.closest('tr')).data()["CompletionDays"];
        var freq = configRATable.row(btn.closest('tr')).data()["RAFrequency"];
        $updateFreqTitle.html('Update - ' + moduleName);
        $freqBox.val(freq);
        $completionDaysBox.val(cmpDays);

        $updateFrequencyModal.modal('show');
    }

    //function to update frequency of Risk Assessment

    $updateFreqBtn.click(function () { updateRAFrequency(raId) });

    function updateRAFrequency(raId) {
        if (confirm("Are you sure you want to update frequency?")) {
            var url = hdnBaseUrl + 'RiskAssessment/UpdateFrequency';
            var freq = $freqBox.val();
            var cmpDays = $completionDaysBox.val();
            var data = { ModuleID: raId, RAFrequency: freq, CompletionDays: cmpDays }
            UTILS.makeAjaxCall(url, data, function (result) {
                if (result.success == 1) {
                    UTILS.Alert.show($updateFreqMessage, 'success', 'Frequency updated successfully');
                    configRATable.draw();
                }
                else
                    UTILS.Alert.show($updateFreqMessage, 'error', 'Failed to update frequency');
            }, function (status) {
                console.log(status);
                UTILS.Alert.show($updateFreqMessage, 'success', 'Failed to update frequency, Please try again later');
            });
        }
    }
    return {
        showUpdateFreqPopUP: showUpdateFreqPopUP,
        updateRAFrequency: updateRAFrequency
    }
})();