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
    const toggleMapLayersBtn = document.getElementById('toggleMapLayersBtn');

    // Default coordinates (center of Bulgaria)
    const defaultLat = 42.6977;
    const defaultLng = 23.3219;

    // Initialize map with detailed view
    var map = L.map('createLocationMap', {
        zoomControl: false,
        attributionControl: false
    }).setView([defaultLat, defaultLng], 13);

    // Map layers are now handled by the layer control
    
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

    // Add custom CSS for the marker and map controls
    const style = document.createElement('style');
    style.textContent = `
        .custom-security-marker {
            background: transparent;
            border: none;
        }
        .marker-pin {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            background: #0d6efd;
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 3px 8px rgba(0,0,0,0.3);
            border: 3px solid white;
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
            font-size: 18px;
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
        .leaflet-control-layers {
            border-radius: 4px !important;
            box-shadow: 0 3px 8px rgba(0,0,0,0.2) !important;
        }
        .leaflet-control-layers-expanded {
            padding: 10px 12px !important;
            background-color: rgba(255, 255, 255, 0.9) !important;
            border-radius: 4px !important;
        }
        .leaflet-control-layers-selector {
            margin-right: 5px !important;
        }
        .leaflet-control-layers-separator {
            margin: 8px 0 !important;
        }
        #createLocationMap {
            border: 1px solid rgba(0,0,0,0.1);
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
            transition: all 0.3s ease;
        }
        #createLocationMap:hover {
            box-shadow: 0 8px 25px rgba(0,0,0,0.15);
        }
        .leaflet-control-fullscreen a, 
        .leaflet-control-locate a {
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
            width: 30px !important;
            height: 30px !important;
            background-color: white !important;
            color: #333 !important;
            transition: all 0.2s ease !important;
        }
        .leaflet-control-fullscreen a:hover, 
        .leaflet-control-locate a:hover {
            background-color: #f4f4f4 !important;
            color: #0d6efd !important;
        }
    `;
    document.head.appendChild(style);

    // Add layer controls for different map views
    const baseMaps = {
        "Standard": L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }),
        "Detailed": L.tileLayer('https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }),
        "Transport": L.tileLayer('https://{s}.tile.thunderforest.com/transport/{z}/{x}/{y}.png?apikey=6170aad10dfd42a38d4d8c709a536f38', {
            attribution: '&copy; <a href="http://www.thunderforest.com/">Thunderforest</a>, &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }),
        "Satellite": L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
            maxZoom: 19
        })
    };
    
    // Create layer control but don't add it to the map yet
    const layerControl = L.control.layers(baseMaps, null, {
        position: 'bottomright',
        collapsed: false
    });
    
    // Select the detailed map by default
    baseMaps["Detailed"].addTo(map);

    // Toggle map layers control
    let layersVisible = false;
    
    if (toggleMapLayersBtn) {
        toggleMapLayersBtn.addEventListener('click', function() {
            if (layersVisible) {
                map.removeControl(layerControl);
                toggleMapLayersBtn.innerHTML = '<i class="bi bi-layers me-1"></i> Show Map Layers';
            } else {
                layerControl.addTo(map);
                toggleMapLayersBtn.innerHTML = '<i class="bi bi-layers-fill me-1"></i> Hide Map Layers';
            }
            layersVisible = !layersVisible;
        });
    }
    
    // Add geocoder control with enhanced features
    const geocoder = L.Control.geocoder({
        defaultMarkGeocode: false,
        position: 'topright',
        placeholder: 'Search address...',
        errorMessage: 'Address not found',
        showResultIcons: true,
        suggestMinLength: 3,
        suggestTimeout: 250,
        queryMinLength: 3
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
    
    // Add fullscreen control
    const fullscreenButton = L.control({position: 'topright'});
    fullscreenButton.onAdd = function() {
        const div = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        div.innerHTML = '<a class="leaflet-control-fullscreen" href="#" title="View Fullscreen" style="font-size: 22px; line-height: 30px;"><i class="bi bi-arrows-fullscreen"></i></a>';
        
        div.onclick = function() {
            const mapElement = document.getElementById('createLocationMap');
            if (!document.fullscreenElement) {
                if (mapElement.requestFullscreen) {
                    mapElement.requestFullscreen();
                } else if (mapElement.mozRequestFullScreen) {
                    mapElement.mozRequestFullScreen();
                } else if (mapElement.webkitRequestFullscreen) {
                    mapElement.webkitRequestFullscreen();
                } else if (mapElement.msRequestFullscreen) {
                    mapElement.msRequestFullscreen();
                }
                setTimeout(() => map.invalidateSize(), 200);
            } else {
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                } else if (document.mozCancelFullScreen) {
                    document.mozCancelFullScreen();
                } else if (document.webkitExitFullscreen) {
                    document.webkitExitFullscreen();
                } else if (document.msExitFullscreen) {
                    document.msExitFullscreen();
                }
                setTimeout(() => map.invalidateSize(), 200);
            }
            return false;
        };
        
        return div;
    };
    fullscreenButton.addTo(map);
    
    // Add locate control to find user's location
    const locateControl = L.control({position: 'topright'});
    locateControl.onAdd = function() {
        const div = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        div.innerHTML = '<a class="leaflet-control-locate" href="#" title="Find my location" style="font-size: 18px; line-height: 30px;"><i class="bi bi-geo-alt-fill"></i></a>';
        
        div.onclick = function() {
            map.locate({setView: true, maxZoom: 16});
            return false;
        };
        
        return div;
    };
    locateControl.addTo(map);
    
    // Handle location found event
    map.on('locationfound', function(e) {
        updateMarker(e.latlng.lat, e.latlng.lng);
        reverseGeocode(e.latlng.lat, e.latlng.lng);
        
        // Show accuracy circle
        const radius = e.accuracy / 2;
        L.circle(e.latlng, {
            radius: radius,
            color: '#4a80f5',
            fillColor: '#4a80f5',
            fillOpacity: 0.15,
            weight: 2,
            dashArray: '5,5'
        }).addTo(map);
    });
    
    // Handle location error
    map.on('locationerror', function(e) {
        alert("Could not find your location: " + e.message);
    });
}); 