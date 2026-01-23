// Location List Mobile View Handler
$(document).ready(function() {
    // Check if mobile device and auto-switch to card view
    function checkMobileView() {
        if (window.innerWidth <= 768) {
            // Mobile: show card view, hide table view
            $("#locationsTableView").addClass('d-none');
            $("#locationsCardView").removeClass('d-none');
        } else {
            // Desktop: show table view, hide card view
            $("#locationsTableView").removeClass('d-none');
            $("#locationsCardView").addClass('d-none');
        }
    }
    
    // Check on page load
    checkMobileView();
    
    // Check on window resize
    $(window).on('resize', function() {
        checkMobileView();
    });
    
    // Search functionality for both views
    $("#locationSearch").on("keyup", function() {
        const searchTerm = $(this).val().toLowerCase();
        
        // Filter table view
        $("#locationsTable tbody tr").each(function() {
            const locationName = $(this).find("h6").text().toLowerCase();
            const locationAddress = $(this).find("td").eq(1).text().toLowerCase();
            const locationId = $(this).find("small").text().toLowerCase();
            
            if (locationName.includes(searchTerm) || locationAddress.includes(searchTerm) || locationId.includes(searchTerm)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
        
        // Filter card view
        $(".location-card-item").each(function() {
            const locationName = $(this).find(".location-card-title h6").text().toLowerCase();
            const locationAddress = $(this).find(".location-card-address span").text().toLowerCase();
            const locationId = $(this).find(".location-card-title small").text().toLowerCase();
            
            if (locationName.includes(searchTerm) || locationAddress.includes(searchTerm) || locationId.includes(searchTerm)) {
                $(this).closest(".col-12").show();
            } else {
                $(this).closest(".col-12").hide();
            }
        });
    });
});
