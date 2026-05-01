$(function () {
    UTILS.activateNavigationLink('profileLink');
    UTILS.activateMenuNavigationLink('menu-allocate-profile');
    $('[data-toggle="tooltip"]').tooltip();
    $('#table_container').hide();
    $('#info_container').show();
});

$("#ddlProfiles").on('change', function () {
    var val = $(this).val();
    if (val > 0) {
        $('#table_container').show();
        $('#info_container').hide();
    }
    else {
        $('#table_container').hide();
        $('#info_container').show();
    }
})