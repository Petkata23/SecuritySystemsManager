document.addEventListener("DOMContentLoaded", function () {
    // Check if map container exists
    var mapElement = document.getElementById('locationsMap');
    if (!mapElement) {
        console.error("❌ Error: Map container with id='locationsMap' not found.");
        return;
    }

    // Initialize map with colorful theme
    var map = L.map('locationsMap', {
        zoomControl: false,
        attributionControl: false
    }).setView([42.6977, 23.3242], 7); // Default center in Bulgaria

    // Define available map styles/layers
    var streets = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
    });

    var colorful = L.tileLayer('https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Tiles style by <a href="https://www.hotosm.org/" target="_blank">HOT</a>',
        maxZoom: 19
    });

    var dark = L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://stadiamaps.com/">Stadia Maps</a>, &copy; <a href="https://openmaptiles.org/">OpenMapTiles</a> &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
        maxZoom: 20
    });

    var satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
        maxZoom: 18
    });


    // Set default layer based on localStorage or use colorful as default
    var savedStyle = localStorage.getItem('preferredMapStyle');
    var defaultLayer;
    
    switch(savedStyle) {
        case 'streets':
            defaultLayer = streets;
            break;
        case 'dark':
            defaultLayer = dark;
            break;
        case 'satellite':
            defaultLayer = satellite;
            break;
        default:
            defaultLayer = colorful;
            savedStyle = 'colorful';
    }
    
    // Add default layer to map
    defaultLayer.addTo(map);

    // Create layer control
    var baseMaps = {
        "Colorful": colorful,
        "Standard": streets,
        "Dark": dark,
        "Satellite": satellite
    };

    // Add layer control to map
    L.control.layers(baseMaps, null, {
        position: 'topright'
    }).addTo(map);

    // Save selected layer to localStorage
    map.on('baselayerchange', function(e) {
        let styleKey;
        if (e.name === "Colorful") styleKey = 'colorful';
        else if (e.name === "Standard") styleKey = 'streets';
        else if (e.name === "Dark") styleKey = 'dark';
        else if (e.name === "Satellite") styleKey = 'satellite';
        
        localStorage.setItem('preferredMapStyle', styleKey);
    });

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

    // Customize control styles
    const mapStyle = document.createElement('style');
    mapStyle.textContent = `
        .leaflet-control-layers {
            border: none !important;
            border-radius: 8px !important;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2) !important;
        }

        .leaflet-control-layers-toggle {
            width: 36px !important;
            height: 36px !important;
            background-size: 20px 20px !important;
        }

        .leaflet-control-layers-expanded {
            padding: 10px !important;
            background: white !important;
            border-radius: 8px !important;
        }

        .leaflet-control-layers-expanded label {
            margin-bottom: 5px !important;
        }

        .leaflet-control-zoom {
            border: none !important;
            border-radius: 8px !important;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2) !important;
        }

        .leaflet-control-zoom a {
            width: 36px !important;
            height: 36px !important;
            line-height: 36px !important;
            border-radius: 4px !important;
            background-color: white !important;
            color: #0d6efd !important;
        }

        .leaflet-control-zoom a:hover {
            background-color: #f5f5f5 !important;
            color: #0a58ca !important;
        }

        .leaflet-control-scale {
            background: rgba(255, 255, 255, 0.8) !important;
            padding: 2px 5px !important;
            border-radius: 4px !important;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1) !important;
        }
    `;
    document.head.appendChild(mapStyle);

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
        .custom-popup .leaflet-popup-content-wrapper {
            background: white;
            color: #333;
            border-radius: 8px;
            padding: 0;
            box-shadow: 0 3px 14px rgba(0,0,0,0.2);
            overflow: hidden;
        }
        .custom-popup .leaflet-popup-content {
            margin: 0;
            padding: 0;
            width: 320px !important;
        }
        .custom-popup .leaflet-popup-tip {
            background: white;
            box-shadow: 0 3px 14px rgba(0,0,0,0.2);
        }
        .popup-header {
            background: linear-gradient(135deg, #0d6efd, #0a58ca);
            color: white;
            padding: 12px 15px;
            font-weight: bold;
            font-size: 16px;
            display: flex;
            align-items: center;
        }
        .popup-header i {
            margin-right: 8px;
            font-size: 18px;
        }
        .popup-body {
            padding: 15px;
        }
        .popup-info {
            margin-bottom: 15px;
        }
        .popup-info p {
            margin: 8px 0;
            display: flex;
            align-items: center;
            color: #333;
        }
        .popup-info i {
            width: 20px;
            margin-right: 8px;
            color: #0d6efd;
        }
        .popup-actions {
            display: flex;
            gap: 8px;
            margin-top: 15px;
        }
        .popup-actions .btn {
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 600;
        }
        .popup-actions .btn-primary {
            background-color: #0d6efd;
            border-color: #0d6efd;
            color: white;
        }
        .popup-actions .btn-warning {
            background-color: #ffc107;
            border-color: #ffc107;
            color: #212529;
        }
        .popup-actions .btn i {
            margin-right: 5px;
        }
        .order-list {
            max-height: 200px;
            overflow-y: auto;
            margin-top: 10px;
        }
        .order-item {
            padding: 8px;
            border-radius: 4px;
            margin-bottom: 5px;
            border-left: 3px solid #0d6efd;
            background-color: #f8f9fa;
        }
        .order-item:last-child {
            margin-bottom: 0;
        }
        .order-title {
            font-weight: 600;
            color: #0d6efd;
            margin-bottom: 2px;
            display: block;
        }
        .order-date {
            font-size: 12px;
            color: #6c757d;
            margin-bottom: 2px;
        }
        .order-status {
            display: inline-block;
            padding: 2px 6px;
            font-size: 12px;
            border-radius: 3px;
            font-weight: 600;
        }
        .status-pending {
            background-color: #ffc107;
            color: #212529;
        }
        .status-in-progress {
            background-color: #0dcaf0;
            color: #212529;
        }
        .status-completed {
            background-color: #198754;
            color: white;
        }
        .status-cancelled {
            background-color: #dc3545;
            color: white;
        }
        .no-orders {
            color: #6c757d;
            font-style: italic;
            text-align: center;
            padding: 10px;
            background-color: #f8f9fa;
            border-radius: 4px;
        }
    `;
    document.head.appendChild(style);

    // Load locations from server
    fetch('/Location/GetAllLocations')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load locations');
            }
            return response.json();
        })
        .then(locations => {
            // Log data for debugging
            console.log("📌 Locations data:", locations);

            if (!locations || locations.length === 0) {
                console.warn("⚠️ No locations available to display.");
                return;
            }

            var bounds = L.latLngBounds();
            var markers = L.markerClusterGroup({
                maxClusterRadius: 50,
                spiderfyOnMaxZoom: true,
                showCoverageOnHover: false,
                zoomToBoundsOnClick: true,
                disableClusteringAtZoom: 16,
                iconCreateFunction: function(cluster) {
                    return L.divIcon({
                        html: `<div class="cluster-marker">${cluster.getChildCount()}</div>`,
                        className: 'custom-cluster',
                        iconSize: L.point(40, 40)
                    });
                }
            });
            
            // Add custom CSS for clusters
            const clusterStyle = document.createElement('style');
            clusterStyle.textContent = `
                .custom-cluster {
                    background: transparent;
                    border: none;
                }
                .cluster-marker {
                    width: 40px;
                    height: 40px;
                    background: linear-gradient(135deg, #0d6efd, #0a58ca);
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: white;
                    font-weight: bold;
                    box-shadow: 0 3px 8px rgba(0,0,0,0.3);
                    border: 2px solid white;
                    animation: pulse-cluster 2s ease infinite;
                }
                @keyframes pulse-cluster {
                    0% {
                        box-shadow: 0 0 0 0 rgba(13, 110, 253, 0.7);
                    }
                    70% {
                        box-shadow: 0 0 0 10px rgba(13, 110, 253, 0);
                    }
                    100% {
                        box-shadow: 0 0 0 0 rgba(13, 110, 253, 0);
                    }
                }
            `;
            document.head.appendChild(clusterStyle);
            
            // Add markers for each location
            locations.forEach(function (location) {
                // Parse coordinates
                var lat = parseFloat(location.latitude);
                var lon = parseFloat(location.longitude);

                console.log(`📌 Coordinates for ${location.name}: lat=${lat}, lon=${lon}`);

                // Check if coordinates are valid
                if (!isNaN(lat) && !isNaN(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180) {
                    var marker = L.marker([lat, lon], {
                        icon: securityIcon,
                        title: location.name,
                        alt: location.name,
                        riseOnHover: true
                    });

                    // Function to get status badge class
                    function getStatusBadgeClass(status) {
                        switch(status.toLowerCase()) {
                            case 'pending': return 'status-pending';
                            case 'in progress': return 'status-in-progress';
                            case 'completed': return 'status-completed';
                            case 'cancelled': return 'status-cancelled';
                            default: return 'status-pending';
                        }
                    }

                    // Generate orders HTML
                    let ordersHtml = '';
                    if (location.orders && location.orders.length > 0) {
                        ordersHtml = '<div class="order-list">';
                        location.orders.forEach(order => {
                            // Format date
                            const orderDate = new Date(order.requestedDate);
                            const formattedDate = orderDate.toLocaleDateString('en-GB');
                            
                            ordersHtml += `
                                <div class="order-item">
                                    <span class="order-title">${order.title}</span>
                                    <div class="order-date">${formattedDate}</div>
                                    <span class="order-status ${getStatusBadgeClass(order.status)}">${order.status}</span>
                                </div>
                            `;
                        });
                        ordersHtml += '</div>';
                    } else {
                        ordersHtml = '<div class="no-orders">No orders at this location</div>';
                    }

                    // Create enhanced popup content
                    var popupContent = `
                        <div class="popup-header">
                            <i class="bi bi-geo-alt-fill"></i>
                            ${location.name}
                        </div>
                        <div class="popup-body">
                            <div class="popup-info">
                                <p><i class="bi bi-clipboard-check"></i> <strong>Orders at this location:</strong></p>
                                ${ordersHtml}
                            </div>
                            <div class="popup-actions">
                                <a href="/Location/Details/${location.id}" class="btn btn-sm btn-primary">
                                    <i class="bi bi-eye"></i> Details
                                </a>
                                <a href="/Location/Edit/${location.id}" class="btn btn-sm btn-warning">
                                    <i class="bi bi-pencil"></i> Edit
                                </a>
                            </div>
                        </div>
                    `;
                    
                    // Create popup with custom class
                    var popup = L.popup({
                        className: 'custom-popup',
                        closeButton: true,
                        autoClose: true,
                        closeOnEscapeKey: true,
                        closeOnClick: true,
                        minWidth: 320,
                        maxWidth: 320
                    }).setContent(popupContent);
                    
                    marker.bindPopup(popup);
                    
                    // Remove hover effect - show popup only on click
                    marker.off('mouseover');
                    
                    // Add to marker cluster group
                    markers.addLayer(marker);

                    // Add coordinates to bounds
                    bounds.extend([lat, lon]);
                } else {
                    console.warn(`⚠️ Invalid coordinates for location: ${location.name} (lat=${location.latitude}, lon=${location.longitude})`);
                }
            });
            
            // Add marker cluster group to map
            map.addLayer(markers);

            // Check if markers were added and adjust map view
            if (bounds.isValid()) {
                if (locations.length > 1) {
                    map.fitBounds(bounds, { padding: [50, 50] });
                } else {
                    map.setView(bounds.getCenter(), 15);
                }
            } else {
                console.warn("⚠️ No valid coordinates to display on the map.");
            }
        })
        .catch(error => {
            console.error('Error loading locations:', error);
            mapElement.innerHTML = `
                <div class="alert alert-danger text-center m-3">
                    <i class="bi bi-exclamation-triangle"></i> 
                    Error loading location data. Please try again later.
                </div>
            `;
        });

    // Force map to recalculate its size
    setTimeout(() => {
        map.invalidateSize();
    }, 500);
}); 