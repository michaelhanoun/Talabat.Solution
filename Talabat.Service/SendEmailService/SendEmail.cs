using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Talabat.Core.Services.Contract;

namespace Talabat.Application.SendEmailService
{
    public class SendEmail : ISendEmail
    {
        private readonly IConfiguration _configuration;

        public SendEmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendMail(string recipients, string subject, string body)
        {
            var email = new MailMessage();
            email.Subject = subject;
            email.Body = $"<html><html>{body}</html></html>";
            email.IsBodyHtml = true;
            email.From = new MailAddress(_configuration["EmailSettings:SenderEmail"]);
            email.To.Add(recipients);
            var smtp = new SmtpClient(_configuration["EmailSettings:SmtpClientServer"], int.Parse(_configuration["EmailSettings:SmtpClientPort"]??"0")) {
             Credentials = new NetworkCredential(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderPassword"]),EnableSsl = true            };
            await smtp.SendMailAsync(email);
        }
    }
}
