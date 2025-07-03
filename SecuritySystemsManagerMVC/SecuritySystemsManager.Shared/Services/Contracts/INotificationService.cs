using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface INotificationService : IBaseCrudService<NotificationDto, INotificationRepository>
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<bool> SendOrderStatusChangeNotificationAsync(int orderId, int clientId, OrderStatus oldStatus, OrderStatus newStatus);
    }
} 