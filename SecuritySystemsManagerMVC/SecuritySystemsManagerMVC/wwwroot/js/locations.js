/**
 * Основен файл с функции за работа с локации и карти
 */

// Функция за инициализация на карта
function initMap(elementId, center = [42.6977, 23.3242], zoom = 7) {
    var mapElement = document.getElementById(elementId);
    
    if (!mapElement) {
        console.error(`❌ Грешка: Елемент с id='${elementId}' не е намерен.`);
        return null;
    }
    
    var map = L.map(elementId).setView(center, zoom);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);
    
    // Добавяне на контрол за мащаб
    L.control.scale({
        imperial: false,
        position: 'bottomleft'
    }).addTo(map);
    
    // Принудително обновяване на картата
    setTimeout(() => {
        map.invalidateSize();
    }, 500);
    
    return map;
}

// Функция за добавяне на маркер
function addMarker(map, lat, lng, popupContent = null, draggable = false) {
    if (!map || isNaN(lat) || isNaN(lng)) {
        console.error("❌ Грешка: Невалидни параметри за добавяне на маркер.");
        return null;
    }
    
    var marker = L.marker([lat, lng], { draggable: draggable }).addTo(map);
    
    if (popupContent) {
        marker.bindPopup(popupContent);
    }
    
    return marker;
}

// Функция за търсене на адрес
function searchAddress(query) {
    return new Promise((resolve, reject) => {
        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Грешка при търсене на адрес");
                }
                return response.json();
            })
            .then(data => {
                if (data && data.length > 0) {
                    resolve(data);
                } else {
                    reject("Не е намерен адрес");
                }
            })
            .catch(error => {
                console.error("Грешка при търсене на адрес:", error);
                reject(error);
            });
    });
}

// Функция за обратно геокодиране (координати -> адрес)
function reverseGeocode(lat, lng) {
    return new Promise((resolve, reject) => {
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Грешка при обратно геокодиране");
                }
                return response.json();
            })
            .then(data => {
                if (data && data.display_name) {
                    resolve(data);
                } else {
                    reject("Не е намерен адрес за тези координати");
                }
            })
            .catch(error => {
                console.error("Грешка при обратно геокодиране:", error);
                reject(error);
            });
    });
}

// Функция за извличане на град и пощенски код от адрес
function extractCityAndPostalCode(addressData) {
    if (!addressData || !addressData.address) {
        return { city: "", postalCode: "" };
    }
    
    const address = addressData.address;
    let city = address.city || address.town || address.village || address.hamlet || "";
    let postalCode = address.postcode || "";
    
    return { city, postalCode };
}

// Функция за форматиране на адрес
function formatAddress(addressData) {
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