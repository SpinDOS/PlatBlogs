using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PlatBlogs.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    //public class EmailSender : IEmailSender
    //{
    //    public Task SendEmailAsync(string email, string subject, string message)
    //    {
    //        return Task.CompletedTask;
    //    }
    //}
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<EmailCredentials> optionsAccessor)
        {
            var emailCredentials = optionsAccessor.Value;
            SmtpClient = new SmtpClient(ServerAddress, ServerPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(emailCredentials.Username, emailCredentials.Password),
            };
            From = emailCredentials.Username + HostFrom;
        }

        private const string ServerAddress = "smtp.mail.ru";
        private const int ServerPort = 25;
        private const string HostFrom = "@mail.ru";

        private string From { get; }

        private SmtpClient SmtpClient { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(email, subject, message);
        }

        public Task Execute(string email, string subject, string message)
        {
            var mailMessage = new MailMessage(From, email, subject, message);
            return SmtpClient.SendMailAsync(mailMessage);
        }
    }
}

