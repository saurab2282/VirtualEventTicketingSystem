using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace VirtualEventTicketingSystem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                EnableSsl = true,                   
                UseDefaultCredentials = false,      
                Credentials = new NetworkCredential(
                    _smtpSettings.Username,
                    _smtpSettings.Password
                ),
                DeliveryMethod = SmtpDeliveryMethod.Network  // <-- REQUIRED
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }

    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}