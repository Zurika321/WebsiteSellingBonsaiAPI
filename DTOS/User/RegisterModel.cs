using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;

    public class RegisterModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập quá ngắn.")]
        [MaxLength(15, ErrorMessage = "Tên đăng nhập quá dài.")]
        [CustomValidation(typeof(RegisterModel), nameof(ValidateUserName))]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [CustomValidation(typeof(RegisterModel), nameof(ValidatePassword))]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu và mật khẩu xác nhận không giống nhau.")]
        public string ComfrimPassword { get; set; }

        public static ValidationResult? ValidateUserName(object? value, ValidationContext context)
        {
            if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult("Tên đăng nhập không được toàn khoảng trắng.");
            }

            return ValidationResult.Success;
        }
        public static ValidationResult? ValidatePassword(object? value, ValidationContext context)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Mật khẩu không được để trống.");
            }

            if (password.Length < 7)
            {
                return new ValidationResult("Mật khẩu phải có ít nhất 7 ký tự.");
            }

            if (password.Length > 12)
            {
                return new ValidationResult("Mật khẩu không được vượt quá 15 ký tự.");
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
            {
                return new ValidationResult("Mật khẩu phải chứa ít nhất một ký tự đặc biệt.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ cái viết hoa.");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ cái viết thường.");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ số.");
            }

            return ValidationResult.Success;
        }
    }
}
