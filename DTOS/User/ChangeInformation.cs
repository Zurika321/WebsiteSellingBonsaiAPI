using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class ChangeInformation
    {
        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(10)]
        public string? Phone { get; set; }
    }
}
