using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecuritySystemsManager.Data.Entities
{
    public class Invoice : BaseEntity
    {
        public int SecuritySystemOrderId { get; set; }
        public virtual SecuritySystemOrder SecuritySystemOrder { get; set; }

        public DateTime IssuedOn { get; set; }
        
        [Range(0, 999999.99, ErrorMessage = "Total amount must be between 0 and 999,999.99")]
        public decimal TotalAmount { get; set; }
        
        public bool IsPaid { get; set; }
    }
}
