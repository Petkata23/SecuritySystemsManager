document.addEventListener("DOMContentLoaded", function () {
    // Check if map container exists
    var mapElement = document.getElementById('createLocationMap');
    if (!mapElement) {
        console.error("❌ Error: Map container with id='createLocationMap' not found.");
        return;
    }

    // Get form input elements
    const latitudeInput = document.getElementById('latitudeInput');
    const longitudeInput = document.getElementById('longitudeInput');
    const addressInput = document.getElementById('addressInput');

    // Default coordinates (center of Bulgaria)
    const defaultLat = 42.6977;
    const defaultLng = 23.3219;

    // Initialize map with colorful theme
    var map = L.map('createLocationMap', {
        zoomControl: false,
        attributionControl: false
    }).setView([defaultLat, defaultLng], 7);

    // Add colorful map theme
    L.tileLayer('https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Tiles style by <a href="https://www.hotosm.org/" target="_blank">HOT</a>',
        maxZoom: 19
    }).addTo(map);
    
    // Add attribution in bottom right
    L.control.attribution({
        position: 'bottomright'
    }).addTo(map);
    
    // Add zoom control in top right
    L.control.zoom({
        position: 'topright'
    }).addTo(map);

    // Add scale control
    L.control.scale({
        imperial: false,
        position: 'bottomleft',
        maxWidth: 200
    }).addTo(map);

    // Custom security icon
    var securityIcon = L.divIcon({
        className: 'custom-security-marker',
        html: '<div class="marker-pin"><i class="bi bi-shield-lock-fill"></i></div>',
        iconSize: [36, 36],
        iconAnchor: [18, 36],
        popupAnchor: [0, -36]
    });

    // Add custom CSS for the marker
    const style = document.createElement('style');
    style.textContent = `
        .custom-security-marker {
            background: transparent;
            border: none;
        }
        .marker-pin {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            background: #0d6efd;
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 3px 8px rgba(0,0,0,0.3);
            border: 2px solid white;
            animation: bounce 0.5s ease infinite alternate;
        }
        @keyframes bounce {
            from {
                transform: translateY(0);
            }
            to {
                transform: translateY(-5px);
            }
        }
        .marker-pin i {
            color: white;
            font-size: 16px;
            animation: pulse 1.5s ease infinite;
        }
        @keyframes pulse {
            0% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.2);
            }
            100% {
                transform: scale(1);
            }
        }
        .leaflet-control-geocoder {
            box-shadow: 0 3px 8px rgba(0,0,0,0.2) !important;
            border-radius: 4px !important;
            overflow: hidden;
        }
        .leaflet-control-geocoder-form input {
            padding: 8px 12px !important;
            font-size: 14px !important;
            border: none !important;
            width: 100% !important;
        }
        .leaflet-control-geocoder-alternatives {
            background: white !important;
            border-top: 1px solid #eee !important;
        }
        .leaflet-control-geocoder-alternatives li {
            padding: 8px 12px !important;
        }
        .leaflet-control-geocoder-alternatives li:hover {
            background: #f5f5f5 !important;
        }
    `;
    document.head.appendChild(style);

    // Add geocoder control
    const geocoder = L.Control.geocoder({
        defaultMarkGeocode: false,
        position: 'topright',
        placeholder: 'Search address...',
        errorMessage: 'Address not found',
        showResultIcons: true
    }).addTo(map);

    // Current marker
    var marker = null;

    // Function to update marker
    function updateMarker(lat, lng) {
        // Remove existing marker if any
        if (marker) {
            map.removeLayer(marker);
        }

        // Create new marker
        marker = L.marker([lat, lng], {
            icon: securityIcon,
            draggable: true,
            title: 'Drag to adjust location',
            alt: 'Location marker'
        }).addTo(map);

        // Update form inputs
        latitudeInput.value = lat.toFixed(6);
        longitudeInput.value = lng.toFixed(6);

        // Handle marker drag
        marker.on('dragend', function (e) {
            const position = marker.getLatLng();
            latitudeInput.value = position.lat.toFixed(6);
            longitudeInput.value = position.lng.toFixed(6);

            // Reverse geocode to get address
            reverseGeocode(position.lat, position.lng);
        });
    }

    // Function to reverse geocode
    function reverseGeocode(lat, lng) {
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`)
            .then(response => response.json())
            .then(data => {
                if (data && data.display_name) {
                    addressInput.value = data.display_name;
                }
            })
            .catch(error => {
                console.error('Error during reverse geocoding:', error);
            });
    }

    // Handle map click
    map.on('click', function (e) {
        updateMarker(e.latlng.lat, e.latlng.lng);
        reverseGeocode(e.latlng.lat, e.latlng.lng);
    });

    // Handle geocoder results
    geocoder.on('markgeocode', function (e) {
        const center = e.geocode.center;
        updateMarker(center.lat, center.lng);
        
        if (e.geocode.name) {
            addressInput.value = e.geocode.name;
        }
        
        map.fitBounds(e.geocode.bbox);
    });

    // Handle manual coordinate input
    latitudeInput.addEventListener('change', updateFromInputs);
    longitudeInput.addEventListener('change', updateFromInputs);

    function updateFromInputs() {
        const lat = parseFloat(latitudeInput.value);
        const lng = parseFloat(longitudeInput.value);

        if (!isNaN(lat) && !isNaN(lng) && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
            updateMarker(lat, lng);
            map.setView([lat, lng], 15);
        }
    }

    // Initialize with default values if provided
    if (latitudeInput.value && longitudeInput.value) {
        const lat = parseFloat(latitudeInput.value);
        const lng = parseFloat(longitudeInput.value);
        
        if (!isNaN(lat) && !isNaN(lng)) {
            updateMarker(lat, lng);
            map.setView([lat, lng], 15);
        }
    }

    // Force map to recalculate its size
    setTimeout(() => {
        map.invalidateSize();
    }, 500);
}); 