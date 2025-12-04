using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace VirtualEventTicketingSystem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");

            var client = new SmtpClient(smtp["Host"])
            {
                Port = int.Parse(smtp["Port"]!),
                Credentials = new NetworkCredential(smtp["saurab.sima@gmail.com"], smtp["jlzi lubt fdtl xeom"]),
                EnableSsl = bool.Parse(smtp["EnableSsl"]!)
            };

            var mail = new MailMessage(from: smtp["User"], to: email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mail);
        }
    }
}