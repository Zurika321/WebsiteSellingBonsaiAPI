using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("CartDetails")]
    public class CartDetail
    {
        [Key]
        public int CART_D_ID { get; set; }
        [ForeignKey("Cart")]
        public int CART_ID { get; set; }
        [ForeignKey("Bonsai")]
        public int BONSAI_ID { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;

        // Navigation properties
        public virtual Cart Cart { get; set; }
        public virtual Bonsai Bonsai { get; set; }
    }
}

