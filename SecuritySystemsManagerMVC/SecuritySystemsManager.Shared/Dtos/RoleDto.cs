using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class RoleDto : BaseDto
    {
        public RoleDto()
        {
            Users = new List<UserDto>();
        }

        public string Name { get; set; }
        public RoleType RoleType { get; set; }

        public List<UserDto> Users { get; set; }
    }
} 