using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthenticateController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var (isSuccess, message) = await _authService.RegisterUser(model);

            if (!isSuccess)
            {
                return StatusCode(500, new ResponseModel { Status = "Error", Message = message });
            }

            return Ok(new ResponseModel { Status = "Success", Message = message });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var (token, expiration, roles, error) = await _authService.LoginUser(model);

            if (error != null)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = error });
            }
            return Ok(new ResponseModel { Status = "Succes", Message = token });
        }

        [HttpGet]
        [Route("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userName = User.Identity?.Name;
                Console.WriteLine($"User.Identity.Name: {userName}");

                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Không thể lấy thông tin người dùng." });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    Console.WriteLine($"User not found: {userName}");
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                Console.WriteLine($"Found user: {user.UserName}");

                return Ok(new ApplicationUser
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Avatar = user.Avatar,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserInfo: {ex.Message}");
                return StatusCode(500, new { Message = "Lỗi không xác định." });
            }
        }


    }
}
