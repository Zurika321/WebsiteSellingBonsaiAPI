﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        public int ORDER_D_ID { get; set; }
        [ForeignKey("Order")]
        public int ORDER_ID { get; set; }
        [ForeignKey("Bonsai")]
        public int BONSAI_ID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;

        public virtual Order Orders { get; set; }
        public virtual Bonsai Bonsais { get; set; }

    }
}