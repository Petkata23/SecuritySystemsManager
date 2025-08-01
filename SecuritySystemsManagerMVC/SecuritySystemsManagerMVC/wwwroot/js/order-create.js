$(document).ready(function() {
    // Initialize map
    let map;
    let marker;
    
    function initMap() {
        // Default center (Sofia, Bulgaria)
        const center = { lat: 42.6977, lng: 23.3219 };
        
        // Initialize Leaflet map
        map = L.map('map', {
            zoomControl: false,
            attributionControl: false
        }).setView([center.lat, center.lng], 13);
        
        // Add OpenStreetMap tile layer
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);
        
        // Add zoom control
        L.control.zoom({
            position: 'topright'
        }).addTo(map);
        
        // Add scale control
        L.control.scale({
            imperial: false,
            position: 'bottomleft',
            maxWidth: 200
        }).addTo(map);
        
        // Add attribution
        L.control.attribution({
            position: 'bottomright'
        }).addTo(map);
        
        // Add click event to map
        map.on('click', function(event) {
            placeMarker(event.latlng);
            reverseGeocode(event.latlng);
        });
        
        // Try to get user's location
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                function(position) {
                    const pos = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude
                    };
                    map.setView([pos.lat, pos.lng], 15);
                    placeMarker(pos);
                    reverseGeocode(pos);
                },
                function() {
                    // Handle location error - use default center
                    placeMarker(center);
                }
            );
        } else {
            // Fallback to default center
            placeMarker(center);
        }
    }
    
    function placeMarker(location) {
        if (marker) {
            marker.setLatLng(location);
        } else {
            // Create custom security icon with better visibility
            var securityIcon = L.divIcon({
                className: 'custom-security-marker',
                html: '<div class="marker-pin"><i class="bi bi-geo-alt-fill"></i></div>',
                iconSize: [56, 56],
                iconAnchor: [28, 56],
                popupAnchor: [0, -56]
            });
            
            marker = L.marker(location, {
                icon: securityIcon,
                draggable: true
            }).addTo(map);
            
            // Add drag event to marker
            marker.on('dragend', function(event) {
                const newLocation = event.target.getLatLng();
                updateCoordinates(newLocation);
                reverseGeocode(newLocation);
            });
            
            // Add popup to marker
            marker.bindPopup('<div class="text-center"><strong>📍 Security Location</strong><br>Click and drag to move</div>').openPopup();
        }
        
        updateCoordinates(location);
    }
    
    function updateCoordinates(location) {
        $("#locationLatitude").val(location.lat.toFixed(6));
        $("#locationLongitude").val(location.lng.toFixed(6));
    }
    
    function reverseGeocode(location) {
        // Show loading state for address
        $("#locationAddress").val("Loading address...").prop('disabled', true);
        
        // Try multiple geocoding services
        tryGeocoding(location, 0);
    }
    
    function tryGeocoding(location, attempt) {
        const services = [
            {
                name: 'Nominatim',
                url: `https://nominatim.openstreetmap.org/reverse?format=json&lat=${location.lat}&lon=${location.lng}&zoom=18&addressdetails=1`,
                headers: {
                    'Accept': 'application/json',
                    'User-Agent': 'SecuritySystemsManager/1.0'
                }
            },
            {
                name: 'Nominatim Simple',
                url: `https://nominatim.openstreetmap.org/reverse?format=json&lat=${location.lat}&lon=${location.lng}&zoom=10`,
                headers: {
                    'Accept': 'application/json',
                    'User-Agent': 'SecuritySystemsManager/1.0'
                }
            }
        ];
        
        if (attempt >= services.length) {
            // All services failed, use coordinates as fallback
            const lat = location.lat.toFixed(6);
            const lng = location.lng.toFixed(6);
            $("#locationAddress").val(`Location at ${lat}, ${lng}`);
            $("#locationAddress").prop('disabled', false);
            return;
        }
        
        const service = services[attempt];
        
        $.ajax({
            url: service.url,
            type: 'GET',
            dataType: 'json',
            headers: service.headers,
            timeout: 8000,
            success: function(data) {
                if (data && data.display_name) {
                    // Extract the most relevant part of the address
                    let address = data.display_name;
                    
                    // Try to get a shorter, more relevant address
                    if (data.address) {
                        const parts = [];
                        if (data.address.road) parts.push(data.address.road);
                        if (data.address.house_number) parts.push(data.address.house_number);
                        if (data.address.city) parts.push(data.address.city);
                        else if (data.address.town) parts.push(data.address.town);
                        else if (data.address.village) parts.push(data.address.village);
                        else if (data.address.county) parts.push(data.address.county);
                        
                        if (parts.length > 0) {
                            address = parts.join(', ');
                        }
                    }
                    
                    $("#locationAddress").val(address);
                    $("#locationAddress").prop('disabled', false);
                } else {
                    // Try next service
                    tryGeocoding(location, attempt + 1);
                }
            },
            error: function(xhr, status, error) {
                console.error(`${service.name} geocoding error:`, status, error);
                // Try next service
                tryGeocoding(location, attempt + 1);
            }
        });
    }
    
    // Load Leaflet when modal is shown
    $('#createLocationModal').on('shown.bs.modal', function () {
        if (!map) {
            // Check if Leaflet is already loaded
            if (typeof L === 'undefined') {
                // Load Leaflet CSS and JS
                if (!document.querySelector('link[href*="leaflet.css"]')) {
                    const leafletCSS = document.createElement('link');
                    leafletCSS.rel = 'stylesheet';
                    leafletCSS.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
                    document.head.appendChild(leafletCSS);
                }
                
                // Load Leaflet JS
                $.getScript('https://unpkg.com/leaflet@1.9.4/dist/leaflet.js', function() {
                    initMap();
                });
            } else {
                initMap();
            }
        }
        
        // Trigger resize to fix map display
        setTimeout(() => {
            if (map) {
                map.invalidateSize();
            }
        }, 100);
    });
    
    // Reset form when modal is hidden
    $('#createLocationModal').on('hidden.bs.modal', function () {
        $("#locationForm")[0].reset();
        if (marker) {
            map.removeLayer(marker);
            marker = null;
        }
    });
    
    // Save location
    $("#saveLocationBtn").on("click", function() {
        const $btn = $(this);
        const originalText = $btn.html();
        
        // Validate required fields
        if (!$("#locationName").val().trim()) {
            showAlert("Please enter a location name", "danger");
            $("#locationName").focus();
            return;
        }
        
        if (!$("#locationAddress").val().trim() || $("#locationAddress").val() === "Loading address..." || $("#locationAddress").val() === "Could not load address") {
            showAlert("Please wait for the address to load or enter it manually", "warning");
            $("#locationAddress").focus();
            return;
        }
        
        if (!$("#locationLatitude").val() || !$("#locationLongitude").val()) {
            showAlert("Please select a location on the map", "danger");
            return;
        }
        
        // Show loading state
        $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Saving...');
        
        // Create location via AJAX
        $.ajax({
            url: '/Location/CreateLocationAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Name: $("#locationName").val().trim(),
                Address: $("#locationAddress").val().trim(),
                Latitude: $("#locationLatitude").val(),
                Longitude: $("#locationLongitude").val(),
                Description: $("#locationDescription").val().trim()
            }),
            success: function(response) {
                if (response.success) {
                    // Add new option to select
                    var newOption = new Option(response.locationName, response.locationId, true, true);
                    $("#LocationId").append(newOption).trigger('change');
                    
                    // Show success message
                    showAlert("Location created successfully!", "success");
                    
                    // Close modal after short delay
                    setTimeout(() => {
                        $('#createLocationModal').modal('hide');
                    }, 1000);
                } else {
                    showAlert("Error: " + response.message, "danger");
                }
            },
            error: function(xhr, status, error) {
                showAlert("An error occurred while creating the location. Please try again.", "danger");
                console.error("AJAX Error:", xhr.responseText);
            },
            complete: function() {
                // Reset button state
                $btn.prop('disabled', false).html(originalText);
            }
        });
    });
    
    function showAlert(message, type) {
        // Remove existing alerts
        $('.alert').remove();
        
        // Create new alert
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'exclamation-triangle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        // Insert alert at the top of the modal body
        $('#createLocationModal .modal-body .row .col-md-5 .p-4').prepend(alertHtml);
        
        // Auto-dismiss success alerts after 3 seconds
        if (type === 'success') {
            setTimeout(() => {
                $('.alert-success').fadeOut();
            }, 3000);
        }
    }
}); 