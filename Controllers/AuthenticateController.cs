using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;

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
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(500, new ResponseModel { Status = "Error", Message = "User already exists!" });

            // Tạo đối tượng người dùng
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                // Không cần FullName nữa
            };

            // Tạo người dùng với mật khẩu
            var result = await _userManager.CreateAsync(user, model.Password);

            // Kiểm tra xem việc tạo người dùng có thành công không
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(500, new ResponseModel { Status = "Error", Message = $"User creation failed: {errors}" });
            }

            // Gán vai trò cho người dùng, ví dụ như vai trò "User"
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(500, new ResponseModel { Status = "Error", Message = "User already exists!" });

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
                return StatusCode(500, new ResponseModel { Status = "Error", Message = $"User creation failed: {errors}" });
            }

            // Gán vai trò "Admin" cho người dùng
            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new ResponseModel { Status = "Success", Message = "Admin account created successfully!" });
        }

        //[HttpPost]
        //[Route("login")]
        //public async Task<IActionResult> Login([FromBody] LoginModel model)
        //{
        //    var user = await _userManager.FindByNameAsync(model.Username);
        //    if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
        //        return Unauthorized(new ResponseModel { Status = "Error", Message = "Invalid credentials!" });

        //    var roles = await _userManager.GetRolesAsync(user);
        //    var authClaims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, user.UserName),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    foreach (var role in roles)
        //    {
        //        authClaims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var token = GenerateJwtToken(authClaims);
        //    return Ok(new
        //    {
        //        token = new JwtSecurityTokenHandler().WriteToken(token),
        //        expiration = token.ValidTo
        //    });
        //}

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
                return Unauthorized(new ResponseModel { Status = "Error", Message = "Invalid credentials!" });

            var roles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateJwtToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                roles = roles // Thêm danh sách vai trò của người dùng
            });
        }


        private JwtSecurityToken GenerateJwtToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:ExpireMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}
