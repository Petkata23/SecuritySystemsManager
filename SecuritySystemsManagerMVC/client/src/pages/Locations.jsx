import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { locationService } from '../services/locationService';
// Leaflet CSS imports
import 'leaflet/dist/leaflet.css';
import 'leaflet.markercluster/dist/MarkerCluster.css';
import 'leaflet.markercluster/dist/MarkerCluster.Default.css';
// Additional Leaflet plugin CSS
import 'leaflet-control-geocoder/style.css';
import 'leaflet.fullscreen/dist/Control.FullScreen.css';
// Note: leaflet.markercluster and leaflet.locatecontrol will be loaded dynamically
// Main locations styles (includes all Leaflet customizations from original project)
import '../styles/locations.css';

export default function Locations() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [locations, setLocations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isMobile, setIsMobile] = useState(() => {
    if (typeof window !== 'undefined') {
      return window.innerWidth <= 768;
    }
    return false;
  });
  const mapInstanceRef = useRef(null);
  const markersByIdRef = useRef({});
  const markerClusterGroupRef = useRef(null);

  const isClient = user?.roles?.includes('Client');
  const isAdminOrManager = user?.roles?.includes('Admin') || user?.roles?.includes('Manager');

  useEffect(() => {
    loadLocations();
  }, []);

  // Handle mobile/desktop view switching (exactly as in original)
  useEffect(() => {
    const checkMobileView = () => {
      try {
        if (typeof window === 'undefined') return;
        
        const mobile = window.innerWidth <= 768;
        setIsMobile(mobile);
        
        // Toggle views based on screen size
        const tableView = document.getElementById('locationsTableView');
        const cardView = document.getElementById('locationsCardView');
        
        if (tableView && cardView) {
          if (mobile) {
            // Mobile: show card view, hide table view
            tableView.classList.add('d-none');
            cardView.classList.remove('d-none');
          } else {
            // Desktop: show table view, hide card view
            tableView.classList.remove('d-none');
            cardView.classList.add('d-none');
          }
        }
      } catch (error) {
        console.error('Error in checkMobileView:', error);
      }
    };

    // Check on mount (with delay to ensure DOM is ready)
    const timeoutId = setTimeout(checkMobileView, 100);

    // Check on window resize
    window.addEventListener('resize', checkMobileView);
    return () => {
      clearTimeout(timeoutId);
      window.removeEventListener('resize', checkMobileView);
    };
  }, [locations]); // Use locations instead of filteredLocations to avoid dependency issues

  useEffect(() => {
    if (locations.length > 0 && !mapInstanceRef.current) {
      console.log('Initializing map with locations:', locations.length);
      initializeMap(locations);
    } else if (locations.length > 0 && mapInstanceRef.current) {
      // If map is already initialized but locations changed, update markers
      console.log('Map already initialized, updating markers for', locations.length, 'locations');
      updateMarkers(locations);
    }
  }, [locations]);

  // Add effect to handle table row hover highlighting and click (exactly as in original)
  useEffect(() => {
    try {
      if (typeof document === 'undefined') return;
      
      // Use setTimeout to ensure DOM is ready
      const timeoutId = setTimeout(() => {
        if (mapInstanceRef.current && markersByIdRef.current && locations.length > 0) {
          const rows = document.querySelectorAll('.location-row');
          
          if (rows.length === 0) return; // No rows yet, skip
          
          // Remove old event listeners by cloning nodes
          rows.forEach(row => {
            try {
              const newRow = row.cloneNode(true);
              row.parentNode?.replaceChild(newRow, row);
            } catch (e) {
              console.warn('Error replacing row:', e);
            }
          });
          
          // Add new event listeners
          document.querySelectorAll('.location-row').forEach(row => {
            try {
              const locationId = parseInt(row.getAttribute('data-location-id'));
              if (locationId) {
                // Highlight on mouseenter
                row.addEventListener('mouseenter', () => {
                  try {
                    if (window.highlightMarker) {
                      window.highlightMarker(locationId);
                    } else {
                      highlightMarker(locationId);
                    }
                  } catch (e) {
                    console.error('Error highlighting marker:', e);
                  }
                });
                
                // Unhighlight on mouseleave
                row.addEventListener('mouseleave', () => {
                  try {
                    if (window.unhighlightMarkers) {
                      window.unhighlightMarkers();
                    } else {
                      unhighlightMarkers();
                    }
                  } catch (e) {
                    console.error('Error unhighlighting markers:', e);
                  }
                });
                
                // Click to zoom and show popup
                row.addEventListener('click', () => {
                  try {
                    const marker = markersByIdRef.current[locationId];
                    if (marker && mapInstanceRef.current) {
                      if (markerClusterGroupRef.current && markerClusterGroupRef.current.getVisibleParent) {
                        // Use clustering if available
                        const parent = markerClusterGroupRef.current.getVisibleParent(marker);
                        if (parent === marker) {
                          // Marker is visible (not in a cluster)
                          mapInstanceRef.current.setView(marker.getLatLng(), 16);
                          setTimeout(() => {
                            marker.openPopup();
                          }, 300);
                        } else {
                          // Marker is in a cluster, zoom to show it
                          if (markerClusterGroupRef.current.zoomToShowLayer) {
                            markerClusterGroupRef.current.zoomToShowLayer(marker, () => {
                              marker.openPopup();
                            });
                          } else {
                            // Fallback if zoomToShowLayer is not available
                            mapInstanceRef.current.setView(marker.getLatLng(), 16);
                            setTimeout(() => {
                              marker.openPopup();
                            }, 300);
                          }
                        }
                      } else {
                        // No clustering - just zoom and show popup
                        mapInstanceRef.current.setView(marker.getLatLng(), 16);
                        setTimeout(() => {
                          marker.openPopup();
                        }, 300);
                      }
                    }
                  } catch (e) {
                    console.error('Error handling row click:', e);
                  }
                });
              }
            } catch (e) {
              console.error('Error setting up row listeners:', e);
            }
          });
        }
      }, 200);

      return () => clearTimeout(timeoutId);
    } catch (error) {
      console.error('Error in table row effect:', error);
    }
  }, [locations]); // Don't include refs in dependencies

  const loadLocations = async () => {
    try {
      setLoading(true);
      console.log('Loading locations...');
      const response = await locationService.getAllLocations();
      console.log('Locations response:', response);
      
      // Handle different response formats
      let locationsData = [];
      if (Array.isArray(response)) {
        locationsData = response;
      } else if (response && Array.isArray(response.data)) {
        locationsData = response.data;
      } else if (response && response.locations && Array.isArray(response.locations)) {
        locationsData = response.locations;
      }
      
      console.log('Locations loaded:', locationsData.length);
      console.log('Locations data sample:', locationsData.slice(0, 2));
      
      // Validate that locations have coordinates
      const locationsWithCoords = locationsData.filter(loc => {
        const lat = parseFloat(loc.latitude);
        const lon = parseFloat(loc.longitude);
        return !isNaN(lat) && !isNaN(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;
      });
      
      console.log('Locations with valid coordinates:', locationsWithCoords.length);
      if (locationsWithCoords.length < locationsData.length) {
        console.warn('Some locations are missing valid coordinates');
      }
      
      setLocations(locationsData);
    } catch (error) {
      console.error('Error loading locations:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      setLocations([]);
    } finally {
      setLoading(false);
    }
  };

  const highlightMarker = (locationId) => {
    const marker = markersByIdRef.current[locationId];
    if (marker && window.highlightedIcon) {
      marker.setIcon(window.highlightedIcon);
      marker.setZIndexOffset(1000);
    }
  };

  const unhighlightMarkers = () => {
    if (mapInstanceRef.current && window.securityIcon) {
      Object.values(markersByIdRef.current).forEach(marker => {
        marker.setIcon(window.securityIcon);
        marker.setZIndexOffset(0);
      });
    }
  };

  const initializeMap = async (locationsToMap = locations) => {
    try {
      if (mapInstanceRef.current) {
        console.log('Map already initialized, skipping...');
        return;
      }
      
      const mapElement = document.getElementById('locationsMap');
      if (!mapElement) {
        console.log('Map element not found, skipping...');
        return;
      }

      // Check if map container is already initialized
      if (mapElement._leaflet_id) {
        console.log('Map container already initialized, skipping...');
        return;
      }

      console.log('Starting map initialization with', locationsToMap.length, 'locations');
      // Dynamically import Leaflet
      const L = (await import('leaflet')).default;
      window.L = L; // Store globally for access from other functions
      
      // Dynamically import marker cluster plugin
      try {
        await import('leaflet.markercluster');
        
        // After import, L.markerClusterGroup should be available
        if (!L.markerClusterGroup) {
          console.warn('⚠️ L.markerClusterGroup not available after import, trying alternative...');
          // Try alternative import
          const markerClusterModule = await import('leaflet.markercluster');
          console.log('MarkerCluster module keys:', Object.keys(markerClusterModule));
          
          // Try to manually extend L if needed
          if (markerClusterModule.default) {
            const MarkerClusterGroupClass = markerClusterModule.default;
            if (typeof MarkerClusterGroupClass === 'function') {
              L.markerClusterGroup = function(options) {
                return new MarkerClusterGroupClass(options);
              };
              console.log('✅ Manually extended L.markerClusterGroup');
            }
          }
        }
        
        if (!L.markerClusterGroup) {
          console.error('❌ Failed to load markerClusterGroup. Continuing without clustering...');
          // Don't throw - continue without clustering
        } else {
          console.log('✅ MarkerClusterGroup available:', typeof L.markerClusterGroup);
        }
      } catch (clusterError) {
        console.error('❌ Error loading markerClusterGroup:', clusterError);
        // Continue without clustering - don't break the whole app
      }
      
      // Fix for default marker icon issue
      delete L.Icon.Default.prototype._getIconUrl;
      L.Icon.Default.mergeOptions({
        iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
        iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
      });

      // Initialize map with same settings as original
      const map = L.map('locationsMap', {
        zoomControl: false,
        attributionControl: false,
        fullscreenControl: true
      }).setView([42.6977, 23.3242], 7);

      // Define available map styles/layers (exactly as in original)
      const streets = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
      });

      const colorful = L.tileLayer('https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Tiles style by <a href="https://www.hotosm.org/" target="_blank">HOT</a>',
        maxZoom: 19
      });

      const dark = L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth_dark/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://stadiamaps.com/">Stadia Maps</a>, &copy; <a href="https://openmaptiles.org/">OpenMapTiles</a> &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors',
        maxZoom: 20
      });

      const satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
        maxZoom: 18
      });

      const detailed = L.tileLayer('https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
      });

      // Set default layer based on localStorage or use colorful as default
      const savedStyle = localStorage.getItem('preferredMapStyle');
      let defaultLayer;
      
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
        case 'detailed':
          defaultLayer = detailed;
          break;
        default:
          defaultLayer = colorful;
      }
      
      // Add default layer to map
      defaultLayer.addTo(map);

      // Create layer control
      const baseMaps = {
        "Colorful": colorful,
        "Standard": streets,
        "Detailed": detailed,
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
        else if (e.name === "Detailed") styleKey = 'detailed';
        
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

      // Add fullscreen control
      try {
        const FullscreenModule = await import('leaflet.fullscreen');
        const Fullscreen = FullscreenModule.default || FullscreenModule;
        if (Fullscreen && Fullscreen.Control) {
          map.addControl(new Fullscreen.Control({
            position: 'topright',
            title: 'Show fullscreen',
            titleCancel: 'Exit fullscreen'
          }));
        }
      } catch (err) {
        console.log('Fullscreen control not available:', err);
      }

      // Add locate control
      try {
        // Load CSS dynamically
        const locateCSS = document.createElement('link');
        locateCSS.rel = 'stylesheet';
        locateCSS.href = 'https://cdn.jsdelivr.net/npm/leaflet.locatecontrol@0.86.0/dist/L.Control.Locate.css';
        document.head.appendChild(locateCSS);
        
        // Import locate control (it extends L.control automatically)
        const LocateControlModule = await import('leaflet.locatecontrol');
        // The module should extend L.control.locate automatically
        if (L.control && L.control.locate) {
          L.control.locate({
            position: 'topright',
            strings: {
              title: "Show my location"
            },
            locateOptions: {
              enableHighAccuracy: true,
              maxZoom: 15
            }
          }).addTo(map);
        } else {
          // If not available, try to use the class directly
          const LocateControl = LocateControlModule.default || LocateControlModule.LocateControl || LocateControlModule;
          if (LocateControl) {
            const locateControl = new LocateControl({
              position: 'topright',
              strings: {
                title: "Show my location"
              },
              locateOptions: {
                enableHighAccuracy: true,
                maxZoom: 15
              }
            });
            locateControl.addTo(map);
          }
        }
      } catch (err) {
        console.log('Locate control not available:', err);
      }

      // Add geocoder control
      try {
        const GeocoderModule = await import('leaflet-control-geocoder');
        const Geocoder = GeocoderModule.default || GeocoderModule;
        if (Geocoder && Geocoder.geocoder) {
          Geocoder.geocoder({
            defaultMarkGeocode: false
          }).on('markgeocode', function(e) {
            const latlng = e.geocode.center;
            L.marker(latlng).addTo(map).bindPopup(e.geocode.name).openPopup();
            map.fitBounds(e.geocode.bbox);
          }).addTo(map);
        }
      } catch (err) {
        console.log('Geocoder not available:', err);
      }

      // Add custom CSS for markers and popups (exactly as in original)
      // This must be added BEFORE creating markers
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
        .marker-pin.highlighted {
            width: 42px;
            height: 42px;
            background: #dc3545;
            animation: bounce-highlight 0.5s ease infinite alternate;
            z-index: 1000;
        }
        @keyframes bounce {
            from {
                transform: translateY(0);
            }
            to {
                transform: translateY(-5px);
            }
        }
        @keyframes bounce-highlight {
            from {
                transform: translateY(0) scale(1.1);
            }
            to {
                transform: translateY(-8px) scale(1.1);
            }
        }
        .marker-pin i {
            color: white;
            font-size: 16px;
            animation: pulse 1.5s ease infinite;
        }
        .marker-pin.highlighted i {
            font-size: 20px;
            animation: pulse-highlight 1.5s ease infinite;
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
        @keyframes pulse-highlight {
            0% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.3);
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
      document.head.appendChild(style);

      // Add custom CSS for map controls (exactly as in original)
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

        .leaflet-control-fullscreen a {
            width: 36px !important;
            height: 36px !important;
            line-height: 36px !important;
            border-radius: 4px !important;
            background-color: white !important;
            color: #0d6efd !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
        }

        .leaflet-control-fullscreen a:hover {
            background-color: #f5f5f5 !important;
            color: #0a58ca !important;
        }

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
        .marker-pin.highlighted {
            width: 42px;
            height: 42px;
            background: #dc3545;
            animation: bounce-highlight 0.5s ease infinite alternate;
            z-index: 1000;
        }
        @keyframes bounce {
            from {
                transform: translateY(0);
            }
            to {
                transform: translateY(-5px);
            }
        }
        @keyframes bounce-highlight {
            from {
                transform: translateY(0) scale(1.1);
            }
            to {
                transform: translateY(-8px) scale(1.1);
            }
        }
        .marker-pin i {
            color: white;
            font-size: 16px;
            animation: pulse 1.5s ease infinite;
        }
        .marker-pin.highlighted i {
            font-size: 20px;
            animation: pulse-highlight 1.5s ease infinite;
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
        @keyframes pulse-highlight {
            0% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.3);
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
      document.head.appendChild(mapStyle);

      // Custom security icon (exactly as in original)
      const securityIcon = L.divIcon({
        className: 'custom-security-marker',
        html: '<div class="marker-pin"><i class="bi bi-shield-lock-fill"></i></div>',
        iconSize: [36, 36],
        iconAnchor: [18, 36],
        popupAnchor: [0, -36]
      });

      // Highlighted security icon (exactly as in original)
      const highlightedIcon = L.divIcon({
        className: 'custom-security-marker',
        html: '<div class="marker-pin highlighted"><i class="bi bi-shield-lock-fill"></i></div>',
        iconSize: [42, 42],
        iconAnchor: [21, 42],
        popupAnchor: [0, -42]
      });

      // Store icons globally for highlightMarker function (exactly as in original)
      window.securityIcon = securityIcon;
      window.highlightedIcon = highlightedIcon;

      // Create marker cluster group with custom styling (exactly as in original)
      let markers;
      if (L.markerClusterGroup) {
        markers = L.markerClusterGroup({
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
        markerClusterGroupRef.current = markers;
      } else {
        // Fallback: create a simple layer group if clustering is not available
        console.warn('⚠️ Using fallback layer group instead of markerClusterGroup');
        markers = L.layerGroup();
        markerClusterGroupRef.current = markers;
      }
      
      // Add custom CSS for clusters (exactly as in original)
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

      // Helper functions for status
      const getStatusBadgeClass = (status) => {
        switch(status?.toLowerCase()) {
          case 'pending': return 'status-pending';
          case 'in progress': return 'status-in-progress';
          case 'completed': return 'status-completed';
          case 'cancelled': return 'status-cancelled';
          default: return 'status-pending';
        }
      };

      const getStatusDisplayName = (status) => {
        switch(status?.toLowerCase()) {
          case 'pending': return 'Изчакваща';
          case 'in progress': return 'В процес';
          case 'completed': return 'Завършена';
          case 'cancelled': return 'Отменена';
          default: return status || 'Изчакваща';
        }
      };

      // Add markers for each location
      const bounds = L.latLngBounds();
      console.log('Creating markers for', locationsToMap.length, 'locations');
      
      locationsToMap.forEach((location) => {
        const lat = parseFloat(location.latitude);
        const lon = parseFloat(location.longitude);

        console.log(`Processing location: ${location.name}, lat: ${lat}, lon: ${lon}`);

        if (!isNaN(lat) && !isNaN(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180) {
          const marker = L.marker([lat, lon], {
            icon: securityIcon,
            title: location.name,
            alt: location.name,
            riseOnHover: true,
            locationId: location.id
          });

          // Store marker by location ID
          markersByIdRef.current[location.id] = marker;

          // Generate orders HTML
          let ordersHtml = '';
          if (location.orders && location.orders.length > 0) {
            ordersHtml = '<div class="order-list">';
            location.orders.forEach(order => {
              const orderDate = new Date(order.requestedDate || order.requestedDate);
              const formattedDate = orderDate.toLocaleDateString('bg-BG');
              
              ordersHtml += `
                <div class="order-item">
                  <span class="order-title">${order.title || 'Поръчка'}</span>
                  <div class="order-date">${formattedDate}</div>
                  <span class="order-status ${getStatusBadgeClass(order.status)}">${getStatusDisplayName(order.status)}</span>
                </div>
              `;
            });
            ordersHtml += '</div>';
          } else {
            ordersHtml = '<div class="no-orders">Няма поръчки на тази локация</div>';
          }

          // Check if mobile device
          const isMobile = window.innerWidth <= 768;
          
          // Truncate address for mobile devices
          let displayAddress = location.address || 'Няма адрес';
          if (isMobile && displayAddress.length > 45) {
            displayAddress = displayAddress.substring(0, 42) + '...';
          }
          
          // Create enhanced popup content (exactly as in original)
          const popupContent = `
            <div class="popup-header">
              <i class="bi bi-geo-alt-fill"></i>
              ${location.name || 'Локация'}
            </div>
            <div class="popup-body">
              <div class="popup-info">
                <p><i class="bi bi-geo-alt"></i> ${isMobile ? '' : '<strong>Адрес:</strong> '}${displayAddress}</p>
                ${!isMobile ? `<p><i class="bi bi-clipboard-check"></i> <strong>Поръчки на тази локация:</strong></p>` : ''}
                ${ordersHtml}
              </div>
              <div class="popup-actions">
                <a href="/locations/${location.id}" class="btn btn-sm btn-primary">
                  <i class="bi bi-eye"></i> Детайли
                </a>
                ${!isMobile && isAdminOrManager ? `<a href="/locations/${location.id}/edit" class="btn btn-sm btn-warning">
                  <i class="bi bi-pencil"></i> Редактирай
                </a>` : ''}
              </div>
            </div>
          `;
          
          // Create popup with custom class (exactly as in original)
          const popupWidth = isMobile ? 260 : 320;
          const popup = L.popup({
            className: 'custom-popup',
            closeButton: true,
            autoClose: true,
            closeOnEscapeKey: true,
            closeOnClick: true,
            minWidth: popupWidth,
            maxWidth: popupWidth
          }).setContent(popupContent);
          
          // Bind popup to marker (exactly as in original)
          marker.bindPopup(popup);
          
          // Remove hover effect - show popup only on click (exactly as in original)
          marker.off('mouseover');
          
          // Add to marker cluster group
          markers.addLayer(marker);
          
          console.log(`✅ Marker added for location: ${location.name} at [${lat}, ${lon}]`);

          // Add coordinates to bounds
          bounds.extend([lat, lon]);
        } else {
          console.warn(`⚠️ Invalid coordinates for location: ${location.name} (lat=${location.latitude}, lon=${location.longitude})`);
        }
      });
      
      // Add marker cluster group to map
      map.addLayer(markers);

      // Check if markers were added and adjust map view
      console.log('Total markers added:', Object.keys(markersByIdRef.current).length);
      console.log('Bounds valid:', bounds.isValid());
      
      if (bounds.isValid()) {
        if (locationsToMap.length > 1) {
          map.fitBounds(bounds, { padding: [50, 50] });
          console.log('Fitted bounds for multiple locations');
        } else {
          map.setView(bounds.getCenter(), 15);
          console.log('Set view for single location');
        }
      } else {
        console.warn('⚠️ No valid coordinates to display on the map.');
      }

      // Store map globally for access from other scripts
      window.map = map;
      
      // Store highlight functions globally (as in original)
      window.highlightMarker = (locationId) => {
        highlightMarker(locationId);
      };
      
      window.unhighlightMarkers = () => {
        unhighlightMarkers();
      };

      // Force map to recalculate its size
      setTimeout(() => {
        map.invalidateSize();
      }, 500);

      mapInstanceRef.current = map;
      console.log('✅ Map initialized successfully with all features');
      console.log('Markers in cluster group:', markers.getLayers().length);
    } catch (error) {
      console.error('❌ Error initializing map:', error);
      console.error('Error details:', error.stack);
      // Don't throw - just log the error so the app doesn't crash
      // The map will just not show markers, but the rest of the app should work
    }
  };

  // Function to update markers when locations change
  const updateMarkers = (locationsToMap) => {
    if (!mapInstanceRef.current || !markerClusterGroupRef.current) {
      console.log('Map or marker cluster group not initialized');
      return;
    }

    const map = mapInstanceRef.current;
    const markers = markerClusterGroupRef.current;
    const L = window.L;

    // Clear existing markers
    markers.clearLayers();
    markersByIdRef.current = {};

    // Recreate markers
    const bounds = L.latLngBounds();
    const securityIcon = window.securityIcon;

    locationsToMap.forEach((location) => {
      const lat = parseFloat(location.latitude);
      const lon = parseFloat(location.longitude);

      if (!isNaN(lat) && !isNaN(lon) && lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180) {
        const marker = L.marker([lat, lon], {
          icon: securityIcon,
          title: location.name,
          alt: location.name,
          riseOnHover: true,
          locationId: location.id
        });

        markersByIdRef.current[location.id] = marker;
        
        // Create popup (simplified for update)
        const popupContent = `<div>${location.name}</div>`;
        const popup = L.popup({
          className: 'custom-popup',
          closeButton: true,
          autoClose: true,
          closeOnEscapeKey: true,
          closeOnClick: true
        }).setContent(popupContent);
        
        marker.bindPopup(popup);
        marker.off('mouseover');
        markers.addLayer(marker);
        bounds.extend([lat, lon]);
      }
    });

    if (bounds.isValid()) {
      if (locationsToMap.length > 1) {
        map.fitBounds(bounds, { padding: [50, 50] });
      } else {
        map.setView(bounds.getCenter(), 15);
      }
    }
  };

  const filteredLocations = locations.filter((location) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      location.name?.toLowerCase().includes(searchLower) ||
      location.address?.toLowerCase().includes(searchLower) ||
      location.id?.toString().toLowerCase().includes(searchLower)
    );
  });

  // Filter locations for both table and card views (exactly as in original)
  // Note: This effect runs after DOM is rendered, so we use setTimeout to ensure elements exist
  useEffect(() => {
    // Use setTimeout to ensure DOM elements are rendered
    const timeoutId = setTimeout(() => {
      try {
        if (typeof document === 'undefined') return;
        
        if (searchTerm) {
          const searchLower = searchTerm.toLowerCase();
          
          // Filter table rows
          const tableRows = document.querySelectorAll('#locationsTable tbody tr');
          tableRows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchLower) ? '' : 'none';
          });
          
          // Filter card items
          const cardItems = document.querySelectorAll('.location-card-item');
          cardItems.forEach(card => {
            const text = card.textContent.toLowerCase();
            const col = card.closest('.col-12');
            if (col) {
              col.style.display = text.includes(searchLower) ? '' : 'none';
            }
          });
        } else {
          // Show all when search is cleared
          const tableRows = document.querySelectorAll('#locationsTable tbody tr');
          tableRows.forEach(row => {
            row.style.display = '';
          });
          const cardItems = document.querySelectorAll('.location-card-item');
          cardItems.forEach(card => {
            const col = card.closest('.col-12');
            if (col) {
              col.style.display = '';
            }
          });
        }
      } catch (error) {
        console.error('Error filtering locations:', error);
      }
    }, 100);

    return () => clearTimeout(timeoutId);
  }, [searchTerm, locations]); // Use locations instead of filteredLocations

  const getOrderStatusCounts = (orders) => {
    if (!orders || orders.length === 0) return { pending: 0, inProgress: 0, completed: 0 };
    
    return {
      pending: orders.filter((o) => o.status === 'Pending').length,
      inProgress: orders.filter((o) => o.status === 'InProgress').length,
      completed: orders.filter((o) => o.status === 'Completed').length,
    };
  };

  // Toggle map size - Fullscreen on mobile, expand on desktop (exactly as in original)
  const handleToggleMapSize = () => {
    const mapElement = document.getElementById('locationsMap');
    const mapCard = mapElement?.closest('.map-card');
    const button = document.getElementById('toggleMapSize');
    const icon = button?.querySelector('i');
    const isMobile = window.innerWidth <= 768;
    
    if (!mapElement || !mapCard || !button) return;
    
    if (isMobile) {
      // Mobile: Use fullscreen API
      if (!document.fullscreenElement && !document.mozFullScreenElement && 
          !document.webkitFullscreenElement && !document.msFullscreenElement) {
        // Enter fullscreen
        const element = mapCard;
        let promise;
        
        if (element.requestFullscreen) {
          promise = element.requestFullscreen();
        } else if (element.mozRequestFullScreen) {
          promise = element.mozRequestFullScreen();
        } else if (element.webkitRequestFullscreen) {
          promise = element.webkitRequestFullscreen();
        } else if (element.msRequestFullscreen) {
          promise = element.msRequestFullscreen();
        }
        
        if (icon) {
          icon.classList.remove('bi-arrows-angle-expand');
          icon.classList.add('bi-arrows-angle-contract');
        }
        button.innerHTML = '<i class="bi bi-arrows-angle-contract"></i> Намали';
        
        // Wait for fullscreen to be entered, then resize map
        if (promise) {
          promise.then(() => {
            // Fullscreen entered successfully
            setTimeout(() => {
              if (window.map && typeof window.map.invalidateSize === 'function') {
                window.map.invalidateSize(true); // Force resize
              }
              window.dispatchEvent(new Event('resize'));
            }, 100);
          }).catch((err) => {
            console.error('Error entering fullscreen:', err);
          });
        } else {
          // Fallback if promise is not supported
          setTimeout(() => {
            if (window.map && typeof window.map.invalidateSize === 'function') {
              window.map.invalidateSize(true);
            }
            window.dispatchEvent(new Event('resize'));
          }, 300);
        }
        
        // Update button when exiting fullscreen
        const exitFullscreenHandler = () => {
          if (!document.fullscreenElement && !document.mozFullScreenElement && 
              !document.webkitFullscreenElement && !document.msFullscreenElement) {
            if (icon) {
              icon.classList.remove('bi-arrows-angle-contract');
              icon.classList.add('bi-arrows-angle-expand');
            }
            button.innerHTML = '<i class="bi bi-arrows-angle-expand"></i> Разшири';
            document.removeEventListener('fullscreenchange', exitFullscreenHandler);
            document.removeEventListener('webkitfullscreenchange', exitFullscreenHandler);
            document.removeEventListener('mozfullscreenchange', exitFullscreenHandler);
            document.removeEventListener('MSFullscreenChange', exitFullscreenHandler);
            
            // Trigger map resize after exiting fullscreen
            setTimeout(() => {
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
      if (mapElement.classList.contains('map-container-lg')) {
        mapElement.classList.remove('map-container-lg');
        if (icon) {
          icon.classList.remove('bi-arrows-angle-contract');
          icon.classList.add('bi-arrows-angle-expand');
        }
        button.innerHTML = '<i class="bi bi-arrows-angle-expand"></i> Разшири';
      } else {
        mapElement.classList.add('map-container-lg');
        if (icon) {
          icon.classList.remove('bi-arrows-angle-expand');
          icon.classList.add('bi-arrows-angle-contract');
        }
        button.innerHTML = '<i class="bi bi-arrows-angle-contract"></i> Намали';
      }
    }
    
    // Trigger map resize event
    setTimeout(() => {
      if (window.map && typeof window.map.invalidateSize === 'function') {
        window.map.invalidateSize();
      }
      window.dispatchEvent(new Event('resize'));
    }, 200);
  };

  // Initialize tooltips (as in original)
  useEffect(() => {
    // Initialize Bootstrap tooltips if available
    if (window.bootstrap && window.bootstrap.Tooltip) {
      const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
      const tooltipList = [...tooltipTriggerList].map(
        tooltipTriggerEl => new window.bootstrap.Tooltip(tooltipTriggerEl)
      );
      
      return () => {
        tooltipList.forEach(tooltip => tooltip.dispose());
      };
    }
  }, [locations]);

  if (loading) {
    return (
      <div className="container-fluid" style={{ padding: '2rem' }}>
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Зареждане...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid locations-list-container animate-fade-in px-4">
      {filteredLocations.length > 0 ? (
        <>
          {/* Modern Banner */}
          <div className="locations-banner">
            <div className="locations-banner-content">
              <div className="locations-banner-text">
                <h1 className="locations-banner-title">
                  {isClient ? 'Моите локации' : 'Локации'}
                </h1>
                <p className="locations-banner-description">
                  Управлявайте всички ваши локации и проследявайте поръчките им на едно място
                </p>
              </div>
              {isAdminOrManager && (
                <div className="locations-banner-actions">
                  <Link to="/locations/create" className="btn btn-create-location-banner">
                    <i className="bi bi-plus-lg"></i>
                    <span>Добави нова локация</span>
                  </Link>
                </div>
              )}
            </div>
          </div>

          {/* Map Card */}
          <div className="row mb-4">
            <div className="col-12">
              <div className="card map-card shadow-sm">
                <div className="card-header bg-transparent">
                  <div className="d-flex justify-content-between align-items-center">
                    <h5 className="mb-0">
                      <i className="bi bi-map"></i>
                      Карта на обектите ми
                    </h5>
                    <div className="map-controls-toggle d-none d-md-block">
                      <button 
                        className="btn btn-sm btn-outline-primary" 
                        id="toggleMapSize"
                        onClick={handleToggleMapSize}
                      >
                        <i className="bi bi-arrows-angle-expand"></i> Разшири
                      </button>
                    </div>
                  </div>
                </div>
                <div className="card-body p-0">
                  <div id="locationsMap" className="locations-list-map"></div>
                </div>
              </div>
            </div>
          </div>
        </>
      ) : (
        <div className="d-flex justify-content-between align-items-center mb-4">
          <h1 className="h3 mb-0">{isClient ? 'Моите локации' : 'Локации'}</h1>
          {isAdminOrManager && (
            <Link to="/locations/create" className="btn btn-primary">
              <i className="bi bi-plus-lg me-2"></i>Добави нова локация
            </Link>
          )}
        </div>
      )}

      <div className="row">
        <div className="col-12">
          <div className="card locations-card shadow-sm">
            <div className="card-header bg-transparent">
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">
                  <i className="bi bi-list-ul"></i>
                  {isClient ? 'Моите локации' : 'Всички локации'}
                </h5>
                {filteredLocations.length > 0 && (
                  <div className="location-search-container">
                    <input
                      type="text"
                      id="locationSearch"
                      className="form-control"
                      placeholder="Търси локации..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                    />
                  </div>
                )}
              </div>
            </div>
            <div className="card-body p-0">
              {filteredLocations.length > 0 ? (
                <>
                  {/* Table View (Desktop) */}
                  <div className={`table-responsive ${isMobile ? 'd-none' : ''}`} id="locationsTableView">
                    <table className="table table-hover align-middle mb-0" id="locationsTable">
                      <thead>
                        <tr>
                          <th>Име</th>
                          <th>Адрес</th>
                          <th>Поръчки</th>
                          <th>Действия</th>
                        </tr>
                      </thead>
                      <tbody>
                        {filteredLocations.map((location) => {
                          const orderCounts = getOrderStatusCounts(location.orders);
                          const totalOrders = location.orders?.length || 0;

                          return (
                            <tr 
                              key={location.id} 
                              data-location-id={location.id} 
                              className="location-row"
                              style={{ cursor: 'pointer' }}
                            >
                              <td>
                                <div className="d-flex align-items-center">
                                  <div className="location-icon me-3">
                                    <i className="bi bi-geo-alt-fill text-primary"></i>
                                  </div>
                                  <div>
                                    <h6 className="mb-0">{location.name}</h6>
                                    <small className="text-muted">ID: {location.id}</small>
                                  </div>
                                </div>
                              </td>
                              <td>{location.address}</td>
                              <td>
                                {totalOrders > 0 ? (
                                  <div className="d-flex align-items-center">
                                    <span className="badge bg-primary rounded-pill me-2">
                                      {totalOrders}
                                    </span>
                                    <div className="order-status-indicators">
                                      {orderCounts.pending > 0 && (
                                        <span className="badge bg-warning me-1" title={`${orderCounts.pending} Pending`}>
                                          {orderCounts.pending}
                                        </span>
                                      )}
                                      {orderCounts.inProgress > 0 && (
                                        <span className="badge bg-primary me-1" title={`${orderCounts.inProgress} In Progress`}>
                                          {orderCounts.inProgress}
                                        </span>
                                      )}
                                      {orderCounts.completed > 0 && (
                                        <span className="badge bg-success" title={`${orderCounts.completed} Completed`}>
                                          {orderCounts.completed}
                                        </span>
                                      )}
                                    </div>
                                  </div>
                                ) : (
                                  <span className="badge bg-secondary">Няма поръчки</span>
                                )}
                              </td>
                              <td>
                                <div className="btn-group" style={{ position: 'relative', zIndex: 1 }}>
                                  <Link to={`/locations/${location.id}`} className="btn btn-sm btn-info">
                                    <i className="bi bi-info-circle"></i>
                                  </Link>
                                  {isAdminOrManager && (
                                    <>
                                      <Link to={`/locations/${location.id}/edit`} className="btn btn-sm btn-primary">
                                        <i className="bi bi-pencil"></i>
                                      </Link>
                                      <Link to={`/locations/${location.id}/delete`} className="btn btn-sm btn-danger">
                                        <i className="bi bi-trash"></i>
                                      </Link>
                                    </>
                                  )}
                                </div>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>

                  {/* Card View (Mobile) - exactly as in original */}
                  <div className={`locations-card-view ${!isMobile ? 'd-none' : ''}`} id="locationsCardView">
                    <div className="row">
                      {filteredLocations.map((location) => {
                        const orderCounts = getOrderStatusCounts(location.orders);
                        const totalOrders = location.orders?.length || 0;

                        return (
                          <div key={location.id} className="col-12 mb-3">
                            <div 
                              className="location-card-item" 
                              data-location-id={location.id}
                              style={{ cursor: 'pointer' }}
                              onClick={() => {
                                const marker = markersByIdRef.current[location.id];
                                if (marker && markerClusterGroupRef.current && mapInstanceRef.current) {
                                  const parent = markerClusterGroupRef.current.getVisibleParent?.(marker);
                                  if (parent === marker) {
                                    mapInstanceRef.current.setView(marker.getLatLng(), 16);
                                    setTimeout(() => marker.openPopup(), 300);
                                  } else if (markerClusterGroupRef.current.zoomToShowLayer) {
                                    markerClusterGroupRef.current.zoomToShowLayer(marker, () => {
                                      marker.openPopup();
                                    });
                                  } else {
                                    mapInstanceRef.current.setView(marker.getLatLng(), 16);
                                    setTimeout(() => marker.openPopup(), 300);
                                  }
                                }
                              }}
                              onMouseEnter={() => highlightMarker(location.id)}
                              onMouseLeave={() => unhighlightMarkers()}
                            >
                              <div className="location-card-header">
                                <div className="location-card-icon">
                                  <i className="bi bi-geo-alt-fill"></i>
                                </div>
                                <div className="location-card-title">
                                  <h6>{location.name}</h6>
                                  <small>ID: {location.id}</small>
                                </div>
                              </div>
                              <div className="location-card-body">
                                <div className="location-card-address">
                                  <i className="bi bi-geo-alt"></i>
                                  <span>{location.address}</span>
                                </div>
                                <div className="location-card-orders">
                                  {totalOrders > 0 ? (
                                    <>
                                      <span className="badge bg-primary rounded-pill">
                                        {totalOrders} поръчки
                                      </span>
                                      {orderCounts.pending > 0 && (
                                        <span className="badge bg-warning" title={`${orderCounts.pending} Pending`}>
                                          {orderCounts.pending}
                                        </span>
                                      )}
                                      {orderCounts.inProgress > 0 && (
                                        <span className="badge bg-primary" title={`${orderCounts.inProgress} In Progress`}>
                                          {orderCounts.inProgress}
                                        </span>
                                      )}
                                      {orderCounts.completed > 0 && (
                                        <span className="badge bg-success" title={`${orderCounts.completed} Completed`}>
                                          {orderCounts.completed}
                                        </span>
                                      )}
                                    </>
                                  ) : (
                                    <span className="badge bg-secondary">Няма поръчки</span>
                                  )}
                                </div>
                              </div>
                              <div className="location-card-footer">
                                <Link to={`/locations/${location.id}`} className="btn btn-sm btn-info">
                                  <i className="bi bi-info-circle"></i>
                                </Link>
                                {isAdminOrManager && (
                                  <>
                                    <Link to={`/locations/${location.id}/edit`} className="btn btn-sm btn-primary">
                                      <i className="bi bi-pencil"></i>
                                    </Link>
                                    <Link to={`/locations/${location.id}/delete`} className="btn btn-sm btn-danger">
                                      <i className="bi bi-trash"></i>
                                    </Link>
                                  </>
                                )}
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </>
              ) : (
                <div className="locations-empty-state">
                  <i className="bi bi-geo-alt"></i>
                  <h5>Няма намерени локации</h5>
                  <p>Започнете като добавите първата си локация</p>
                  {isAdminOrManager && (
                    <Link to="/locations/create" className="btn btn-primary">
                      <i className="bi bi-plus-lg me-2"></i>Добави локация
                    </Link>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
