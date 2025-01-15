using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Features")]
    public class Feature : BaseModel
    {
        [Key]
        public int FEA_ID { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }

}
