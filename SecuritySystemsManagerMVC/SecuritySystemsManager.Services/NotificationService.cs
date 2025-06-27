using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class NotificationService : BaseCrudService<NotificationDto, INotificationRepository>, INotificationService
    {
        public NotificationService(INotificationRepository repository) : base(repository)
        {
        }
    }
} 