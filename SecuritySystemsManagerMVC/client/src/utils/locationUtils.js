/**
 * Utility functions for working with locations and maps
 * Ported from original locations.js
 */

/**
 * Initialize a map
 * @param {string} elementId - ID of the map container element
 * @param {Array<number>} center - [lat, lng] coordinates
 * @param {number} zoom - Zoom level
 * @returns {L.Map|null} Leaflet map instance or null if failed
 */
export async function initMap(elementId, center = [42.6977, 23.3242], zoom = 7) {
  const mapElement = document.getElementById(elementId);
  
  if (!mapElement) {
    console.error(`❌ Error: Element with id='${elementId}' not found.`);
    return null;
  }
  
  // Dynamically import Leaflet
  const L = (await import('leaflet')).default;
  
  const map = L.map(elementId).setView(center, zoom);
  
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);
  
  // Add scale control
  L.control.scale({
    imperial: false,
    position: 'bottomleft'
  }).addTo(map);
  
  // Force map to recalculate its size
  setTimeout(() => {
    map.invalidateSize();
  }, 500);
  
  return map;
}

/**
 * Add a marker to the map
 * @param {L.Map} map - Leaflet map instance
 * @param {number} lat - Latitude
 * @param {number} lng - Longitude
 * @param {string|null} popupContent - Popup content HTML
 * @param {boolean} draggable - Whether the marker is draggable
 * @returns {L.Marker|null} Marker instance or null if failed
 */
export async function addMarker(map, lat, lng, popupContent = null, draggable = false) {
  if (!map || isNaN(lat) || isNaN(lng)) {
    console.error("❌ Error: Invalid parameters for adding marker.");
    return null;
  }
  
  const L = window.L || (await import('leaflet')).default;
  const marker = L.marker([lat, lng], { draggable: draggable }).addTo(map);
  
  if (popupContent) {
    marker.bindPopup(popupContent);
  }
  
  return marker;
}

/**
 * Search for an address using Nominatim
 * @param {string} query - Address search query
 * @returns {Promise<Array>} Array of search results
 */
export function searchAddress(query) {
  return new Promise((resolve, reject) => {
    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}`)
      .then(response => {
        if (!response.ok) {
          throw new Error("Error searching for address");
        }
        return response.json();
      })
      .then(data => {
        if (data && data.length > 0) {
          resolve(data);
        } else {
          reject("Address not found");
        }
      })
      .catch(error => {
        console.error("Error searching for address:", error);
        reject(error);
      });
  });
}

/**
 * Reverse geocode coordinates to address
 * @param {number} lat - Latitude
 * @param {number} lng - Longitude
 * @returns {Promise<Object>} Address data object
 */
export function reverseGeocode(lat, lng) {
  return new Promise((resolve, reject) => {
    fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`)
      .then(response => {
        if (!response.ok) {
          throw new Error("Error reverse geocoding");
        }
        return response.json();
      })
      .then(data => {
        if (data && data.display_name) {
          resolve(data);
        } else {
          reject("No address found for these coordinates");
        }
      })
      .catch(error => {
        console.error("Error reverse geocoding:", error);
        reject(error);
      });
  });
}

/**
 * Extract city and postal code from address data
 * @param {Object} addressData - Address data from reverse geocode
 * @returns {Object} Object with city and postalCode properties
 */
export function extractCityAndPostalCode(addressData) {
  if (!addressData || !addressData.address) {
    return { city: "", postalCode: "" };
  }
  
  const address = addressData.address;
  let city = address.city || address.town || address.village || address.hamlet || "";
  let postalCode = address.postcode || "";
  
  return { city, postalCode };
}

/**
 * Format address data to a readable string
 * @param {Object} addressData - Address data from reverse geocode
 * @returns {string} Formatted address string
 */
export function formatAddress(addressData) {
  if (!addressData || !addressData.address) {
    return "";
  }
  
  const address = addressData.address;
  const parts = [];
  
  if (address.road) parts.push(address.road);
  if (address.house_number) parts.push(address.house_number);
  if (address.city || address.town || address.village) {
    parts.push(address.city || address.town || address.village);
  }
  if (address.postcode) parts.push(address.postcode);
  if (address.country) parts.push(address.country);
  
  return parts.join(", ");
}
