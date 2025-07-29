$(document).ready(function() {
    // Mark notification as read
    $('.mark-read').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const btn = $(this);
        const notificationId = btn.data('id');
        const notificationItem = btn.closest('.notification-item');
        
        $.ajax({
            url: '/Notification/MarkAsRead',
            type: 'POST',
            data: { id: notificationId },
            success: function(response) {
                if (response.success) {
                    notificationItem.removeClass('unread');
                    btn.parent().remove();
                    
                    // Check if there are any unread notifications left
                    if ($('.notification-item.unread').length === 0) {
                        $('#markAllAsRead').remove();
                    }
                }
            }
        });
    });
    
    // Mark all as read
    $('#markAllAsRead').on('click', function(e) {
        e.preventDefault();
        
        $.ajax({
            url: '/Notification/MarkAllAsRead',
            type: 'POST',
            success: function(response) {
                if (response.success) {
                    $('.notification-item').removeClass('unread');
                    $('.mark-read').parent().remove();
                    $('#markAllAsRead').remove();
                }
            }
        });
    });
}); 