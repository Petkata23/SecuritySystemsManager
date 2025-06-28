using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class NotificationController : BaseCrudController<NotificationDto, INotificationRepository, INotificationService, NotificationEditVm, NotificationDetailsVm>
    {
        public NotificationController(IMapper mapper, INotificationService service)
            : base(service, mapper)
        {
        }
    }
} 