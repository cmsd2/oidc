using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenIdConnectServer.Services;

namespace OpenIdConnectServer
{
    public enum TransportSecurity {
        None,
        Ssl,
        Tls
    }

    public class FromAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public TransportSecurity Security { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public FromAddress From { get; set; }
    }

    public class AuthenticatorSettings
    {
        public string Issuer { get; set; }
    }

    public class Settings
    {
        public SmtpSettings Smtp { get; set; }
        public DirectorySettings Ldap { get; set; }
        public DynamoDbSettings DynamoDB { get; set; }
        public AuthenticatorSettings Authenticator { get; set; }
    }
}
