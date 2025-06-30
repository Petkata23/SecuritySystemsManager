document.addEventListener("DOMContentLoaded", function () {
    // Check if map container exists
    var mapElement = document.getElementById('deleteLocationMap');
    if (!mapElement) {
        console.error("❌ Error: Map container with id='deleteLocationMap' not found.");
        return;
    }

    // Custom security icon for delete view
    var deleteIcon = L.divIcon({
        className: 'custom-delete-marker',
        html: '<div class="marker-pin"><i class="bi bi-shield-lock-fill"></i></div>',
        iconSize: [36, 36],
        iconAnchor: [18, 36],
        popupAnchor: [0, -36]
    });

    // Add custom CSS for the marker
    const style = document.createElement('style');
    style.textContent = `
        .custom-delete-marker {
            background: transparent;
            border: none;
        }
        .marker-pin {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            background: #dc3545;
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 3px 8px rgba(0,0,0,0.3);
            border: 2px solid white;
            animation: pulse 1.5s ease infinite;
        }
        @keyframes pulse {
            0% {
                transform: scale(1);
                opacity: 1;
            }
            50% {
                transform: scale(1.2);
                opacity: 0.8;
            }
            100% {
                transform: scale(1);
                opacity: 1;
            }
        }
        .marker-pin i {
            color: white;
            font-size: 16px;
        }
        .custom-delete-popup .leaflet-popup-content-wrapper {
            background: white;
            color: #333;
            border-radius: 8px;
            padding: 0;
            box-shadow: 0 3px 14px rgba(0,0,0,0.2);
            overflow: hidden;
        }
        .custom-delete-popup .leaflet-popup-content {
            margin: 0;
            padding: 0;
            width: 280px !important;
        }
        .custom-delete-popup .leaflet-popup-tip {
            background: white;
            box-shadow: 0 3px 14px rgba(0,0,0,0.2);
        }
        .delete-popup-header {
            background: linear-gradient(135deg, #dc3545, #c82333);
            color: white;
            padding: 12px 15px;
            font-weight: bold;
            font-size: 16px;
            display: flex;
            align-items: center;
        }
        .delete-popup-header i {
            margin-right: 8px;
            font-size: 18px;
        }
        .delete-popup-body {
            padding: 15px;
        }
        .delete-popup-info {
            margin-bottom: 15px;
        }
        .delete-popup-info p {
            margin: 8px 0;
            display: flex;
            align-items: center;
            color: #333;
        }
        .delete-popup-info i {
            width: 20px;
            margin-right: 8px;
            color: #dc3545;
        }
        .delete-location-circle {
            stroke-dasharray: 10, 10;
            animation: dash 10s linear infinite;
        }
        @keyframes dash {
            to {
                stroke-dashoffset: 200;
            }
        }
    `;
    document.head.appendChild(style);

    // Get location data from data attributes
    const lat = parseFloat(mapElement.dataset.latitude);
    const lon = parseFloat(mapElement.dataset.longitude);
    const name = mapElement.dataset.name;
    const address = mapElement.dataset.address;
    const description = mapElement.dataset.description;

    // Check if coordinates are valid
    if (!isNaN(lat) && !isNaN(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180) {
        // Initialize map with colorful theme
        var map = L.map('deleteLocationMap', {
            zoomControl: false,
            attributionControl: false
        }).setView([lat, lon], 15);
        
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

        // Add marker with custom icon
        var marker = L.marker([lat, lon], {
            icon: deleteIcon,
            title: name,
            alt: name,
            riseOnHover: true
        }).addTo(map);
        
        // Create enhanced popup content
        var popupContent = `
            <div class="delete-popup-header">
                <i class="bi bi-exclamation-triangle-fill"></i>
                Delete: ${name}
            </div>
            <div class="delete-popup-body">
                <div class="delete-popup-info">
                    <p><i class="bi bi-pin-map"></i> ${address}</p>
                    ${description ? `<p><i class="bi bi-info-circle"></i> ${description}</p>` : ''}
                </div>
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle"></i> This location will be permanently deleted!
                </div>
            </div>
        `;
        
        // Create popup with custom class
        var popup = L.popup({
            className: 'custom-delete-popup',
            closeButton: true,
            autoClose: false,
            closeOnEscapeKey: true,
            closeOnClick: false,
            minWidth: 280,
            maxWidth: 280
        }).setContent(popupContent);
        
        marker.bindPopup(popup).openPopup();

        // Add a circle around the marker to highlight the area
        L.circle([lat, lon], {
            color: '#dc3545',
            fillColor: '#dc3545',
            fillOpacity: 0.1,
            radius: 100,
            weight: 2,
            className: 'delete-location-circle'
        }).addTo(map);

        // Force map to recalculate its size
        setTimeout(() => {
            map.invalidateSize();
        }, 500);
    } else {
        console.warn(`⚠️ Invalid coordinates for location: ${name} (lat=${lat}, lon=${lon})`);
        mapElement.innerHTML = `
            <div class="alert alert-warning text-center m-3">
                <i class="bi bi-exclamation-triangle"></i> 
                Invalid coordinates for this location.
            </div>
        `;
    }
}); 