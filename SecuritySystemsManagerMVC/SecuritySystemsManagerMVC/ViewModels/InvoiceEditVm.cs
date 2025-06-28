using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SecuritySystemsManagerMVC.ViewModels
{
    public class InvoiceEditVm : BaseVm
    {
        [Required(ErrorMessage = "Order is required")]
        [DisplayName("Security System Order")]
        public int SecuritySystemOrderId { get; set; }
        public IEnumerable<SelectListItem> AllOrders { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DisplayName("Issued On")]
        [DataType(DataType.Date)]
        public DateTime IssuedOn { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DisplayName("Total Amount")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Is Paid")]
        public bool IsPaid { get; set; }

        public InvoiceEditVm()
        {
            AllOrders = new List<SelectListItem>();
        }
    }
}
