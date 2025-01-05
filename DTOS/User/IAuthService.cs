using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message)> RegisterUser(RegisterModel model);
        Task<(string? Token, DateTime? Expiration, List<string>? Roles, string? Error)> LoginUser(LoginModel model);
    }
}
