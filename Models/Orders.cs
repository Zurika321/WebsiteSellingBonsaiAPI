using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Orders")]
    public class Order : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ORDER_ID { get; set; }

        [ForeignKey("AdminUser")]
        public int USE_ID { get; set; }

        [StringLength(100)]
        public string PaymentMethod { get; set; }

        [StringLength(24)]
        public string Status { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total must be a positive value.")]
        public decimal Total { get; set; }

        public virtual AdminUser AdminUser { get; set; }
    }
}