using System.Text.Json.Serialization;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        //[JsonPropertyName("FullName")]
        //public string FullName { get; set; }
    }
}
