$(function () {
    UTILS.activateNavigationLink('helpLink');
    UTILS.activateMenuNavigationLink('menu-contact');
    $('[data-toggle="tooltip"]').tooltip();
    UTILS.makeAjaxCall(hdnBaseUrl + "/common/GetCompanyTrainerDetails", {}, function (res) {
        $('#spnCompanyName').html(res.trainer.Company);
        $('#spnTrainer').html(res.trainer.Trainer);
        $('#spnTrainerEmail').html(res.trainer.Email);
        $('#spnTrainerPhone').html(res.trainer.Phone);
    })

    $("#btnSupportTicket").click(function () {
        var left = (screen.width / 2) - (900 / 2);
        var top = (screen.height / 2) - (500 / 2);
        return window.open("https://elearningate.com/hc/en-gb/requests/new", "ELG_Ticket", 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=900, height=500, top=' + top + ', left=' + left);
    })
})