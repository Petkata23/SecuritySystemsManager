$(document).ready(function() {
    // Animation for device items
    $(".device-item").each(function(index) {
        $(this).css("animation-delay", (index * 0.1) + "s");
    });
}); 