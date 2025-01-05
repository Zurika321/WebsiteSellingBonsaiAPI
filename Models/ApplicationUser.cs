using Microsoft.AspNetCore.Identity;

namespace WebsiteSellingBonsaiAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
