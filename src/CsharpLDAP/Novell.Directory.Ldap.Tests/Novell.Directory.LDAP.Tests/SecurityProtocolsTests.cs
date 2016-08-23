using Novell.Directory.Ldap;
using System;
using Xunit;

namespace Novell.Directory.LDAP.Tests
{
    public class SecurityProtocolsTests
    {
        [Fact]
        public void Ldap_Connection_Should_Bind_Login_And_Password()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);
            ldap.Bind(Globals.LoginDN, Globals.Password);

            Assert.True(ldap.Connected);
        }

        [Fact]
        public void Ldap_Connection_Should_Start_TLS()
        {
            var ldap = new LdapConnection();
            ldap.UserDefinedServerCertValidationDelegate += (certificate, certificateErrors) => true;
            ldap.Connect(Globals.Host, Globals.DefaultPort);
            ldap.startTLS();

            Assert.True(ldap.TLS);
        }

        [Fact]
        public void Ldap_Connection_Should_Start_and_Stop_TLS()
        {
            var ldap = new LdapConnection();
            ldap.UserDefinedServerCertValidationDelegate += (certificate, certificateErrors) => true;
            ldap.Connect(Globals.Host, Globals.DefaultPort);
            ldap.startTLS();
            ldap.stopTLS();

            Assert.False(ldap.TLS);
        }

        [Fact]
        public void Ldap_Connection_Should_Not_Start_TLS_With_Invalid_Certificate_That_Is_Processed_By_Default_Certificate_Validation_Handler()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);

            Assert.Throws(typeof(LdapException), () => { ldap.startTLS(); });
        }

        [Fact]
        public void Ldap_Connection_Should_Connect_SSL()
        {
            var ldap = new LdapConnection();
            ldap.SecureSocketLayer = true;
            ldap.UserDefinedServerCertValidationDelegate += (certificate, certificateErrors) => true;
            ldap.Connect(Globals.Host, Globals.SslPort);
            ldap.Bind(Globals.LoginDN, Globals.Password);
            
            Assert.True(ldap.Connected);
        }
    }
}
