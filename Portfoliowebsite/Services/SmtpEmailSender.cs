using System.Net;
using System.Net.Mail;

namespace Portfoliowebsite.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string Name, string Email, string Subject, string Message)
        {
            var host = _configuration["Email:Smtp:Host"];
            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            var toAddress = _configuration["Email:ToAddress"];
            var fromAddress = _configuration["Email:FromAddress"];
            var fromName = _configuration["Email:FromName"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(toAddress) ||
                string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new InvalidOperationException("Emailinstellingen ontbreken. Controleer appsettings.json (Email-sectie).");
            }

            var port = int.TryParse(_configuration["Email:Smtp:Port"], out var parsedPort)
                ? parsedPort
                : 25;

            var enableSsl = bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var parsedSsl) && parsedSsl;

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName ?? "Portfoliowebsite"),
                Subject = $"Contactformulier: {Subject}",
                Body = $"Naam: {Name}\nE-mail: {Email}\nOnderwerp: {Subject}\n\nBericht:\n{Message}",
                IsBodyHtml = false
            };

            mail.To.Add(toAddress);
            mail.ReplyToList.Add(new MailAddress(Email, Name));

            await smtp.SendMailAsync(mail);
        }
    }
}
