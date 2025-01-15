using System.ComponentModel.DataAnnotations;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.View
{
    public class FeatureDTO : BaseModel
    {
        [Key]
        public int FEA_ID { get; set; }
        [MaxLength(50)]
        [Required]
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        [MaxLength(500)]
        [Required]
        public string Description { get; set; }
        [MaxLength(200)]
        [Required]
        public string Link { get; set; }
    }
}
