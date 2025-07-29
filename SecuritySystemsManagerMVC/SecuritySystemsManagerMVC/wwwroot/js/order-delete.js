$(document).ready(function() {
    // Enable delete button only when checkbox is checked
    $('#confirmDelete').on('change', function() {
        $('#deleteBtn').prop('disabled', !this.checked);
    });
    
    // Add confirmation dialog
    $('form').on('submit', function(e) {
        if (!confirm('Are you absolutely sure you want to delete this order? This action cannot be undone.')) {
            e.preventDefault();
        }
    });
}); 