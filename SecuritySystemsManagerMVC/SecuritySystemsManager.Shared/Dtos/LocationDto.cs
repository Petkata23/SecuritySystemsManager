using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class LocationDto : BaseDto
    {
        public LocationDto()
        {
            Orders = new List<SecuritySystemOrderDto>();
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string? Description { get; set; }

        public int ClientId { get; set; }
        public UserDto Client { get; set; }

        public List<SecuritySystemOrderDto> Orders { get; set; }
    }
} 