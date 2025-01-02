using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(RegisterModel model);
        Task<string> LoginUser(LoginModel model);
    }
}
