using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace OpenIdConnectServer.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly Settings _settings;
        private readonly ILogger _logger;

        public AuthMessageSender(IOptions<Settings> settings, ILoggerFactory loggerFactory)
        {
            _settings = settings.Value;
            _logger = loggerFactory.CreateLogger<AuthMessageSender>();
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                _logger.LogInformation("sending email to {Email} with subject {Subject}", email, subject);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.Smtp.From.Name, _settings.Smtp.From.Address));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = subject;
                message.Body = new TextPart
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, _settings.Smtp.Ssl);
                    await client.AuthenticateAsync(_settings.Smtp.Username, _settings.Smtp.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("error sending email to {Email} with subject {Subject}: {Exception}", email, subject, e);
                throw e;
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            throw new NotSupportedException("sms messaging not supported");
        }
    }
}
