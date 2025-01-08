using Microsoft.AspNetCore.Identity;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS.User
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
