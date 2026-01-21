$(function () {
    UTILS.activateNavigationLink('resourcesLink');
    UTILS.activateMenuNavigationLink('menu-resources');
    $('[data-toggle="tooltip"]').tooltip();
    $('#btnDownloadBusinessCertificate').click(function () {
        var url = hdnBaseUrl + "Certificate/GetBusinessCertificate";
        window.open(url, '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes');
    });
})
