using Microsoft.AspNetCore.Identity;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class ApplicationUserDTO : IdentityUser
    {
        public string Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Address { get; set; }
        public ICollection<string> Role { get; set; }
    }
}
