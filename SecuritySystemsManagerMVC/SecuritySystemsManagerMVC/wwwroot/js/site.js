// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Initialize Bootstrap components
document.addEventListener('DOMContentLoaded', function() {
    // Fix for multiple modal backdrops
    const removeExtraBackdrops = () => {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        if (backdrops.length > 1) {
            for (let i = 1; i < backdrops.length; i++) {
                backdrops[i].remove();
            }
        }
    };
    
    // Clean up any stray modal-open classes or backdrops on page load
    document.body.classList.remove('modal-open');
    removeExtraBackdrops();

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Initialize alerts
    var alertList = [].slice.call(document.querySelectorAll('.alert'));
    alertList.map(function (alertEl) {
        return new bootstrap.Alert(alertEl);
    });

    // Initialize dropdowns
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });

    // Initialize modals
    var modalTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="modal"]'));
    modalTriggerList.map(function (modalTriggerEl) {
        modalTriggerEl.addEventListener('click', function(event) {
            var targetModalSelector = this.getAttribute('data-bs-target');
            if (targetModalSelector) {
                var targetModal = document.querySelector(targetModalSelector);
                if (targetModal) {
                    // Check if this modal already has a Bootstrap modal instance
                    var existingModal = bootstrap.Modal.getInstance(targetModal);
                    if (existingModal) {
                        existingModal.show();
                    } else {
                        var modal = new bootstrap.Modal(targetModal, {
                            backdrop: true,
                            keyboard: true,
                            focus: true
                        });
                        modal.show();
                    }
                    
                    // Fix for multiple backdrops
                    setTimeout(removeExtraBackdrops, 50);
                }
            }
        });
    });

    // Add event listener for all modals when they're hidden
    document.querySelectorAll('.modal').forEach(function(modalEl) {
        modalEl.addEventListener('hidden.bs.modal', function() {
            removeExtraBackdrops();
            
            // If no modals are visible, remove modal-open class from body
            if (document.querySelectorAll('.modal.show').length === 0) {
                document.body.classList.remove('modal-open');
            }
        });
    });

    // Close button for alerts
    var closeButtons = document.querySelectorAll('.alert .btn-close');
    closeButtons.forEach(function(button) {
        button.addEventListener('click', function() {
            this.closest('.alert').classList.add('d-none');
        });
    });
});

// Security Systems Hristovi - Main JavaScript

// Document Ready Function
$(document).ready(function() {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Active navigation item
    highlightActiveNavItem();
    
    // Back to top button
    initBackToTop();
    
    // Animate on scroll
    initAOS();
    
    // Form validation
    initFormValidation();
    
    // Notification badge
    updateNotificationBadge();
    
    // Handle alerts dismissal
    setTimeout(function() {
        $('.alert-dismissible').fadeOut('slow');
    }, 5000);
    
    // Animate elements on scroll
    const animateElements = document.querySelectorAll('.animate-on-scroll');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animated');
            }
        });
    }, { threshold: 0.1 });
    
    animateElements.forEach(element => {
        observer.observe(element);
    });

    // Mobile navigation enhancements
    initMobileNavigation();

    // Notifications handling
    // Mark all notifications as read
    $('.mark-all-read').on('click', function(e) {
        e.preventDefault();
        
        $.ajax({
            url: '/Notification/MarkAllAsRead',
            type: 'POST',
            success: function(response) {
                if (response.success) {
                    // Remove unread styling and badge
                    $('.notification-item').removeClass('unread');
                    $('.notification-dropdown .badge').remove();
                }
            }
        });
    });
    
    // Mark individual notification as read
    $('.notification-item').on('click', function() {
        const notificationId = $(this).data('id');
        
        if ($(this).hasClass('unread')) {
            $.ajax({
                url: '/Notification/MarkAsRead',
                type: 'POST',
                data: { id: notificationId },
                success: function(response) {
                    if (response.success) {
                        // Update UI
                        updateNotificationCount();
                    }
                }
            });
        }
    });
    
    function updateNotificationCount() {
        // Count unread notifications
        const unreadCount = $('.notification-item.unread').length - 1;
        
        // Update badge
        if (unreadCount <= 0) {
            $('.notification-dropdown .badge').remove();
        } else {
            const badgeText = unreadCount > 9 ? '9+' : unreadCount;
            $('.notification-dropdown .badge').text(badgeText);
        }
    }
    
    // Debug images
    debugImages();
});

// Mobile navigation enhancements
function initMobileNavigation() {
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    const body = document.body;
    const isSmallScreen = () => window.innerWidth < 992;
    
    // Handle body scroll lock when menu opens/closes
    if (navbarCollapse) {
        navbarCollapse.addEventListener('show.bs.collapse', function() {
            body.classList.add('menu-open');
        });
        
        navbarCollapse.addEventListener('hide.bs.collapse', function() {
            body.classList.remove('menu-open');
        });
    }
    
    // Close navbar when clicking on a nav item on mobile (except dropdowns)
    document.querySelectorAll('.navbar-nav .nav-link:not(.dropdown-toggle)').forEach(function(link) {
        link.addEventListener('click', function() {
            if (isSmallScreen() && navbarCollapse && navbarCollapse.classList.contains('show')) {
                // Use Bootstrap's collapse API
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) {
                    bsCollapse.hide();
                }
            }
        });
    });

    // Handle window resize
    window.addEventListener('resize', function() {
        if (!isSmallScreen() && navbarCollapse) {
            // Reset menu state when resizing to desktop
            if (navbarCollapse.classList.contains('show')) {
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) {
                    bsCollapse.hide();
                }
            }
            body.classList.remove('menu-open');
        }
    });
}

// Highlight active navigation item
function highlightActiveNavItem() {
    const currentUrl = window.location.pathname;
    $('.navbar-nav .nav-link').each(function() {
        const href = $(this).attr('href');
        if (href && currentUrl.includes(href) && href !== '/') {
            $(this).addClass('active');
        } else if (currentUrl === '/' && href === '/') {
            $(this).addClass('active');
        }
    });
}

// Back to top button
function initBackToTop() {
    $(window).scroll(function() {
        if ($(this).scrollTop() > 200) {
            $('.back-to-top').addClass('active');
        } else {
            $('.back-to-top').removeClass('active');
        }
    });
    
    $('.back-to-top').click(function(e) {
        e.preventDefault();
        $('html, body').animate({scrollTop: 0}, 800);
        return false;
    });
}

// Animate on scroll
function initAOS() {
    // Add animation classes to elements with data-animate attribute
    $('[data-animate]').each(function() {
        const element = $(this);
        const animation = element.data('animate');
        const delay = element.data('delay') || 0;
        
        element.css('opacity', '0');
        
        $(window).scroll(function() {
            const elementTop = element.offset().top;
            const elementBottom = elementTop + element.outerHeight();
            const viewportTop = $(window).scrollTop();
            const viewportBottom = viewportTop + $(window).height();
            
            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                setTimeout(function() {
                    element.css('opacity', '1').addClass(animation);
                }, delay);
            }
        });
        
        // Trigger scroll event to check for visible elements on page load
        $(window).trigger('scroll');
    });
}

// Form validation
function initFormValidation() {
    // Get all forms with the class 'needs-validation'
    const forms = document.querySelectorAll('.needs-validation');
    
    // Loop over them and prevent submission
    Array.prototype.slice.call(forms).forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            
            form.classList.add('was-validated');
        }, false);
    });
}

// Update notification badge
function updateNotificationBadge() {
    // This would typically be an AJAX call to get the notification count
    // For demonstration, we'll just set a random number
    const count = Math.floor(Math.random() * 5);
    
    if (count > 0) {
        $('.notification-badge').text(count).show();
    } else {
        $('.notification-badge').hide();
    }
}

// Toggle password visibility
function togglePasswordVisibility(inputId, iconId) {
    const passwordInput = document.getElementById(inputId);
    const icon = document.getElementById(iconId);
    
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash');
    } else {
        passwordInput.type = 'password';
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye');
    }
}

// Format currency
function formatCurrency(amount, currency = 'BGN') {
    return new Intl.NumberFormat('bg-BG', {
        style: 'currency',
        currency: currency
    }).format(amount);
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('bg-BG', { 
        year: 'numeric', month: 'long', day: 'numeric' 
    });
}

// Confirm action
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Show toast notification
function showToast(message, type = 'success') {
    // Implementation depends on the toast library you're using
}

// Debounce function for search inputs etc.
function debounce(func, wait = 300) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Debug images
function debugImages() {
    // Глобален кеш за изображенията, който ще се запази между навигациите
    window.imageCache = window.imageCache || {};
    
    // Функция за предварително зареждане на изображение
    function preloadImage(src) {
        if (!src || src.includes('favicon.svg')) {
            return Promise.resolve(null);
        }
        
        if (window.imageCache[src]) {
            return Promise.resolve(window.imageCache[src]);
        }
        
        return new Promise((resolve, reject) => {
            const img = new Image();
            
            img.onload = () => {
                window.imageCache[src] = img;
                resolve(img);
            };
            
            img.onerror = (e) => {
                console.error(`Failed to preload image: ${src}`, e);
                reject(e);
            };
            
            // Добавяме случаен параметър, за да избегнем кеширане от браузъра
            img.src = src.includes('?') ? src + '&_t=' + new Date().getTime() : src + '?_t=' + new Date().getTime();
        });
    }
    
    // Функция за прилагане на кеширани изображения
    function applyCachedImages() {
        const profileImages = document.querySelectorAll('.profile-pic');
        
        profileImages.forEach(img => {
            const src = img.getAttribute('data-original-src') || img.getAttribute('src');
            
            // Запазваме оригиналния URL
            if (!img.getAttribute('data-original-src')) {
                img.setAttribute('data-original-src', src);
            }
            
            if (window.imageCache[src]) {
                // Ако изображението е кеширано, го прилагаме веднага
                img.src = window.imageCache[src].src;
                img.classList.add('image-loaded');
            } else {
                // Иначе зареждаме изображението
                preloadImage(src)
                    .then(cachedImg => {
                        if (cachedImg) {
                            img.src = cachedImg.src;
                            img.classList.add('image-loaded');
                        }
                    })
                    .catch(() => {
                        // При грешка, показваме изображението по подразбиране
                        img.src = '/img/favicon.svg';
                    });
            }
        });
    }
    
    // Прилагаме кеширани изображения при зареждане на страницата
    applyCachedImages();
    
    // Прилагаме кеширани изображения след всяка AJAX заявка
    $(document).ajaxComplete(function() {
        setTimeout(applyCachedImages, 100);
    });
    
    // Прилагаме кеширани изображения при всяка промяна в DOM
    const observer = new MutationObserver(function(mutations) {
        setTimeout(applyCachedImages, 100);
    });
    
    observer.observe(document.body, { 
        childList: true, 
        subtree: true 
    });
}
