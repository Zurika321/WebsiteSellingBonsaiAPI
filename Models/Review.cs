using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Reviews")]
    public class Review : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int REVIEW_ID { get; set; }

        [ForeignKey("Bonsai")]
        public int BONSAI_ID { get; set; }

        [ForeignKey("ApplicationUser")]
        public string USE_ID { get; set; }

        [Range(1, 5, ErrorMessage = "Chỉ nhận từ 1 đến 5")]
        public double Rate { get; set; }
        [MaxLength(2048)]
        public string? Comment { get; set; }

        public Bonsai Bonsai { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
