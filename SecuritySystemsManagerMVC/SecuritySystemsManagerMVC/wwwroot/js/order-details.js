/**
 * Order Details Page JavaScript
 * Handles functionality specific to the order details page
 */

document.addEventListener('DOMContentLoaded', function() {
    // Fix for multiple modal backdrops
    const removeExtraBackdrops = () => {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        if (backdrops.length > 1) {
            for (let i = 1; i < backdrops.length; i++) {
                backdrops[i].remove();
            }
        }
    };

    // Initialize the assign technician modal
    var customModal = document.getElementById('customModal');
    if (customModal) {
        // Get the button that opens the modal
        var openCustomModalBtn = document.getElementById('openCustomModal');
        if (openCustomModalBtn) {
            openCustomModalBtn.addEventListener('click', function(event) {
                event.preventDefault();
                customModal.style.display = 'flex';
                document.body.style.overflow = 'hidden';
            });
        }
        
        // Handle modal close
        var closeCustomModalBtn = document.getElementById('closeCustomModal');
        if (closeCustomModalBtn) {
            closeCustomModalBtn.addEventListener('click', function(event) {
                event.preventDefault();
                customModal.style.display = 'none';
                document.body.style.overflow = 'auto';
            });
        }
        
        // Handle cancel button
        var cancelCustomModalBtn = document.getElementById('cancelCustomModal');
        if (cancelCustomModalBtn) {
            cancelCustomModalBtn.addEventListener('click', function(event) {
                event.preventDefault();
                customModal.style.display = 'none';
                document.body.style.overflow = 'auto';
            });
        }
        
        // Handle overlay click to close modal
        var customModalOverlay = document.getElementById('customModalOverlay');
        if (customModalOverlay) {
            customModalOverlay.addEventListener('click', function(event) {
                if (event.target === customModalOverlay) {
                    customModal.style.display = 'none';
                    document.body.style.overflow = 'auto';
                }
            });
        }
        
        // Handle form submission
        var technicianForm = customModal.querySelector('form');
        if (technicianForm) {
            technicianForm.addEventListener('submit', function(event) {
                // Form will submit normally
                console.log('Form submitted');
            });
        }
    }
    
    // Handle technician removal confirmation
    var removeTechnicianForms = document.querySelectorAll('form[action*="RemoveTechnician"]');
    removeTechnicianForms.forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!confirm('Are you sure you want to remove this technician from the order?')) {
                event.preventDefault();
            }
        });
    });

    // Clean up any stray modal-open classes or backdrops on page load
    document.body.classList.remove('modal-open');
    removeExtraBackdrops();
}); 