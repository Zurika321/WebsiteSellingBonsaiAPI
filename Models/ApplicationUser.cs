using Microsoft.AspNetCore.Identity;
using System.Net.Sockets;

namespace WebsiteSellingBonsaiAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Address { get; set; }
    }
}
