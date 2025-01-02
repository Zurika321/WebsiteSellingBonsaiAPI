using System.ComponentModel.DataAnnotations;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class FeatureDTO : BaseModel
    {
        [Key]
        public int FEA_ID { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string Description { get; set; }
    }
}
