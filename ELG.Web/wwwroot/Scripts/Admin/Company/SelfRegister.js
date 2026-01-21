$(function () {
    UTILS.activateNavigationLink('userLink');
    UTILS.activateMenuNavigationLink('menu-self-register');
    $('#divNoSelfRegistration').hide();
    $('#divSelfRegistrationSteps').hide();
    $('#hdn_txt_copy_area').hide()

    var url = hdnBaseUrl + "UserManagement/LoadSelfRegisterData";
    UTILS.makeAjaxCall(url, {}, function (res) { 
        if (res.selfregister == 1) {
            $('#divSelfRegistrationSteps').show();
        }
        else {
            $('#divNoSelfRegistration').show();
        }
    },
    function (err) {
        console.log(err);
    });

});



    function CopyTextToClipBoard() {
        /* Get the text field */
        $('#hdn_txt_copy_area').show()
        var copyText = document.getElementById("hdn_txt_copy_area");

        copyText.focus();
        copyText.select();

        /* Alert the copied text */
        try {
            var successful = document.execCommand('copy');
            var msg = successful ? 'successful' : 'unsuccessful';
            alert("Text copied to clipboard");
            console.log('Copying text command was ' + msg);
        } catch (err) {
            console.log('Oops, unable to copy');
            console.log(err);
        }
        $('#hdn_txt_copy_area').hide()
    } 