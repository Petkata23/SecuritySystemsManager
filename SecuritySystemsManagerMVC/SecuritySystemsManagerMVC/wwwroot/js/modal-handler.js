/**
 * Simple Modal Handler
 * A lightweight jQuery plugin to handle Bootstrap modals
 */

(function($) {
    $.fn.simpleModal = function(options) {
        // Default options
        var settings = $.extend({
            backdrop: true,
            keyboard: true,
            focus: true
        }, options);

        // Return this for chaining
        return this.each(function() {
            var $modal = $(this);
            var $modalBackdrop;
            
            // Methods
            var methods = {
                show: function() {
                    // Clean up any existing modals
                    methods.cleanup();
                    
                    // Create backdrop if needed
                    if (settings.backdrop) {
                        $modalBackdrop = $('<div class="modal-backdrop"></div>');
                        $('body').append($modalBackdrop);
                        setTimeout(function() {
                            $modalBackdrop.addClass('show');
                        }, 10);
                    }
                    
                    // Show modal
                    $modal.css('display', 'block');
                    setTimeout(function() {
                        $modal.addClass('show');
                    }, 10);
                    
                    // Add modal-open class to body
                    $('body').addClass('modal-open');
                    
                    // Focus on modal
                    if (settings.focus) {
                        setTimeout(function() {
                            $modal.find('[autofocus]').first().focus();
                        }, 100);
                    }
                    
                    // Trigger shown event
                    setTimeout(function() {
                        $modal.trigger('shown.bs.modal');
                    }, 300);
                },
                
                hide: function() {
                    // Hide modal
                    $modal.removeClass('show');
                    
                    // Remove backdrop
                    if ($modalBackdrop) {
                        $modalBackdrop.removeClass('show');
                    }
                    
                    // Complete hide after animation
                    setTimeout(function() {
                        $modal.css('display', 'none');
                        if ($modalBackdrop) {
                            $modalBackdrop.remove();
                        }
                        
                        // Remove modal-open class if no other modals are open
                        if ($('.modal.show').length === 0) {
                            $('body').removeClass('modal-open');
                        }
                        
                        // Trigger hidden event
                        $modal.trigger('hidden.bs.modal');
                    }, 300);
                },
                
                cleanup: function() {
                    // Remove any orphaned backdrops
                    $('.modal-backdrop').each(function() {
                        var $backdrop = $(this);
                        var backdropInUse = false;
                        
                        $('.modal.show').each(function() {
                            if ($(this).data('backdrop-element') === $backdrop[0]) {
                                backdropInUse = true;
                                return false;
                            }
                        });
                        
                        if (!backdropInUse) {
                            $backdrop.remove();
                        }
                    });
                }
            };
            
            // Store methods on the element
            $modal.data('simpleModal', methods);
            
            // Set up event handlers
            $modal.find('[data-bs-dismiss="modal"]').on('click', function(e) {
                e.preventDefault();
                methods.hide();
            });
            
            // Handle ESC key
            if (settings.keyboard) {
                $(document).on('keydown.modal', function(e) {
                    if (e.which === 27 && $modal.hasClass('show')) {
                        methods.hide();
                    }
                });
            }
            
            // Handle backdrop click
            if (settings.backdrop === true) {
                $modal.on('click', function(e) {
                    if ($(e.target).is($modal)) {
                        methods.hide();
                    }
                });
            }
        });
    };
    
    // Auto-initialize modals with data-simple-modal attribute
    $(document).ready(function() {
        $('[data-simple-modal]').each(function() {
            $(this).simpleModal();
        });
    });
})(jQuery); 