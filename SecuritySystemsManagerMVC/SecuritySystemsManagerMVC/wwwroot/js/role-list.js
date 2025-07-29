$(document).ready(function() {
    // Search functionality
    $("#roleSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $("#rolesTable tbody tr").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
    
    // Tooltip initialization
    $('[data-bs-toggle="tooltip"]').tooltip();
}); 