namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class LoginModel
    {
        public required string Password { get; set; }
        public required string Username { get; set; }
        public bool RememberMe { get; set; }
    }
}
