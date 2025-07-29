$(document).ready(function() {
    // Search functionality
    $("#invoiceSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $(".invoice-list-item").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
}); 