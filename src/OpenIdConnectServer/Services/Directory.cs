using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenIdConnectServer.Services
{
    public enum DirectorySearchMode
    {
        NetBiosDomain,
        DistinguishedName
    }

    public class DirectoryOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string NetBiosDomain { get; set; }
        public string DistinguishedNameSearchBase { get; set; }
        public DirectorySearchMode DirectorySearchMode { get; set; } = DirectorySearchMode.DistinguishedName;
        public bool SecureSocketLayer { get; set; } = false;
    }

    public class Directory : IDirectory
    {
        private readonly DirectoryOptions _options;
        private readonly ILogger _logger;

        public Directory(IOptions<Settings> options, ILoggerFactory loggerFactory)
        {
            _options = options.Value.Ldap;
            _logger = loggerFactory.CreateLogger<Directory>();
        }

        public bool HandleCertValidation(X509Certificate cert, SslPolicyErrors errors)
        {
            _logger.LogInformation("ldap ssl validation result for cert {CertIssuer} {CertSubject}: {SslErrors}", cert.Issuer, cert.Subject, errors.ToString());
            return true;
        }

        public Task<DirectoryLoginResult> VerifyUserPassword(string username, string password)
        {
            const int ldapVersion = LdapConnection.Ldap_V3;
            var conn = new LdapConnection();
            conn.SecureSocketLayer = _options.SecureSocketLayer;
            conn.Connection.OnCertificateValidation += new CertificateValidationCallback(HandleCertValidation);

            try
            {
                conn.Connect(_options.Host, _options.Port);
                string dn;
                if (_options.DirectorySearchMode == DirectorySearchMode.NetBiosDomain)
                {
                    dn = $"{_options.NetBiosDomain}\\{username}";
                }
                else
                {
                    dn = $"uid={username},{_options.DistinguishedNameSearchBase}";
                }
                conn.Bind(ldapVersion, dn, password);
                conn.Disconnect();
            }
            catch (LdapException e)
            {
                return Task.FromResult(new DirectoryLoginResult {Success = false, Message = e.Message});
            }
            catch (System.IO.IOException e)
            {
                return Task.FromResult(new DirectoryLoginResult {Success = false, Message = e.Message});
            }

            return Task.FromResult(new DirectoryLoginResult {Success = true});
        }
    }
}
