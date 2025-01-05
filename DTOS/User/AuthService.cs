using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.DTOS.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, string Message)> RegisterUser(RegisterModel model)
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
            };

            // Tạo người dùng với mật khẩu
            var result = await _userManager.CreateAsync(user, model.Password);

            // Kiểm tra xem việc tạo người dùng có thành công không
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"User creation failed: {errors}");
            }

            // Gán vai trò cho người dùng
            await _userManager.AddToRoleAsync(user, "User");

            return (true, "User created successfully!");
        }

        public async Task<(string? Token, DateTime? Expiration, List<string>? Roles, string? Error)> LoginUser(LoginModel model)
        {
            // Kiểm tra xem người dùng có tồn tại không
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return (null, null, null, "Invalid credentials!");

            // Lấy danh sách vai trò của người dùng
            var roles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
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
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Lưu token vào session
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _httpContextAccessor.HttpContext.Session.Set("AuthToken", tokenString);

            // Trả về token, thời gian hết hạn và danh sách vai trò
            return (tokenString, token.ValidTo, roles.ToList(), null);
        }
    }

}
