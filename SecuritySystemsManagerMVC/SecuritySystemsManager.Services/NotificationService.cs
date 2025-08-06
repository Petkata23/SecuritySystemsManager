using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class NotificationService : BaseCrudService<NotificationDto, INotificationRepository>, INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(INotificationRepository repository, IUserRepository userRepository) : base(repository)
        {
            _notificationRepository = repository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(int userId)
        {
            // Assuming we have a method in the repository to get notifications for a user
            // If not, we can filter them here
            var allNotifications = await _notificationRepository.GetAllAsync();
            return allNotifications.Where(n => n.RecipientId == userId)
                                  .OrderByDescending(n => n.CreatedAt);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            
            // Verify the notification belongs to the user and is not already read
            if (notification != null && notification.RecipientId == userId && !notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.SaveAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var userNotifications = await GetNotificationsForUserAsync(userId);
            
            foreach (var notification in userNotifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                await _notificationRepository.SaveAsync(notification);
            }
        }

        public async Task<bool> SendOrderStatusChangeNotificationAsync(int orderId, int clientId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            try
            {
                var client = await _userRepository.GetByIdAsync(clientId);
                if (client == null)
                    return false;

                string message = GetOrderStatusChangeMessage(orderId, oldStatus, newStatus);
                
                var notification = new NotificationDto
                {
                    RecipientId = clientId,
                    Message = message,
                    DateSent = DateTime.Now,
                    IsRead = false
                };
                
                await SaveAsync(notification);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetOrderStatusChangeMessage(int orderId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            return $"Order #{orderId} status has changed from {oldStatus} to {newStatus}.";
        }
    }
} 