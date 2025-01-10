using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.DTOS.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebsiteSellingBonsaiAPI.Utils;
using Azure.Core;
using Microsoft.AspNetCore.Identity.UI.Services;
using NuGet.Common;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        private readonly EmailSender _emailSender;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    EmailSender emailSender,
    IUrlHelperFactory urlHelperFactory,
    IActionContextAccessor actionContextAccessor)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<(bool IsSuccess, string Message)> RegisterUser([FromForm] RegisterModel model)
        {
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return (false, "User already exists!");
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
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = urlHelper.Action(
                action: "ConfirmEmail",
                controller: "Users",
                values: new { userId = user.Id, token, area = "Admin" },
                protocol: actionContext.HttpContext.Request.Scheme);

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
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
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
    }

}
