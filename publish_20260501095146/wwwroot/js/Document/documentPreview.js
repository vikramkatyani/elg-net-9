document.addEventListener('DOMContentLoaded', function () {
    // Download button: routes through server-side DownloadDocument proxy so private Azure Blob containers work
    const downloadButton = document.querySelector('#download-button');
    const hiddenDownloadUrl = (document.querySelector('#hdnFileDwnldPath') || {}).value;
    const hiddenDocId = (document.querySelector('#hdnDocId') || {}).value;
    const dataDownloadUrl = (downloadButton && downloadButton.getAttribute('data-download-url')) || '';
    const resolvedDownloadUrl =
        (typeof documentDownloadUrl !== 'undefined' && documentDownloadUrl) ||
        dataDownloadUrl ||
        hiddenDownloadUrl ||
        (hiddenDocId ? ('/Learner/Document/DownloadDocument?id=' + encodeURIComponent(hiddenDocId)) : '');

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

    // PDF.js inline rendering — works on all devices including iOS/Android mobile.
    // An <iframe> cannot display PDFs on mobile browsers, so PDF.js renders to <canvas> instead.
    // The PDF is proxied through the server (StreamDocument action) to avoid Azure Blob CORS restrictions.
    if (typeof pdfjsLib !== 'undefined' && typeof documentStreamUrl !== 'undefined' && documentStreamUrl) {
        pdfjsLib.GlobalWorkerOptions.workerSrc =
            'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
        initPdfViewer(documentStreamUrl);
    }
});


// ─── PDF.js viewer ─────────────────────────────────────────────────────────────

var _pdfDoc = null;          // cached PDFJS document object
var _renderPending = false;  // prevents overlapping re-renders on resize

function initPdfViewer(url) {
    var loadingEl = document.getElementById('pdf-loading');
    var container = document.getElementById('pdf-pages-container');
    if (!container) return;

    pdfjsLib.getDocument(url).promise.then(function (pdf) {
        _pdfDoc = pdf;
        if (loadingEl) loadingEl.style.display = 'none';
        renderAllPages(pdf);
    }, function (reason) {
        if (loadingEl) {
            loadingEl.textContent = 'Failed to load document. Please use the Download button below.';
            loadingEl.style.color = '#f8d7da';
        }
        console.error('PDF.js load error:', reason);
    });

    // Re-render pages on orientation change / window resize (important for mobile)
    var resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (_pdfDoc && !_renderPending) {
                var c = document.getElementById('pdf-pages-container');
                if (c) c.innerHTML = '';
                renderAllPages(_pdfDoc);
            }
        }, 300);
    });
}

function renderAllPages(pdf) {
    _renderPending = true;
    var queue = [];
    for (var i = 1; i <= pdf.numPages; i++) queue.push(i);

    function renderNext() {
        if (queue.length === 0) { _renderPending = false; return; }
        renderPage(pdf, queue.shift(), renderNext);
    }
    renderNext();
}

function renderPage(pdf, pageNum, callback) {
    pdf.getPage(pageNum).then(function (page) {
        var container = document.getElementById('pdf-pages-container');
        var wrapper   = document.getElementById('pdf-viewer-wrapper');
        if (!container || !wrapper) { if (callback) callback(); return; }

        // Scale the page to fill the wrapper width, minus a small margin
        var availWidth = (wrapper.clientWidth > 0 ? wrapper.clientWidth : window.innerWidth) - 20;
        var baseViewport = page.getViewport({ scale: 1 });
        var scale    = availWidth / baseViewport.width;
        var viewport = page.getViewport({ scale: scale });

        var canvas       = document.createElement('canvas');
        canvas.width     = viewport.width;
        canvas.height    = viewport.height;
        canvas.style.display    = 'block';
        canvas.style.margin     = '6px auto';
        canvas.style.maxWidth   = '100%';
        canvas.style.boxShadow  = '0 2px 8px rgba(0,0,0,0.5)';

        container.appendChild(canvas);

        page.render({ canvasContext: canvas.getContext('2d'), viewport: viewport })
            .promise.then(function () { if (callback) callback(); });
    });
}

// ───────────────────────────────────────────────────────────────────────────────

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
