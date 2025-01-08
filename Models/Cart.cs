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
        public string USE_ID { get; set; }
        [ForeignKey("USE_ID")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<CartDetail> CartDetails { get; set; }
    }
}
