$(document).ready(function() {
    // Update badge when resolved checkbox changes
    $("#Resolved").on("change", function() {
        if ($(this).is(":checked")) {
            $("#resolvedBadge")
                .removeClass("status-pending")
                .addClass("status-resolved")
                .text("Resolved");
        } else {
            $("#resolvedBadge")
                .removeClass("status-resolved")
                .addClass("status-pending")
                .text("Pending");
        }
    });
    
    // Format date for input
    const dateInput = $("#Date");
    if (dateInput.val()) {
        const date = new Date(dateInput.val());
        const formattedDate = date.toISOString().split('T')[0];
        dateInput.val(formattedDate);
    }
    
    // Form validation styling enhancement
    $("form").on("submit", function() {
        $(this).find(".form-control, .form-select").each(function() {
            if ($(this).hasClass("input-validation-error")) {
                $(this).parent().addClass("has-error");
            }
        });
    });
}); 