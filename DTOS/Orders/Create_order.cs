using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS.Orders
{
    public class Create_order
    {
        public string list_bonsai {  get; set; }
        public string list_quantity {  get; set; }
        [StringLength(500)]
        public string Address { get; set; }
    }
}
