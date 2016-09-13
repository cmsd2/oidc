using System;
using Xunit;
using Novell.Directory.Ldap;

namespace Novell.Directory.LDAP.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void Ldap_Connection_Should_Connect()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);

            Assert.True(ldap.Connected);
        }

        [Fact]
        public void Ldap_Connection_Should_Not_Connect()
        {
            var fakeHost = "0.0.0.0";

            var ldap = new LdapConnection();
            try
            {
                ldap.Connect(fakeHost, Globals.DefaultPort);
            }
            catch (Exception){}
           
            Assert.False(ldap.Connected);
        }

        [Fact]
        public void Ldap_Connection_Should_Connect_And_Disconnect()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);

            Assert.True(ldap.Connected);

            ldap.Disconnect();

            Assert.False(ldap.Connected);
        }

        [Fact]
        public void Ldap_Connection_Clone_Method_Should_Return_Another_Instance_Of_Object()
        {
            var ldapConnection = new LdapConnection();
            var ldapConnectionClone = ldapConnection.Clone();

            Assert.NotEqual(ldapConnection, ldapConnectionClone);
        }

        [Fact]
        public void Ldap_Connection_Should_Return_Simple_Authentication_Method()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);
            ldap.Bind(Globals.LoginDN, Globals.Password);

            Assert.Equal("simple", ldap.AuthenticationMethod);
        }

        [Fact]
        public void Ldap_Connection_Should_Return_Right_Host()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);

            Assert.Equal(Globals.Host, ldap.Host);
        }

        [Fact]
        public void Ldap_Connection_Should_Return_Right_Port()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);

            Assert.Equal(Globals.DefaultPort, ldap.Port);
        }

        [Fact]
        public void Ldap_Connection_Should_Be_Authenticated()
        {
            var ldap = new LdapConnection();
            ldap.Connect(Globals.Host, Globals.DefaultPort);
            ldap.Bind(Globals.LoginDN, Globals.Password);

            Assert.True(ldap.Bound);
        }
    }
}
