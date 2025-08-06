$(document).ready(function() {
    // Search functionality
    $("#roleSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $("#rolesTable tbody tr").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
        
        // Always remove existing no-results message first
        $("#no-results-roles").remove();
        
        // Show message if no results (count visible rows excluding the no-results message)
        const visibleRows = $("#rolesTable tbody tr:visible").not("#no-results-roles").length;
        
        if (visibleRows === 0) {
            $("#rolesTable tbody").append(
                '<tr id="no-results-roles"><td colspan="100%" class="text-center py-4">' +
                '<i class="bi bi-search text-muted mb-2" style="font-size: 2rem;"></i>' +
                '<p class="mb-0">No roles match your search criteria</p>' +
                '</td></tr>'
            );
        }
    });
    
    // Tooltip initialization
    $('[data-bs-toggle="tooltip"]').tooltip();
}); 