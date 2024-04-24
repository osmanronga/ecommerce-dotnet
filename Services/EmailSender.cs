using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using BestStoreApi.Interfaces;
using System.Net;

namespace BestStoreApi.Services
{
    public class EmailSender
    {
        private readonly string apiKey;
        private readonly string fromEmail;
        private readonly string senderName;

        public EmailSender(IConfiguration configuration)
        {
            apiKey = configuration["EmailSender:ApiKey"]!;
            fromEmail = configuration["EmailSender:FromEmail"]!;
            senderName = configuration["EmailSender:SenderName"]!;
        }

        public async Task SendEmail(string subject, string toEmail, string username, string message)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, senderName);
            var to = new EmailAddress(toEmail, username);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 25)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("rongawork@gmail.com", "hcfl ygsf xrwc itpl")
            };

            return client.SendMailAsync(
                new MailMessage(from: "rongawork@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}