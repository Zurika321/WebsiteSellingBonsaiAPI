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
        public int USE_ID { get; set; }
        public virtual AdminUser AdminUser { get; set; }
    }
}
