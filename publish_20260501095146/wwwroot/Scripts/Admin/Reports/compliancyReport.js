$(function () {
    $('#txtAdvCompRptFrom').datepicker({ dateFormat: 'yy-mm-dd' });
    $('#txtAdvCompRptTo').datepicker({ dateFormat: 'yy-mm-dd' });
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-adv-report');
    compliancyReportHandler.init();
    $('[data-toggle="tooltip"]').tooltip();

    $('.adv-com-rep tbody').scroll(function (e) { //detect a scroll event on the tbody
  	/*
    Setting the thead left value to the negative valule of tbody.scrollLeft will make it track the movement
    of the tbody element. Setting an elements left value to that of the tbody.scrollLeft left makes it maintain 			it's relative position at the left of the table.    
    */
        $('.adv-com-rep thead').css("left", -$(".adv-com-rep tbody").scrollLeft()); //fix the thead relative to the body scrolling
        $('.adv-com-rep thead th:nth-child(1)').css("left", $(".adv-com-rep tbody").scrollLeft()); //fix the first cell of the header
        $('.adv-com-rep tbody td:nth-child(1)').css("left", $(".adv-com-rep tbody").scrollLeft()); //fix the first column of tdbody
    });

})

var $loader = '<div class="text-center mb-4"> <i class="fa fa-spinner fa-spin fa-3x fa-fw" ></i> <span class="sr-only">Loading...</span> </div >'

var compliancyReportHandler = (function () {

    var $ddlLoc = $('#ddlLocation')
    var $txtRptFrom = $('#txtAdvCompRptFrom')
    var $txtRptTo = $('#txtAdvCompRptTo')
    var $searchBtn = $('#searchCompliancy');
    var $clearSearchBtn = $('#clearSearchCompliancy');
    //var $downloadBtn = $('#downloadAnnouncementReport');

    // function to initialise report page
    // bind drop down in search area
    function init() {
        renderLocationDropDown();
        showReport();
    }

    //function to render list of all locations in organisation
    function renderLocationDropDown() {
        $ddlLoc.empty();
        $ddlLoc.append($('<option/>', { value: '0', text: 'Select All' }));

        UTILS.data.getAllLocations(function (data) {
            if (data && data.locationList != null) {
                $.each(data.locationList, function (index, item) {
                    $ddlLoc.append($('<option/>', {
                        value: item.LocationId,
                        text: item.LocationName
                    }))
                });
            }
        })
    }

    //apply filters for search
    $searchBtn.click(function (e) {
        e.preventDefault();
        $('#divCompliancyReportHolder').html('');
        $('#divCompliancyReportHolder').html($loader);
        compliancyReportHandler.showReport();
    });

    //clear search filters
    $clearSearchBtn.click(function (e) {
        e.preventDefault();
        $ddlLoc.val('0');
        $txtRptFrom.val('');
        $txtRptTo.val('');
        compliancyReportHandler.showReport();
    });

    function showReport() {
        var url = hdnBaseUrl + "Report/AdvanceCompliancyReport"
        var data = {
            Location: $ddlLoc.val(),
            FromDate: $txtRptFrom.val(),
            ToDate: $txtRptTo.val()
        }
        UTILS.makeAjaxCall(url, data, function (e) {
            $('#divCompliancyReportHolder').html('');
            $('#divCompliancyReportHolder').html(e);
        }, function (err) {
            console.log(err);
        })
    }

    return {
        init: init,
        showReport: showReport
    }
})();