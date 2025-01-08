using System.Net;
using System.Net.Mail;
using WebsiteSellingBonsaiAPI.DTOS.User;

namespace WebsiteSellingBonsaiAPI.Utils
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)//587 cổng
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("conan17032004@gmail.com", "rifgizqezhpbiwgc")
            };

            return client.SendMailAsync(new MailMessage
            {
                From = new MailAddress("conan17032004@gmail.com"),
                To = { email },
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            });

        }
    }
}
