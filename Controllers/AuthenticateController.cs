using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
using NuGet.Common;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.DTOS.Constants;

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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null)
            {
                return BadRequest("Payload không hợp lệ.");
            }
            var (isSuccess, message) = await _authService.RegisterUser(model);

            if (!isSuccess)
            {
                return StatusCode(500, new ResponseModel { Status = "Error", Message = message });
            }

            return Ok(new ResponseModel { Status = "Success", Message = message });
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var (token, expiration, roles, error) = await _authService.LoginUser(model);

            if (error != null)
            {
                return Unauthorized(new ResponseModel { Status = "Error", Message = error });
            }
            return Ok(new ResponseModel { Status = "Success", Message = token });
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userName = User.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Không thể lấy thông tin người dùng." });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

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
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmail comfrim)
        {
            if (comfrim != null && string.IsNullOrEmpty(comfrim.userId) || string.IsNullOrEmpty(comfrim.token))
            {
                return BadRequest("Invalid user ID or token.");
            }

            var user = await _userManager.FindByIdAsync(comfrim.userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, comfrim.token);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Xác thực email thành công."});
            }
            return StatusCode(500, new { Message = "Xác thực email thất bại" });
        }
    }
}
