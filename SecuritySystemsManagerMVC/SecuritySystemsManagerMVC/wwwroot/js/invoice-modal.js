document.addEventListener('DOMContentLoaded', function() {
    // Invoice Modal Elements
    const invoiceModal = document.getElementById('invoiceModal');
    const openInvoiceModalBtn = document.getElementById('openInvoiceModal');
    const closeInvoiceModalBtn = document.getElementById('closeInvoiceModal');
    const cancelInvoiceModalBtn = document.getElementById('cancelInvoiceModal');
    const invoiceModalOverlay = document.getElementById('invoiceModalOverlay');
    const generateInvoiceForm = document.getElementById('generateInvoiceForm');
    
    // Form Elements
    const laborCostInput = document.getElementById('laborCost');
    const deviceCostInputs = document.querySelectorAll('.device-cost');
    const totalAmountSpan = document.getElementById('totalAmount');
    
    // Check if invoice already exists and hide the generate button
    const invoiceSection = document.querySelector('.card-body');
    if (invoiceSection && invoiceSection.querySelector('.d-flex.justify-content-between.align-items-center.mb-4')) {
        // Invoice exists, hide the generate button
        if (openInvoiceModalBtn) {
            openInvoiceModalBtn.style.display = 'none';
        }
    }
    
    // Open Modal
    if (openInvoiceModalBtn) {
        openInvoiceModalBtn.addEventListener('click', function() {
            if (invoiceModal) {
                invoiceModal.style.display = 'block';
                document.body.style.overflow = 'hidden';
                
                // Focus on labor cost input
                if (laborCostInput) {
                    laborCostInput.focus();
                }
            }
        });
    }
    
    // Close Modal Functions
    function closeInvoiceModal() {
        if (invoiceModal) {
            invoiceModal.style.display = 'none';
            document.body.style.overflow = 'auto';
            
            // Reset form
            if (generateInvoiceForm) {
                generateInvoiceForm.reset();
            }
            
            // Reset totals
            updateTotalAmount();
        }
    }
    
    // Close Modal Event Listeners
    if (closeInvoiceModalBtn) {
        closeInvoiceModalBtn.addEventListener('click', closeInvoiceModal);
    }
    
    if (cancelInvoiceModalBtn) {
        cancelInvoiceModalBtn.addEventListener('click', closeInvoiceModal);
    }
    
    if (invoiceModalOverlay) {
        invoiceModalOverlay.addEventListener('click', closeInvoiceModal);
    }
    
    // Close modal on Escape key
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape' && invoiceModal && invoiceModal.style.display === 'block') {
            closeInvoiceModal();
        }
    });
    
    // Calculate device totals
    function calculateDeviceTotal(input) {
        const row = input.closest('tr');
        const quantity = parseInt(row.querySelector('td:nth-child(2)').textContent) || 0;
        const unitPrice = parseFloat(input.value) || 0;
        const total = quantity * unitPrice;
        
        const totalCell = row.querySelector('.device-total');
        if (totalCell) {
            totalCell.textContent = `$${total.toFixed(2)}`;
        }
        
        return total;
    }
    
    // Update total amount
    function updateTotalAmount() {
        let total = 0;
        
        // Add labor cost
        const laborCost = parseFloat(laborCostInput.value) || 0;
        total += laborCost;
        
        // Add device costs
        deviceCostInputs.forEach(input => {
            total += calculateDeviceTotal(input);
        });
        
        // Update total display
        if (totalAmountSpan) {
            totalAmountSpan.textContent = `$${total.toFixed(2)}`;
        }
    }
    
    // Event listeners for cost inputs
    if (laborCostInput) {
        laborCostInput.addEventListener('input', updateTotalAmount);
    }
    
    deviceCostInputs.forEach(input => {
        input.addEventListener('input', updateTotalAmount);
    });
    
    // Form submission
    if (generateInvoiceForm) {
        generateInvoiceForm.addEventListener('submit', function(event) {
            const laborCost = parseFloat(laborCostInput.value) || 0;
            
            if (laborCost <= 0) {
                event.preventDefault();
                alert('Please enter a valid labor cost greater than 0.');
                laborCostInput.focus();
                return;
            }
            
            // Show loading state
            const submitBtn = generateInvoiceForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Generating...';
            }
        });
    }
    
    // Initialize totals
    updateTotalAmount();
    
    // Check for success messages and handle them
    const successMessage = document.querySelector('.alert-success');
    if (successMessage && successMessage.textContent.includes('Invoice generated successfully')) {
        // Auto-refresh the page after a short delay to show the new invoice
        setTimeout(() => {
            window.location.reload();
        }, 1500);
    }
}); 