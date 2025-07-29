$(document).ready(function() {
    // Set default date to today
    if (!$("#IssuedOn").val()) {
        $("#IssuedOn").val(new Date().toISOString().split('T')[0]);
    }

    // Auto-populate amount when order is selected
    $("#SecuritySystemOrderId").change(function() {
        // In a real application, you would fetch the order amount via AJAX
        // For now, we'll just show an example placeholder
        if ($(this).val()) {
            // This would be replaced with actual AJAX call in production
            // $.get('/api/orders/' + $(this).val(), function(data) {
            //     $("#TotalAmount").val(data.amount);
            // });
        }
    });
}); 