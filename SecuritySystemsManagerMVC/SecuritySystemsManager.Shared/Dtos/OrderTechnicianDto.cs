using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class OrderTechnicianDto : BaseDto
    {
        public int SecuritySystemOrderId { get; set; }
        public SecuritySystemOrderDto SecuritySystemOrder { get; set; }

        public int TechnicianId { get; set; }
        public UserDto Technician { get; set; }

        public DateTime AssignedAt { get; set; }
    }
} 