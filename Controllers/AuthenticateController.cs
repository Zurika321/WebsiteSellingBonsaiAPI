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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WebsiteSellingBonsaiAPI.DTOS.View;

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
                return BadRequest(new { Message = "Payload không hợp lệ." });/*(new ResponseModel { Status = "Error", Message = "Payload không hợp lệ." });*/
            }
            var (isSuccess, message) = await _authService.RegisterUser(model);

            if (!isSuccess)
            {
                return StatusCode(500, new { Message = message });/*new ResponseModel { Status = "Error", Message = message });*/
            }

            return Ok(new { Message = message }); /*new ResponseModel { Status = "Success", Message = message });*/
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
            //return Ok(new {token = token , roles = roles , expiration = expiration});
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

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new ApplicationUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address == "Không có địa chỉ" ? "" : user.Address,
                    Avatar = user.Avatar,
                    CreatedDate = user.CreatedDate,
                    Role = roles,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserInfo: {ex.Message}");
                return StatusCode(500, new { Message = "Lỗi không xác định." });
            }
        }
        [HttpGet("getlistusers")]
        public async Task<ActionResult<IEnumerable<BonsaiDTO>>> GetListUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();

                var userDtos = new List<ApplicationUserDTO>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    userDtos.Add(new ApplicationUserDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address == "Không có địa chỉ" ? "" : user.Address,
                        Avatar = user.Avatar,
                        CreatedDate = user.CreatedDate,
                        Role = roles,
                    });
                }

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetListUsers: {ex.Message}");
                return StatusCode(500, new { Message = "Lỗi không xác định." });
            }
        }
        [HttpGet("getuserinfobyid")]
        public async Task<IActionResult> GetUserInfoByID(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new ApplicationUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address == "Không có địa chỉ" ? "" : user.Address,
                    Avatar = user.Avatar,
                    CreatedDate = user.CreatedDate,
                    Role = roles,
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
                return BadRequest(new { Message = "Invalid user ID or token."});
            }

            var user = await _userManager.FindByIdAsync(comfrim.userId);
            if (user == null)
            {
                return NotFound(new { Message = "Người dùng không tồn tại."});
            }

            if(user.EmailConfirmed == true)
            {
                return BadRequest(new { Message = "Email đã được xác nhận trước đó." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, comfrim.token);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Xác thực email thành công."});
            }
            return StatusCode(500, new { Message = "Xác thực email thất bại" });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            
            var (issuccess, mes) = await _authService.ForgotPassword(email);

            if (!issuccess) {
                return BadRequest(new { Message = mes });
            }
            return Ok(new { Message = mes });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            if (resetPassword.newpassword != resetPassword.Comfirmpassword) { 
                return BadRequest( new { Message = "mật khẩu mới và mật khẩu xác nhận không giống nhau"});
            }
            var decodedBytes = WebEncoders.Base64UrlDecode(resetPassword.token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var user = await _userManager.FindByIdAsync(resetPassword.userid);
            if (user == null)
            {
                return NotFound(new { Message = "Người dùng không tồn tại." });
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPassword.newpassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Đổi mật khẩu thành công" });
            }
            else
            {
                return BadRequest(new { Message = "Đổi mật khẩu không thành công" });

            }
        }

        [HttpPost("changeAvatar")]
        public async Task<IActionResult> ChangeAvatar([FromBody]string newAvatar)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Bạn cần đăng nhập để thay đổi avatar." });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                if (string.IsNullOrEmpty(newAvatar))
                {
                    return BadRequest(new { Message = "Vui lòng chọn một file avatar." });
                }

                user.Avatar = newAvatar;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return StatusCode(500, new { Message = "Không thể cập nhật avatar." });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new ApplicationUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address == "Không có địa chỉ" ? "" : user.Address,
                    Avatar = user.Avatar,
                    CreatedDate = user.CreatedDate,
                    Role = roles,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChangeAvatar: {ex.Message}");
                return StatusCode(500, new { Message = "Lỗi không xác định khi thay đổi avatar." });
            }
        }
        [HttpPost("changeInformation")]
        public async Task<IActionResult> changeInformation([FromBody] ChangeInformation ci)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Message = "Bạn cần đăng nhập để thay đổi thông tin." });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                if (!string.IsNullOrEmpty(ci.Phone) && (ci.Phone.Length != 10 || !ci.Phone.All(char.IsDigit)))
                {
                    return BadRequest(new { Message = "Số điện thoại không đúng định dạng 10 số." });
                }

                user.Address = ci.Address;
                user.PhoneNumber = ci.Phone;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return StatusCode(500, new { Message = "Không thể cập nhật địa chỉ và sđt." });
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(new ApplicationUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address == "Không có địa chỉ" ? "" : user.Address,
                    Avatar = user.Avatar,
                    CreatedDate = user.CreatedDate,
                    Role = roles,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChangeInformation: {ex.Message}");
                return StatusCode(500, new { Message = "Lỗi không xác định khi thay đổi địa chỉ và sđt." });
            }
        }
    }
}
