// MaintenanceDevice JavaScript functionality

$(document).ready(function() {
    // Initialize animations
    initializeAnimations();
    
    // Initialize form validations
    initializeFormValidations();
    
    // Initialize device status toggle
    initializeStatusToggle();
    
    // Initialize device filters
    initializeDeviceFilters();
});

// Initialize animations for device cards and items
function initializeAnimations() {
    // Add animation delay to each card
    $(".animate-fade-in").each(function(index) {
        $(this).css("animation-delay", (index * 0.1) + "s");
    });
    
    // Add hover effects to device cards
    $(".device-card").hover(
        function() {
            $(this).addClass("shadow-lg");
        },
        function() {
            $(this).removeClass("shadow-lg");
        }
    );
}

// Initialize form validations for device forms
function initializeFormValidations() {
    // Enhance select dropdowns
    $('select').each(function() {
        if ($(this).val()) {
            $(this).addClass('is-valid');
        }
    });
    
    $('select').on('change', function() {
        if ($(this).val()) {
            $(this).addClass('is-valid');
        } else {
            $(this).removeClass('is-valid');
        }
    });
    
    // Enhance text inputs
    $('input[type="text"], textarea').on('blur', function() {
        if ($(this).val().trim().length > 0) {
            $(this).addClass('is-valid');
        } else {
            $(this).removeClass('is-valid');
        }
    });
    
    // Add confirmation for delete forms
    $('form[action*="Delete"]').on('submit', function(e) {
        if (!confirm('Are you sure you want to delete this maintenance device record? This action cannot be undone.')) {
            e.preventDefault();
        }
    });
}

// Initialize device status toggle functionality
function initializeStatusToggle() {
    // Toggle fixed status with animation
    $('.toggle-fixed-btn').on('click', function(e) {
        e.preventDefault();
        
        const $btn = $(this);
        const $form = $btn.closest('form');
        
        $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i> Updating...');
        
        // Submit the form via AJAX
        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            success: function(response) {
                // Update UI based on new status
                const isFixed = $btn.data('is-fixed') === 'True';
                const newStatus = !isFixed;
                
                // Update button
                $btn.data('is-fixed', newStatus ? 'True' : 'False');
                
                if (newStatus) {
                    $btn.removeClass('btn-outline-success').addClass('btn-outline-warning');
                    $btn.html('<i class="bi bi-x-circle me-2"></i> Mark as Not Fixed');
                } else {
                    $btn.removeClass('btn-outline-warning').addClass('btn-outline-success');
                    $btn.html('<i class="bi bi-check-circle me-2"></i> Mark as Fixed');
                }
                
                // Show success message
                showToast('Status updated successfully', 'success');
                
                // Enable button
                $btn.prop('disabled', false);
            },
            error: function() {
                // Show error message
                showToast('Failed to update status. Please try again.', 'danger');
                
                // Enable button
                $btn.prop('disabled', false);
            }
        });
    });
}

// Initialize device filters functionality
function initializeDeviceFilters() {
    // Filter devices by status
    $('#filterDevices').on('change', function() {
        const filterValue = $(this).val();
        
        if (filterValue === 'all') {
            $('.device-card').show();
        } else if (filterValue === 'fixed') {
            $('.device-card').hide();
            $('.device-card .status-fixed').closest('.device-card').show();
        } else if (filterValue === 'not-fixed') {
            $('.device-card').hide();
            $('.device-card .status-not-fixed').closest('.device-card').show();
        }
    });
    
    // Search devices by name
    $('#searchDevices').on('keyup', function() {
        const searchValue = $(this).val().toLowerCase();
        
        $('.device-card').each(function() {
            const deviceName = $(this).find('.device-info-item strong').text().toLowerCase();
            
            if (deviceName.includes(searchValue)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
}

// Helper function to show toast notifications
function showToast(message, type) {
    // Create toast container if it doesn't exist
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="position-fixed bottom-0 end-0 p-3" style="z-index: 5"></div>');
    }
    
    // Create toast element
    const toastId = 'toast-' + Date.now();
    const toast = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    // Append toast to container
    $('#toast-container').append(toast);
    
    // Initialize and show toast
    const toastElement = new bootstrap.Toast(document.getElementById(toastId), {
        delay: 3000
    });
    toastElement.show();
    
    // Remove toast element after it's hidden
    $(`#${toastId}`).on('hidden.bs.toast', function() {
        $(this).remove();
    });
} 