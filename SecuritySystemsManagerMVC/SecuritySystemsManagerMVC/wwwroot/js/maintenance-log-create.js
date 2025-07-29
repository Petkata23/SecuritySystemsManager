$(document).ready(function() {
    // Set current date as default
    const today = new Date().toISOString().split('T')[0];
    $("#Date").val(today);
    
    // Form validation styling enhancement
    $("form").on("submit", function() {
        $(this).find(".form-control, .form-select").each(function() {
            if ($(this).hasClass("input-validation-error")) {
                $(this).parent().addClass("has-error");
            }
        });
    });
}); 