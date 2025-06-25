using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class NotificationDto : BaseDto
    {
        public int RecipientId { get; set; }
        public UserDto Recipient { get; set; }

        public string Message { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsRead { get; set; }
    }
} 