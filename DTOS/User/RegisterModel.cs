using System.Text.Json.Serialization;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public required string Password { get; set; }
        public string? ComfrimPassword { get; set; }
    }
}
