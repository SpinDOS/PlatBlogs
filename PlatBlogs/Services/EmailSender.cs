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
        public EmailSender(IOptions<EmailInfo> optionsAccessor)
        {
            var emailCredentials = optionsAccessor.Value;
            SmtpClient = new SmtpClient(emailCredentials.Server, emailCredentials.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(emailCredentials.Username, emailCredentials.Password),
            };
            From = emailCredentials.From;
        }

        private string From { get; }

        private SmtpClient SmtpClient { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage(From, email, subject, message);
            return SmtpClient.SendMailAsync(mailMessage);
        }
    }
}

