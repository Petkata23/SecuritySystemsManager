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
    var assignTechnicianModal = document.getElementById('assignTechnicianModal');
    if (assignTechnicianModal) {
        // Remove any existing event listeners to prevent duplicates
        const clonedModal = assignTechnicianModal.cloneNode(true);
        assignTechnicianModal.parentNode.replaceChild(clonedModal, assignTechnicianModal);
        assignTechnicianModal = clonedModal;

        // Create modal instance
        var modal = new bootstrap.Modal(assignTechnicianModal, {
            backdrop: 'static',
            keyboard: true,
            focus: true
        });
        
        // Get the button that opens the modal
        var assignTechnicianBtn = document.getElementById('assignTechnicianBtn');
        if (assignTechnicianBtn) {
            assignTechnicianBtn.addEventListener('click', function(event) {
                event.preventDefault();
                // Force cleanup before showing modal
                document.body.classList.remove('modal-open');
                removeExtraBackdrops();
                
                // Show modal after a short delay
                setTimeout(() => {
                    modal.show();
                }, 50);
            });
        }
        
        // Handle form submission
        var technicianForm = assignTechnicianModal.querySelector('form');
        if (technicianForm) {
            technicianForm.addEventListener('submit', function(event) {
                // Form will submit normally
                console.log('Form submitted');
            });
        }
        
        // Handle modal close
        var closeButtons = assignTechnicianModal.querySelectorAll('[data-bs-dismiss="modal"]');
        closeButtons.forEach(function(button) {
            button.addEventListener('click', function(event) {
                event.preventDefault();
                modal.hide();
            });
        });

        // Handle modal hidden event
        assignTechnicianModal.addEventListener('hidden.bs.modal', function() {
            document.body.classList.remove('modal-open');
            removeExtraBackdrops();
        });
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