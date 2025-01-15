using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS.Review
{
    public class AddReview
    {
        public string comment { get; set; }
        public string bonsai_id { get; set; }
        [Range(1, 5)]
        public double rate { get; set; }
    }
}
