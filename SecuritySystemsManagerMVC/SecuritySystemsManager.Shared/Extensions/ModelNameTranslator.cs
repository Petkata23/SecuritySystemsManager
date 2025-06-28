using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Extensions
{
    public static class ModelNameTranslator
    {
        public static string ToFriendlyName(this string typeName)
        {
            return typeName switch
            {
                "InstalledDeviceDto" => "Инсталираното устройство",
                "InvoiceDto" => "Фактурата",
                "LocationDto" => "Локацията",
                "MaintenanceDeviceDto" => "Устройството за поддръжка",
                "MaintenanceLogDto" => "Записът за поддръжка",
                "NotificationDto" => "Известието",
                "OrderTechnicianDto" => "Техникът по поръчката",
                "RoleDto" => "Ролята",
                "SecuritySystemOrderDto" => "Поръчката за охранителна система",
                "UserDto" => "Потребителят",
                _ => typeName
            };
        }
    }
}
