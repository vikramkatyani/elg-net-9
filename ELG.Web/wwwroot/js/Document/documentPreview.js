document.addEventListener('DOMContentLoaded', function () {
    const downloadButton = document.querySelector('#download-button');
    if (downloadButton) {
        downloadButton.addEventListener('click', function () {
            const documentUrl = document.querySelector("#hdnFileDwnldPath").value;

            // Check if the document has loaded
            if (documentUrl !== "" && documentUrl !== null) {
                // Create a hidden form element
                const form = document.createElement('form');
                form.action = documentUrl;
                form.method = 'GET';
                form.style.display = 'none';
                document.body.appendChild(form);

                // Submit the form to trigger the download
                form.submit();

                // Remove the form after the download is triggered
                form.remove();
            } else {
                // Handle case where document hasn't loaded yet
                alert('No document found.');
            }
        });
    }
});


// Set status for the document
function setDocStatus(btn) {
    const alertElement = document.querySelector("#message_doc_preview");
    const docId = btn.id.split('-').pop();
    const checkedRadio = document.querySelector('input[name="learnerDocStatus"]:checked');
    const status = checkedRadio ? checkedRadio.value : null;
    
    if (confirm('Are you sure you want to update the status?')) {
        const url = hdnBaseUrl + "SetDocumentStatus";
        const data = {
            DocumentId: docId,
            Status: status
        };
        UTILS.makeAjaxCall(url, data, function (res) {
            if (res.success === 1) {
                UTILS.Alert.showFloatingAlert(alertElement, 'success', 'Document status updated successfully.');
                window.opener.location.reload();
            } else {
                UTILS.Alert.showFloatingAlert(alertElement, 'error', 'Failed to update document status.');
            }
        }, function (err) {
            UTILS.Alert.showFloatingAlert(alertElement, 'error', 'Failed to update document status. Please try again later.');
        });
    }
}
