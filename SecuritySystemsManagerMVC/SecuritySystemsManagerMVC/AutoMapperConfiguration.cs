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
            CreateMap<LocationDto, LocationDetailsVm>().ReverseMap();
            CreateMap<LocationDto, LocationEditVm>().ReverseMap();

            CreateMap<SecuritySystemOrder, SecuritySystemOrderDto>().ReverseMap();
            CreateMap<SecuritySystemOrderDto, SecuritySystemOrderDetailsVm>().ReverseMap();
            CreateMap<SecuritySystemOrderDto, SecuritySystemOrderEditVm>().ReverseMap();

            CreateMap<MaintenanceDevice, MaintenanceDeviceDto>().ReverseMap();
            CreateMap<MaintenanceDeviceDto, MaintenanceDeviceDetailsVm>().ReverseMap();
            CreateMap<MaintenanceDeviceDto, MaintenanceDeviceEditVm>().ReverseMap();

            CreateMap<MaintenanceLog, MaintenanceLogDto>().ReverseMap();
            CreateMap<MaintenanceLogDto, MaintenanceLogDetailsVm>().ReverseMap();
            CreateMap<MaintenanceLogDto, MaintenanceLogEditVm>().ReverseMap();

            CreateMap<Invoice, InvoiceDto>().ReverseMap();
            CreateMap<InvoiceDto, InvoiceDetailsVm>().ReverseMap();
            CreateMap<InvoiceDto, InvoiceEditVm>().ReverseMap();

            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<NotificationDto, NotificationDetailsVm>().ReverseMap();
            CreateMap<NotificationDto, NotificationEditVm>().ReverseMap();
        }
    }
}
