using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Dtos
{
    public class InvoiceDto : BaseDto
    {
        public int SecuritySystemOrderId { get; set; }
        public SecuritySystemOrderDto SecuritySystemOrder { get; set; }

        public DateTime IssuedOn { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
    }
} 