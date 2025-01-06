using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Carts")]
    public class Cart : BaseModel
    {
        [Key]
        public int CART_ID { get; set; }
        [ForeignKey("AdminUser")]
        public string USE_ID { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public ICollection<CartDetail>? CartDetails { get; set; }
    }
}
