$(document).ready(function() {
    // Initialize map
    let map;
    let marker;
    
    function initMap() {
        // Default center (can be a default location in Bulgaria)
        const center = { lat: 42.6977, lng: 23.3219 }; // Sofia coordinates
        
        map = new google.maps.Map(document.getElementById("map"), {
            zoom: 12,
            center: center,
            styles: [
                { elementType: "geometry", stylers: [{ color: "#242f3e" }] },
                { elementType: "labels.text.stroke", stylers: [{ color: "#242f3e" }] },
                { elementType: "labels.text.fill", stylers: [{ color: "#746855" }] },
                {
                    featureType: "administrative.locality",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#d59563" }],
                },
                {
                    featureType: "poi",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#d59563" }],
                },
                {
                    featureType: "poi.park",
                    elementType: "geometry",
                    stylers: [{ color: "#263c3f" }],
                },
                {
                    featureType: "poi.park",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#6b9a76" }],
                },
                {
                    featureType: "road",
                    elementType: "geometry",
                    stylers: [{ color: "#38414e" }],
                },
                {
                    featureType: "road",
                    elementType: "geometry.stroke",
                    stylers: [{ color: "#212a37" }],
                },
                {
                    featureType: "road",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#9ca5b3" }],
                },
                {
                    featureType: "road.highway",
                    elementType: "geometry",
                    stylers: [{ color: "#746855" }],
                },
                {
                    featureType: "road.highway",
                    elementType: "geometry.stroke",
                    stylers: [{ color: "#1f2835" }],
                },
                {
                    featureType: "road.highway",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#f3d19c" }],
                },
                {
                    featureType: "transit",
                    elementType: "geometry",
                    stylers: [{ color: "#2f3948" }],
                },
                {
                    featureType: "transit.station",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#d59563" }],
                },
                {
                    featureType: "water",
                    elementType: "geometry",
                    stylers: [{ color: "#17263c" }],
                },
                {
                    featureType: "water",
                    elementType: "labels.text.fill",
                    stylers: [{ color: "#515c6d" }],
                },
                {
                    featureType: "water",
                    elementType: "labels.text.stroke",
                    stylers: [{ color: "#17263c" }],
                },
            ]
        });
        
        // Add click event to map
        map.addListener("click", (event) => {
            placeMarker(event.latLng);
        });
        
        // Try to get user's location
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const pos = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude,
                    };
                    map.setCenter(pos);
                    placeMarker(pos);
                },
                () => {
                    // Handle location error
                }
            );
        }
    }
    
    function placeMarker(location) {
        if (marker) {
            marker.setPosition(location);
        } else {
            marker = new google.maps.Marker({
                position: location,
                map: map,
                draggable: true
            });
            
            // Add drag event to marker
            marker.addListener('dragend', function() {
                updateCoordinates(marker.getPosition());
            });
        }
        
        updateCoordinates(location);
    }
    
    function updateCoordinates(location) {
        $("#locationLatitude").val(location.lat().toFixed(6));
        $("#locationLongitude").val(location.lng().toFixed(6));
    }
    
    // Load Google Maps API when modal is shown
    $('#createLocationModal').on('shown.bs.modal', function () {
        if (!map) {
            $.getScript("https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&callback=initMap");
            window.initMap = initMap;
        }
    });
    
    // Save location
    $("#saveLocationBtn").on("click", function() {
        if (!$("#locationName").val() || !$("#locationAddress").val() || !$("#locationLatitude").val() || !$("#locationLongitude").val()) {
            alert("Please fill in all required fields");
            return;
        }
        
        // Create location via AJAX
        $.ajax({
            url: '/Location/CreateLocationAjax',
            type: 'POST',
            data: {
                Name: $("#locationName").val(),
                Address: $("#locationAddress").val(),
                Latitude: $("#locationLatitude").val(),
                Longitude: $("#locationLongitude").val(),
                Description: $("#locationDescription").val()
            },
            success: function(response) {
                if (response.success) {
                    // Add new option to select
                    var newOption = new Option(response.locationName, response.locationId, true, true);
                    $("#LocationId").append(newOption).trigger('change');
                    
                    // Close modal and reset form
                    $('#createLocationModal').modal('hide');
                    $("#locationForm")[0].reset();
                } else {
                    alert("Error: " + response.message);
                }
            },
            error: function() {
                alert("An error occurred while creating the location");
            }
        });
    });
}); 