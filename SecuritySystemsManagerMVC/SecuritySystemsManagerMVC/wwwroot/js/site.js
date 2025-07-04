// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

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
});

// Mobile navigation enhancements
function initMobileNavigation() {
    // Close navbar when clicking on a nav item on mobile
    $('.navbar-nav .nav-link').on('click', function() {
        if (window.innerWidth < 992) {
            $('.navbar-collapse').collapse('hide');
        }
    });

    // Handle dropdowns in mobile navigation
    $('.dropdown-toggle').on('click', function(e) {
        if (window.innerWidth < 992) {
            e.preventDefault();
            e.stopPropagation();
            $(this).next('.dropdown-menu').toggleClass('show');
        }
    });

    // Close other dropdowns when one is opened
    $('.dropdown').on('show.bs.dropdown', function() {
        if (window.innerWidth < 992) {
            $('.dropdown-menu.show').removeClass('show');
        }
    });

    // Close dropdowns when clicking outside
    $(document).on('click', function(e) {
        if (window.innerWidth < 992) {
            if (!$(e.target).closest('.dropdown').length) {
                $('.dropdown-menu.show').removeClass('show');
            }
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
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('bg-BG', options);
}

// Confirm action
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Show toast notification
function showToast(message, type = 'success') {
    toastr[type](message);
}

// Debounce function for search inputs
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

// Search functionality
const handleSearch = debounce(function(searchTerm) {
    // This would typically be an AJAX call to search
    console.log('Searching for:', searchTerm);
});

// Event listener for search input
$(document).on('input', '.search-input', function() {
    handleSearch($(this).val());
});

// Handle mobile menu
$('.navbar-toggler').click(function() {
    if ($(this).hasClass('collapsed')) {
        $(this).removeClass('collapsed');
    } else {
        $(this).addClass('collapsed');
    }
});
