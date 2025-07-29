$(document).ready(function() {
    // Search functionality
    $("#locationSearch").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $("#locationsTable tbody tr").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
    
    // Tooltip initialization
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Toggle map size
    $("#toggleMapSize").on("click", function() {
        var mapContainer = $("#locationsMap");
        var icon = $(this).find("i");
        var buttonText = $(this).text().trim();
        
        if (mapContainer.hasClass("map-container-lg")) {
            mapContainer.removeClass("map-container-lg");
            icon.removeClass("bi-arrows-angle-contract").addClass("bi-arrows-angle-expand");
            $(this).html('<i class="bi bi-arrows-angle-expand"></i> Expand');
        } else {
            mapContainer.addClass("map-container-lg");
            icon.removeClass("bi-arrows-angle-expand").addClass("bi-arrows-angle-contract");
            $(this).html('<i class="bi bi-arrows-angle-contract"></i> Collapse');
        }
        
        // Trigger map resize event
        setTimeout(function() {
            window.dispatchEvent(new Event('resize'));
        }, 200);
    });
    
    // Highlight location on map when hovering over table row
    $(".location-row").on("mouseenter", function() {
        var locationId = $(this).data("location-id");
        if (window.highlightMarker) {
            window.highlightMarker(locationId);
        }
    }).on("mouseleave", function() {
        if (window.unhighlightMarkers) {
            window.unhighlightMarkers();
        }
    });
}); 