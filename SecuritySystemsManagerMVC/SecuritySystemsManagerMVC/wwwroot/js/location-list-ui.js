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
    
    // Toggle map size - Fullscreen on mobile, expand on desktop
    $("#toggleMapSize").on("click", function() {
        var mapContainer = $("#locationsMap");
        var mapCard = mapContainer.closest(".map-card");
        var icon = $(this).find("i");
        var isMobile = window.innerWidth <= 768;
        
        if (isMobile) {
            // Mobile: Use fullscreen API
            if (!document.fullscreenElement && !document.mozFullScreenElement && 
                !document.webkitFullscreenElement && !document.msFullscreenElement) {
                // Enter fullscreen
                var element = mapCard[0];
                var promise;
                
                if (element.requestFullscreen) {
                    promise = element.requestFullscreen();
                } else if (element.mozRequestFullScreen) {
                    promise = element.mozRequestFullScreen();
                } else if (element.webkitRequestFullscreen) {
                    promise = element.webkitRequestFullscreen();
                } else if (element.msRequestFullscreen) {
                    promise = element.msRequestFullscreen();
                }
                
                icon.removeClass("bi-arrows-angle-expand").addClass("bi-arrows-angle-contract");
                $(this).html('<i class="bi bi-arrows-angle-contract"></i> Намали');
                
                // Wait for fullscreen to be entered, then resize map
                if (promise) {
                    promise.then(function() {
                        // Fullscreen entered successfully
                        setTimeout(function() {
                            if (window.map && typeof window.map.invalidateSize === 'function') {
                                window.map.invalidateSize(true); // Force resize
                            }
                            window.dispatchEvent(new Event('resize'));
                        }, 100);
                    }).catch(function(err) {
                        console.error('Error entering fullscreen:', err);
                    });
                } else {
                    // Fallback if promise is not supported
                    setTimeout(function() {
                        if (window.map && typeof window.map.invalidateSize === 'function') {
                            window.map.invalidateSize(true);
                        }
                        window.dispatchEvent(new Event('resize'));
                    }, 300);
                }
                
                // Update button when exiting fullscreen
                var exitFullscreenHandler = function() {
                    if (!document.fullscreenElement && !document.mozFullScreenElement && 
                        !document.webkitFullscreenElement && !document.msFullscreenElement) {
                        icon.removeClass("bi-arrows-angle-contract").addClass("bi-arrows-angle-expand");
                        $("#toggleMapSize").html('<i class="bi bi-arrows-angle-expand"></i> Разшири');
                        document.removeEventListener('fullscreenchange', exitFullscreenHandler);
                        document.removeEventListener('webkitfullscreenchange', exitFullscreenHandler);
                        document.removeEventListener('mozfullscreenchange', exitFullscreenHandler);
                        document.removeEventListener('MSFullscreenChange', exitFullscreenHandler);
                        
                        // Trigger map resize after exiting fullscreen
                        setTimeout(function() {
                            if (window.map && typeof window.map.invalidateSize === 'function') {
                                window.map.invalidateSize(true);
                            }
                            window.dispatchEvent(new Event('resize'));
                        }, 200);
                    }
                };
                
                document.addEventListener('fullscreenchange', exitFullscreenHandler);
                document.addEventListener('webkitfullscreenchange', exitFullscreenHandler);
                document.addEventListener('mozfullscreenchange', exitFullscreenHandler);
                document.addEventListener('MSFullscreenChange', exitFullscreenHandler);
            } else {
                // Exit fullscreen
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                } else if (document.mozCancelFullScreen) {
                    document.mozCancelFullScreen();
                } else if (document.webkitExitFullscreen) {
                    document.webkitExitFullscreen();
                } else if (document.msExitFullscreen) {
                    document.msExitFullscreen();
                }
            }
        } else {
            // Desktop: Toggle between normal and large size
            if (mapContainer.hasClass("map-container-lg")) {
                mapContainer.removeClass("map-container-lg");
                icon.removeClass("bi-arrows-angle-contract").addClass("bi-arrows-angle-expand");
                $(this).html('<i class="bi bi-arrows-angle-expand"></i> Разшири');
            } else {
                mapContainer.addClass("map-container-lg");
                icon.removeClass("bi-arrows-angle-expand").addClass("bi-arrows-angle-contract");
                $(this).html('<i class="bi bi-arrows-angle-contract"></i> Намали');
            }
        }
        
        // Trigger map resize event
        setTimeout(function() {
            if (window.map && typeof window.map.invalidateSize === 'function') {
                window.map.invalidateSize();
            }
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