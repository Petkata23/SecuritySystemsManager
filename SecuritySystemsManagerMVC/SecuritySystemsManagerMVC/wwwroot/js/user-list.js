$(document).ready(function() {
    $("#userSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        
        $("tbody tr").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
        
        // Always remove existing no-results message first
        $("#no-results-users").remove();
        
        // Show message if no results (count visible rows excluding the no-results message)
        const visibleRows = $("tbody tr:visible").not("#no-results-users").length;
        
        if (visibleRows === 0) {
            $("tbody").append(
                '<tr id="no-results-users"><td colspan="100%" class="text-center py-4">' +
                '<i class="bi bi-search text-muted mb-2" style="font-size: 2rem;"></i>' +
                '<p class="mb-0">No users match your search criteria</p>' +
                '</td></tr>'
            );
        }
    });
}); 