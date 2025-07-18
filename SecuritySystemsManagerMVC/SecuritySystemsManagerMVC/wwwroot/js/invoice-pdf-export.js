// Invoice PDF Export functionality
let isProcessing = false;
let originalBodyOverflow = '';
let originalElementsStyles = new Map(); // Store original styles for various elements

// Helper to store and apply/revert styles
function storeAndApplyStyle(element, property, value) {
    if (element) {
        if (!originalElementsStyles.has(element)) {
            originalElementsStyles.set(element, new Map());
        }
        originalElementsStyles.get(element).set(property, element.style[property]);
        element.style[property] = value;
    }
}

function revertStyle(element, property) {
    if (element && originalElementsStyles.has(element)) {
        const styles = originalElementsStyles.get(element);
        if (styles.has(property)) {
            element.style[property] = styles.get(property);
        } else {
            element.style[property] = ''; // If it wasn't set inline, clear it
        }
    }
}

function showLoading() {
    hideLoading(); // Remove any existing overlay

    const overlay = document.createElement('div');
    overlay.id = 'pdf-loading-overlay';
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.7);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    `;

    overlay.innerHTML = `
        <div class="text-center text-white">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <div class="mt-3">Generating PDF...</div>
            <div class="mt-2 small">This may take a few moments</div>
        </div>
    `;

    document.body.appendChild(overlay);
}

function hideLoading() {
    const overlay = document.getElementById('pdf-loading-overlay');
    if (overlay) {
        overlay.remove();
    }
}

function showMessage(message, type) {
    const existingMessages = document.querySelectorAll('.pdf-export-message');
    existingMessages.forEach(msg => msg.remove());

    const toast = document.createElement('div');
    toast.className = 'pdf-export-message';
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 10000;
        min-width: 300px;
    `;

    toast.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        if (toast.parentNode) {
            toast.remove();
        }
    }, 5000);
}

function applyPrintStyles() {
    const invoiceContainer = document.querySelector('.invoice-container');
    const invoiceActions = document.querySelector('.invoice-actions');
    const paymentActionsCard = document.querySelector('.mt-4 .card');
    const invoiceIdParagraph = document.querySelector('.d-flex.justify-content-between.align-items-center.mb-4 > div:first-child p');
    const pageHeader = document.querySelector('.d-flex.justify-content-between.align-items-center.mb-4');

    // Store original body overflow and apply
    originalBodyOverflow = document.body.style.overflow;
    storeAndApplyStyle(document.body, 'overflow', 'hidden');
    storeAndApplyStyle(document.body, 'backgroundColor', '#ffffff'); // Ensure white background for the PDF

    // Hide elements not needed in the PDF
    if (invoiceActions) {
        storeAndApplyStyle(invoiceActions, 'display', 'none');
    }
    if (paymentActionsCard) {
        storeAndApplyStyle(paymentActionsCard, 'display', 'none');
    }
    if (invoiceIdParagraph) {
        storeAndApplyStyle(invoiceIdParagraph, 'display', 'none');
    }
    if (pageHeader) {
        storeAndApplyStyle(pageHeader, 'display', 'none');
    }

    // Invoice Container adjustments for A4 page
    if (invoiceContainer) {
        storeAndApplyStyle(invoiceContainer, 'boxShadow', 'none');
        storeAndApplyStyle(invoiceContainer, 'border', 'none');
        // Important: Remove padding from the container itself to control total width accurately
        storeAndApplyStyle(invoiceContainer, 'padding', '0');
        storeAndApplyStyle(invoiceContainer, 'width', '210mm'); // A4 width
        storeAndApplyStyle(invoiceContainer, 'maxWidth', '210mm');
        storeAndApplyStyle(invoiceContainer, 'margin', '0 auto'); // Center the invoice on the A4 page
    }

    // Apply padding to main sections *inside* the invoice container to create margins
    // A4 dimensions: 210mm x 297mm
    // Let's aim for 10mm margins on all sides within the PDF content
    const horizontalPadding = '10mm'; // Total 20mm horizontal margin
    const verticalPadding = '10mm'; // Total 20mm vertical margin (for top/bottom sections)

    const invoiceHeader = document.querySelector('.invoice-header');
    const invoiceBody = document.querySelector('.invoice-body');
    const invoiceFooter = document.querySelector('.invoice-footer');

    if (invoiceHeader) storeAndApplyStyle(invoiceHeader, 'padding', `${verticalPadding} ${horizontalPadding}`);
    if (invoiceBody) storeAndApplyStyle(invoiceBody, 'padding', `0 ${horizontalPadding}`); // No top/bottom padding for body, relies on header/footer
    if (invoiceFooter) storeAndApplyStyle(invoiceFooter, 'padding', `${verticalPadding} ${horizontalPadding}`);

    // Add table borders dynamically if not already present or for better print visibility
    const invoiceTable = document.querySelector('.invoice-table');
    if (invoiceTable) {
        storeAndApplyStyle(invoiceTable, 'borderCollapse', 'collapse');
        const cells = invoiceTable.querySelectorAll('th, td');
        cells.forEach(cell => {
            const currentBorder = window.getComputedStyle(cell).border;
            if (!currentBorder || currentBorder === 'none' || currentBorder === '0px') {
                storeAndApplyStyle(cell, 'border', '1px solid #dee2e6');
            }
            storeAndApplyStyle(cell, 'padding', '8px');
            storeAndApplyStyle(cell, 'textAlign', 'left');
        });
        const tableHeaderCells = invoiceTable.querySelectorAll('thead th');
        tableHeaderCells.forEach(th => {
            storeAndApplyStyle(th, 'webkitPrintColorAdjust', 'exact');
            storeAndApplyStyle(th, 'colorAdjust', 'exact');
        });
    }

    // Ensure status background colors are preserved 
    const paidStatus = document.querySelector('.invoice-status.status-paid');
    const unpaidStatus = document.querySelector('.invoice-status.status-unpaid');
    const billFromToBoxes = document.querySelectorAll('.invoice-body .col-md-6 > div');

    if (paidStatus) {
        storeAndApplyStyle(paidStatus, 'webkitPrintColorAdjust', 'exact');
        storeAndApplyStyle(paidStatus, 'colorAdjust', 'exact');
    }
    if (unpaidStatus) {
        storeAndApplyStyle(unpaidStatus, 'webkitPrintColorAdjust', 'exact');
        storeAndApplyStyle(unpaidStatus, 'colorAdjust', 'exact');
    }
    billFromToBoxes.forEach(box => {
        storeAndApplyStyle(box, 'webkitPrintColorAdjust', 'exact');
        storeAndApplyStyle(box, 'colorAdjust', 'exact');
    });

    // Logo adjustment
    const invoiceLogo = document.querySelector('.invoice-logo');
    if (invoiceLogo) {
        storeAndApplyStyle(invoiceLogo, 'maxWidth', '80px');
        storeAndApplyStyle(invoiceLogo, 'height', 'auto');
    }
}

function revertPrintStyles() {
    console.log('Reverting print styles...');
    
    // Revert body styles
    document.body.style.overflow = originalBodyOverflow;
    revertStyle(document.body, 'backgroundColor');
    revertStyle(document.body, 'overflow');

    // Revert all stored styles
    originalElementsStyles.forEach((stylesMap, element) => {
        stylesMap.forEach((originalValue, property) => {
            if (originalValue === '') {
                element.style.removeProperty(property);
            } else {
                element.style[property] = originalValue;
            }
        });
    });
    originalElementsStyles.clear();
    
    console.log('Print styles reverted successfully');
}

function exportInvoiceToPdf() {
    console.log('Export PDF function called');
    
    if (isProcessing) {
        console.log('PDF generation already in progress...');
        return;
    }
    
    // Check if libraries are available
    if (typeof window.jspdf === 'undefined' || typeof window.jspdf.jsPDF === 'undefined') {
        showMessage('PDF library (jsPDF) not loaded. Please refresh the page and try again.', 'danger');
        return;
    }
    
    if (typeof window.html2canvas === 'undefined') {
        showMessage('Canvas library (html2canvas) not loaded. Please refresh the page and try again.', 'danger');
        return;
    }
    
    isProcessing = true;
    showLoading();
    
    console.log('Starting PDF export process...');
    
    try {
        const invoiceContainer = document.querySelector('.invoice-container');
        if (!invoiceContainer) {
            throw new Error('Invoice container not found');
        }

        console.log('Applying temporary print styles...');
        applyPrintStyles(); // Apply styles before capturing

        console.log('Invoice container found, generating canvas...');

        // Use a slight delay to ensure styles are applied and rendered by the browser
        setTimeout(() => {
            html2canvas(invoiceContainer, {
                scale: 2, // Increased scale for better resolution, affects capture quality
                useCORS: true,
                allowTaint: true,
                backgroundColor: '#ffffff', // Ensures the canvas itself has a white background
                logging: false
            }).then(function (canvas) {
                console.log('Canvas generated, creating PDF...');

                // Get invoice details for filename
                const invoiceNumberElement = document.querySelector('.invoice-meta-item:nth-child(1) p');
                const invoiceDateElement = document.querySelector('.invoice-meta-item:nth-child(2) p');

                const invoiceNumber = invoiceNumberElement ? invoiceNumberElement.textContent.trim().replace('#', '') : 'invoice';
                const invoiceDate = invoiceDateElement ? invoiceDateElement.textContent.trim().replace(/\//g, '-') : '';

                const { jsPDF } = window.jspdf;
                const pdf = new jsPDF('p', 'mm', 'a4');

                // A4 dimensions in mm (210mm x 297mm)
                const a4Width = 210;
                const a4Height = 297;

                // Calculate aspect ratio of the captured canvas
                const canvasAspectRatio = canvas.width / canvas.height;

                // Calculate image dimensions to fit within A4, maintaining aspect ratio
                let imgWidth = a4Width;
                let imgHeight = a4Width / canvasAspectRatio;

                // If calculated height is greater than A4 height, scale down by height
                if (imgHeight > a4Height) {
                    imgHeight = a4Height;
                    imgWidth = a4Height * canvasAspectRatio;
                }

                // Center the image on the A4 page
                const xOffset = (a4Width - imgWidth) / 2;
                const yOffset = (a4Height - imgHeight) / 2;

                console.log('Adding image to PDF...');

                const imgData = canvas.toDataURL('image/png');

                // Add the image to the PDF, centered and scaled to fit
                pdf.addImage(imgData, 'PNG', xOffset, yOffset, imgWidth, imgHeight);

                // Note: This current logic assumes the entire invoice fits on one page after scaling.
                // If the invoice is genuinely multi-page (e.g., very long table), a more complex slicing
                // and multi-page addition logic would be required. For typical invoices, fitting to one page is usually enough.

                const filename = `Invoice_${invoiceNumber}_${invoiceDate}.pdf`;

                console.log('Saving PDF...');

                pdf.save(filename);

                console.log('PDF export completed successfully');

                // Always revert styles and clean up
                revertPrintStyles();
                hideLoading();
                showMessage('Invoice exported successfully!', 'success');
                isProcessing = false;

            }).catch(function (error) {
                console.error('Canvas generation error:', error);
                // Always revert styles and clean up on error
                revertPrintStyles();
                hideLoading();
                showMessage('Failed to generate PDF. Please try again.', 'danger');
                isProcessing = false;
            });
        }, 100); // Small delay

    } catch (error) {
        console.error('PDF export error:', error);
        // Always revert styles and clean up on error
        revertPrintStyles();
        hideLoading();
        showMessage(`Failed to export PDF: ${error.message}`, 'danger');
        isProcessing = false;
    }
}