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
                "InstalledDeviceDto" => "Installed Device",
                "InvoiceDto" => "Invoice",
                "LocationDto" => "Location",
                "MaintenanceDeviceDto" => "Maintenance Device",
                "MaintenanceLogDto" => "Maintenance Log",
                "NotificationDto" => "Notification",
                "OrderTechnicianDto" => "Order Technician",
                "RoleDto" => "Role",
                "SecuritySystemOrderDto" => "Security System Order",
                "UserDto" => "User",
                _ => typeName
            };
        }
    }
}
