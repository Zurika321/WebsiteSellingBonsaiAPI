using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class BannerDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BAN_ID { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1024)]
        public string? ImageOld { get; set; }
        public IFormFile? Image { get; set; }
    }
}
