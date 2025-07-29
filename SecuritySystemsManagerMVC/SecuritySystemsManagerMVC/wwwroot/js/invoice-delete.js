$(document).ready(function() {
    // Confirmation on form submit
    $("form").submit(function(e) {
        if (!confirm("Are you absolutely sure you want to delete this invoice? This action cannot be undone.")) {
            e.preventDefault();
        }
    });
}); 