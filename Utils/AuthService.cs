using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.DTOS.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Identity.UI.Services;
using NuGet.Common;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace WebsiteSellingBonsaiAPI.Utils
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlService _urlService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            EmailSender emailSender,
            IActionContextAccessor actionContextAccessor,
            IUrlService urlService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _actionContextAccessor = actionContextAccessor;
            _urlService = urlService;
        }

        public (bool, string) IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return (false, "Email không được rỗng.");

            if (!email.Contains("@"))
                return (false, "Email phải chứa ký tự '@'.");

            var parts = email.Split('@');
            if (parts.Length != 2)
                return (false, "Email phải chứa đúng một ký tự '@'.");

            var localPart = parts[0];
            var domainPart = parts[1];

            if (string.IsNullOrWhiteSpace(localPart))
                return (false, "Tên cục bộ của email không được rỗng.");

            if (string.IsNullOrWhiteSpace(domainPart) || domainPart != "gmail.com")
                return (false, "Tên miền mặc định là gmail.com.");

            return (true, "");
        }

        public async Task<(bool IsSuccess, string Message)> RegisterUser([FromForm] RegisterModel model)
        {
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            var userExists = await _userManager.FindByNameAsync(model.Username);
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return (false, "Tên người dùng đã tồn tại!");
            }
            var (IsValid, mes) = IsValidEmail(model.Email);
            if (!IsValid)
            {
                return (false, mes);
            }
            if (emailExists != null)
            {
                if (emailExists.EmailConfirmed)
                {
                    return (false, "Email đã tồn tại!");
                }
                else
                {
                    var deleteResult = await _userManager.DeleteAsync(emailExists);
                    if (!deleteResult.Succeeded)
                    {
                        var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                        return (false, $"Xóa tài khoản cũ chưa xác nhận thất bại: {errors}");
                    }
                }
            }

            // Tạo đối tượng người dùng
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                Address = "Không có địa chỉ",
                Avatar = "Data/usernoimage.png",
                CreatedDate = DateTime.Now,
            };

            // Tạo người dùng với mật khẩu
            var result = await _userManager.CreateAsync(user, model.Password);

            // Kiểm tra xem việc tạo người dùng có thành công không
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"User creation failed: {errors}");
            }

            var actionContext = _actionContextAccessor.ActionContext;
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = _urlService.GenerateUrl(
                                    action: "ConfirmEmail",
                                    controller: "Users",
                                    values: new { userId = user.Id, token },
                                    area: "Admin",
                                    scheme: actionContext.HttpContext.Request.Scheme
            );


            await _emailSender.SendEmailAsync(user.Email,
                "Xác nhận tài khoản",
                $"Vui lòng nhấp vào link này để xác nhận tài khoản: <a href='{confirmationLink}'>Xác nhận tài khoản</a>");

            // Gán vai trò cho người dùng
            await _userManager.AddToRoleAsync(user, "User");

            return (true, "User created successfully!");
        }

        public async Task<(string? Token, DateTime? Expiration, List<string>? Roles, string? Error)> LoginUser(LoginModel model)
        {
            // Kiểm tra xem người dùng có tồn tại không
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return (null, null, null, "Username hoặc password bị sai!");

            // Kiểm tra trạng thái EmailConfirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return (null, null, null, "Email của bạn chưa được xác nhận. Vui lòng kiểm tra email.");
            }

            // Lấy danh sách vai trò của người dùng
            var roles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm vai trò vào claims
            //foreach (var role in roles)
            //{
            //    authClaims.Add(new Claim(ClaimTypes.Role, role));
            //}
            if (roles.Any())
            {
                authClaims.Add(new Claim(ClaimTypes.Role, roles.First()));
            }

            // Tạo token
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                //expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireMinutes"])),
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Lưu token vào session
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            //_httpContextAccessor.HttpContext.Session.SetString("AuthToken", tokenString);

            // Trả về token, thời gian hết hạn và danh sách vai trò
            return (tokenString, token.ValidTo, roles.ToList(), null);
        }
        public async Task<(bool issuccess,string mes)> ForgotPassword ([FromForm] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return (false, "Vui lòng nhập Email");
            }

            var (isValid, mes) = IsValidEmail(email);

            if (!isValid)
            {
                return (false, mes );
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "Không tìm thấy user để gửi email!");
                //return (false",Đã gửi email xác nhận quên mật khẩu!"); // Tránh cho người dùng nhập bừa tài khoản rồi gửi liên tục
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);
            var actionContext = _actionContextAccessor.ActionContext;

            var resetLink = _urlService.GenerateUrl(
                action: "ResetPassword",
                controller: "Users",
                values: new { userId = user.Id, token = encodedToken },
                area: "Admin",
                scheme: actionContext.HttpContext.Request.Scheme
            );

            await _emailSender.SendEmailAsync(user.Email,
            "Đặt lại mật khẩu",
            $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>");


            return (true, "Gửi email xác nhận reset password thành công!");
        }
    }

}
