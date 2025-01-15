using Microsoft.EntityFrameworkCore.Migrations;
using System.ComponentModel.DataAnnotations;

namespace WebsiteSellingBonsaiAPI.DTOS.View
{
    public class AdminUserDTO
    {
        [Key]
        public int USE_ID { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Displayname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? AvatarOld { get; set; }
        public string? Role { get; set; }
        public string? Address { get; set; }
    }
}
