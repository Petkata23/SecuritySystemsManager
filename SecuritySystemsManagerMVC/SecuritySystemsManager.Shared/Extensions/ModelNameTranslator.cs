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
                "InstalledDeviceDto" => "Инсталирано устройство",
                "InvoiceDto" => "Фактура",
                "LocationDto" => "Локация",
                "MaintenanceDeviceDto" => "Устройство за поддръжка",
                "MaintenanceLogDto" => "Дневник за поддръжка",
                "NotificationDto" => "Известие",
                "OrderTechnicianDto" => "Техник по поръчка",
                "RoleDto" => "Роля",
                "SecuritySystemOrderDto" => "Поръчка за система за сигурност",
                "UserDto" => "Потребител",
                _ => typeName
            };
        }
    }
}
