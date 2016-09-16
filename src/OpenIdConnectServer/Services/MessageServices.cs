using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;

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
                    _logger.LogInformation("connecting to smtp gateway at {SmtpHost}", _settings.Smtp.Host);
                    await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, SecureSocketOptionsFromTransportSecurity(_settings.Smtp.Security));
                    _logger.LogInformation("authenticating with smtp gateway");
                    await client.AuthenticateAsync(_settings.Smtp.Username, _settings.Smtp.Password);
                    _logger.LogInformation("sending message to smtp gateway");
                    await client.SendAsync(message);
                    _logger.LogInformation("disconnecting from smtp gateway");
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("message sent to smtp gateway");
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
    


        public static SecureSocketOptions SecureSocketOptionsFromTransportSecurity(TransportSecurity security) {
            switch(security) {
                case TransportSecurity.Tls:
                    return SecureSocketOptions.StartTls;
                case TransportSecurity.Ssl:
                    return SecureSocketOptions.SslOnConnect;
                case TransportSecurity.None:
                    return SecureSocketOptions.None;
                default:
                    throw new Exception($"invalid smtp transport security parameter {security}");
            }
        }
    }
}
