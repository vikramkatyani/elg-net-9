document.addEventListener('DOMContentLoaded', function () {
    // Download button: routes through server-side DownloadDocument proxy so private Azure Blob containers work
    const downloadButton = document.querySelector('#download-button');
    const mobileDownloadButton = document.querySelector('#download-button-mobile');
    const openPdfButton = document.querySelector('#open-pdf-button');
    const iframe = document.querySelector('#pdf-inline-frame');
    const mobileFallback = document.querySelector('#mobile-pdf-fallback');
    const hiddenDownloadUrl = (document.querySelector('#hdnFileDwnldPath') || {}).value;
    const hiddenDocId = (document.querySelector('#hdnDocId') || {}).value;
    const dataDownloadUrl = (downloadButton && downloadButton.getAttribute('data-download-url')) || '';
    const dataMobileDownloadUrl = (mobileDownloadButton && mobileDownloadButton.getAttribute('data-download-url')) || '';
    const resolvedDownloadUrl =
        (typeof documentDownloadUrl !== 'undefined' && documentDownloadUrl) ||
        dataMobileDownloadUrl ||
        dataDownloadUrl ||
        hiddenDownloadUrl ||
        (hiddenDocId ? ('/Learner/Document/DownloadDocument?id=' + encodeURIComponent(hiddenDocId)) : '');
    const resolvedStreamUrl =
        (typeof documentStreamUrl !== 'undefined' && documentStreamUrl)
            ? documentStreamUrl
            : (hiddenDocId ? ('/Learner/Document/StreamDocument?id=' + encodeURIComponent(hiddenDocId)) : '');

    const isMobileDevice = /Android|iPhone|iPad|iPod|Windows Phone|Mobile/i.test(navigator.userAgent || '');

    if (isMobileDevice && iframe && mobileFallback) {
        iframe.style.display = 'none';
        const wrapper = document.querySelector('#pdf-viewer-wrapper');
        if (wrapper) {
            wrapper.style.display = 'none';
        }
        mobileFallback.style.display = 'block';
    }

    if (downloadButton) {
        downloadButton.addEventListener('click', function () {
            if (resolvedDownloadUrl) {
                // Use server-side download proxy — works regardless of blob container access level
                window.location.href = resolvedDownloadUrl;
            } else {
                alert('Document download endpoint is unavailable. Please refresh the page and try again.');
            }
        });
    }

    if (mobileDownloadButton) {
        mobileDownloadButton.addEventListener('click', function () {
            if (resolvedDownloadUrl) {
                window.location.href = resolvedDownloadUrl;
            } else {
                alert('Document download endpoint is unavailable. Please refresh the page and try again.');
            }
        });
    }

    if (openPdfButton) {
        openPdfButton.addEventListener('click', function () {
            if (resolvedStreamUrl) {
                window.open(resolvedStreamUrl, '_blank');
            } else if (resolvedDownloadUrl) {
                window.open(resolvedDownloadUrl, '_blank');
            } else {
                alert('Document preview endpoint is unavailable. Please refresh the page and try again.');
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
    const explicitSetStatusUrl = btn.getAttribute('data-setstatus-url') || '';
    const setStatusUrl =
        explicitSetStatusUrl ||
        ((typeof documentSetStatusUrl !== 'undefined' && documentSetStatusUrl)
            ? documentSetStatusUrl
            : '/Learner/Document/SetDocumentStatus');
    
    if (confirm('Are you sure you want to update the status?')) {
        const data = {
            DocumentId: docId,
            Status: status
        };
        UTILS.makeAjaxCall(setStatusUrl, data, function (res) {
            if (res.success === 1) {
                // Show success message
                UTILS.Alert.show(alertElement, 'success', 'Document status updated successfully.');
                
                // Disable all radio buttons
                const radioButtons = document.querySelectorAll('input[name="learnerDocStatus"]');
                radioButtons.forEach(radio => {
                    radio.disabled = true;
                });
                
                // Disable update button and change appearance
                const updateBtn = document.querySelector(`#btn-update-status-${docId}`);
                if (updateBtn) {
                    updateBtn.disabled = true;
                    updateBtn.style.cursor = 'not-allowed';
                    updateBtn.style.opacity = '0.6';
                }
                
                // Refresh parent window if it exists
                if (window.opener && !window.opener.closed) {
                    try {
                        window.opener.location.reload();
                    } catch (e) {
                        console.log('Could not reload parent window');
                    }
                }
            } else {
                UTILS.Alert.show(alertElement, 'error', 'Failed to update document status.');
            }
        }, function (err) {
            UTILS.Alert.show(alertElement, 'error', 'Failed to update document status. Please try again later.');
        });
    }
}
