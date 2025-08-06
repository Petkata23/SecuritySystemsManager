$(document).ready(function() {
    // Search functionality
    $("#invoiceSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $(".invoice-list-item").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
        
        // Always remove existing no-results message first
        $("#no-results-invoices").remove();
        
        // Show message if no results (count visible items excluding the no-results message)
        const visibleItems = $(".invoice-list-item:visible").not("#no-results-invoices").length;
        
        if (visibleItems === 0) {
            $(".invoice-list").append(
                '<div id="no-results-invoices" class="col-12 text-center py-5">' +
                '<i class="bi bi-search text-muted mb-2" style="font-size: 2rem;"></i>' +
                '<p class="mb-0">No invoices match your search criteria</p>' +
                '</div>'
            );
        }
    });
}); 