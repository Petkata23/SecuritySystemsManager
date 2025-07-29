$(document).ready(function () {
    // Add event listener for image load error
    $('img').on('error', function() {
        console.error('Image failed to load:', this.src);
    });
}); 