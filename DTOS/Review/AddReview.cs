using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS.Review
{
    public class AddReview
    {
        public string? comment { get; set; }
        public int bonsai_id { get; set; }
        [Range(1, 5)]
        public int rate { get; set; }
    }
}
