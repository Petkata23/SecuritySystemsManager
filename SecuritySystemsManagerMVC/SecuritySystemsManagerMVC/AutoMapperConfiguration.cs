using AutoMapper;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManagerMVC.ViewModels;

namespace SecuritySystemsManagerMVC
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<LoginDto, LoginVm>().ReverseMap();
            CreateMap<RegisterVm, UserDto>();

            CreateMap<InstalledDevice, InstalledDeviceDto>().ReverseMap();
            CreateMap<InstalledDeviceDto, InstalledDeviceDetailsVm>().ReverseMap();
            CreateMap<InstalledDeviceDto, InstalledDeviceEditVm>().ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash));
                
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Username.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email != null ? src.Email.ToUpper() : null));
                
            CreateMap<UserDto, UserDetailsVm>().ReverseMap();
            CreateMap<UserDto, UserEditVm>().ReverseMap();

            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<RoleDto, RoleDetailsVm>().ReverseMap();
            CreateMap<RoleDto, RoleEditVm>().ReverseMap();

            CreateMap<Location, LocationDto>().ReverseMap();
            CreateMap<LocationDto, LocationDetailsVm>()
                .ForMember(dest => dest.Orders, opt => opt.MapFrom(src => src.Orders));
            CreateMap<LocationDetailsVm, LocationDto>();
            CreateMap<LocationDto, LocationEditVm>().ReverseMap();

            CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>().ReverseMap();
            CreateMap<SecuritySystemOrderDto, SecuritySystemOrderDetailsVm>().ReverseMap();
            CreateMap<SecuritySystemOrderDto, SecuritySystemOrderEditVm>().ReverseMap();

            CreateMap<MaintenanceDevice, MaintenanceDeviceDto>().ReverseMap();
            CreateMap<MaintenanceDeviceDto, MaintenanceDeviceDetailsVm>()
                .ForMember(dest => dest.InstalledDeviceName, opt => opt.MapFrom(src => 
                    src.InstalledDevice != null ? $"{src.InstalledDevice.Brand} {src.InstalledDevice.Model} - {src.InstalledDevice.DeviceType}" : "Unknown Device"))
                .ForMember(dest => dest.MaintenanceLogDate, opt => opt.MapFrom(src => 
                    src.MaintenanceLog != null ? src.MaintenanceLog.Date.ToString("MM/dd/yyyy") : "Unknown Date"));
            CreateMap<MaintenanceDeviceDetailsVm, MaintenanceDeviceDto>()
                .ForMember(dest => dest.InstalledDevice, opt => opt.Ignore())
                .ForMember(dest => dest.MaintenanceLog, opt => opt.Ignore());
            CreateMap<MaintenanceDeviceDto, MaintenanceDeviceEditVm>().ReverseMap();

            CreateMap<MaintenanceLog, MaintenanceLogDto>().ReverseMap();
            CreateMap<MaintenanceLogDto, MaintenanceLogDetailsVm>()
                .ForMember(dest => dest.OrderTitle, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.Title : "Unknown Order"))
                .ForMember(dest => dest.TechnicianFullName, opt => opt.MapFrom(src => 
                    src.Technician != null ? $"{src.Technician.FirstName} {src.Technician.LastName}" : "Unknown Technician"));
            CreateMap<MaintenanceLogDto, MaintenanceLogEditVm>().ReverseMap();

            CreateMap<Invoice, InvoiceDto>().ReverseMap();
            CreateMap<InvoiceDto, InvoiceDetailsVm>()
                .ForMember(dest => dest.OrderTitle, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.Title : "Unknown Order"))
                .ForMember(dest => dest.SecuritySystemOrderId, opt => opt.MapFrom(src => src.SecuritySystemOrderId))
                .ForMember(dest => dest.ClientFullName, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null && src.SecuritySystemOrder.Client != null ? 
                    $"{src.SecuritySystemOrder.Client.FirstName} {src.SecuritySystemOrder.Client.LastName}" : "Unknown Client"))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null && src.SecuritySystemOrder.Client != null ? 
                    src.SecuritySystemOrder.Client.Email : "N/A"))
                .ForMember(dest => dest.ClientPhone, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.PhoneNumber : "N/A"))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null && src.SecuritySystemOrder.Location != null ? 
                    src.SecuritySystemOrder.Location.Name : "N/A"))
                .ForMember(dest => dest.LocationAddress, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null && src.SecuritySystemOrder.Location != null ? 
                    src.SecuritySystemOrder.Location.Address : "N/A"))
                .ForMember(dest => dest.OrderDescription, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.Description : "N/A"))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.Status.ToString() : "N/A"))
                .ForMember(dest => dest.OrderRequestedDate, opt => opt.MapFrom(src => 
                    src.SecuritySystemOrder != null ? src.SecuritySystemOrder.RequestedDate : DateTime.Now));
            CreateMap<InvoiceDetailsVm, InvoiceDto>();
            CreateMap<InvoiceDto, InvoiceEditVm>().ReverseMap();

            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<NotificationDto, NotificationDetailsVm>().ReverseMap();
            CreateMap<NotificationDto, NotificationEditVm>().ReverseMap();

            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => 
                    src.Sender != null ? src.Sender.UserName : src.SenderName))
                .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src => 
                    src.Recipient != null ? src.Recipient.UserName : src.RecipientName))
                .ReverseMap()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.Recipient, opt => opt.Ignore());
                
            CreateMap<ChatConversationDto, ChatConversationDto>().ReverseMap();
        }
    }
}
