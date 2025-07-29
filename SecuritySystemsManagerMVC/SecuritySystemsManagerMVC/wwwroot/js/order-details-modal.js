$(document).ready(function() {
    // Custom modal functionality
    var $customModal = $('#customModal');
    var $customModalOverlay = $('#customModalOverlay');
    var $openCustomModalBtn = $('#openCustomModal');
    var $closeCustomModalBtn = $('#closeCustomModal');
    var $cancelCustomModalBtn = $('#cancelCustomModal');
    
    // Open modal
    $openCustomModalBtn.on('click', function() {
        $customModal.addClass('show');
    });
    
    // Close modal
    function closeCustomModal() {
        $customModal.removeClass('show');
    }
    
    $closeCustomModalBtn.on('click', closeCustomModal);
    $cancelCustomModalBtn.on('click', closeCustomModal);
    $customModalOverlay.on('click', closeCustomModal);
    
    // Close modal on ESC key
    $(document).on('keydown', function(e) {
        if (e.key === 'Escape' && $customModal.hasClass('show')) {
            closeCustomModal();
        }
    });
}); 