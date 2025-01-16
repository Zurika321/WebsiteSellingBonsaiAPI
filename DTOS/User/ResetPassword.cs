using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public class ResetPassword
    {
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [CustomValidation(typeof(RegisterModel), nameof(ValidatePassword))]
        public string newpassword { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [Compare(nameof(newpassword), ErrorMessage = "Mật khẩu và mật khẩu xác nhận không giống nhau.")]
        public string Comfirmpassword { get; set; }
        public string userid { get; set; }
        public string token { get; set; }
        public static ValidationResult? ValidatePassword(object? value, ValidationContext context)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Mật khẩu không được để trống.");
            }

            if (password.Length < 8)
            {
                return new ValidationResult("Mật khẩu phải có ít nhất 8 ký tự.");
            }

            if (password.Length > 12)
            {
                return new ValidationResult("Mật khẩu không được vượt quá 12 ký tự.");
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
