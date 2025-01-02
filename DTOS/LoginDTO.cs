using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class LoginDTO
    {
        public required string Password { get; set; }
        public string? ComfrimPassword { get; set; }
        public required string Username { get; set; }
        public bool RememberMe { get; set; }
    }
}
