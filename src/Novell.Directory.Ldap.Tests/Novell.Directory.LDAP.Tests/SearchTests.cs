using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.LDAP.Tests
{
    public class SearchTests
    {
        [Fact]
        public void Ldap_Search_Should_Return_Correct_Results_Count()
        {
            var loginDN = "dc=example,dc=com";
            var password = "";

            LdapConnection conn = new LdapConnection();
            conn.Connect(Globals.Host, Globals.DefaultPort);
            conn.Bind(loginDN, password);
            LdapSearchResults lsc = conn.Search(
                    "dc=example,dc=com",
                    LdapConnection.SCOPE_SUB,
                    "objectclass=*",
                    null,
                    false);

            int resultsCount = lsc.Count;
            int counter = 0;
            while (lsc.hasMore())
            {
                LdapEntry nextEntry = lsc.next();
                ++counter;
            }

            Assert.Equal(resultsCount, counter);

            conn.Disconnect();
        }

        [Fact]
        public void Ldap_Search_Should_Return_Not_Null_Entries()
        {
            var loginDN = "dc=example,dc=com";
            var password = "";

            LdapConnection conn = new LdapConnection();
            conn.Connect(Globals.Host, Globals.DefaultPort);
            conn.Bind(loginDN, password);
            LdapSearchResults lsc = conn.Search(
                    "dc=example,dc=com",
                    LdapConnection.SCOPE_SUB,
                    "objectclass=*",
                    null,
                    false);

            while (lsc.hasMore())
            {
                LdapEntry nextEntry = lsc.next();
                Assert.NotEqual(nextEntry, (LdapEntry)null);
            }

            conn.Disconnect();
        }

        [Fact]
        public void Ldap_Entry_Should_Return_Dn_Property()
        {
            var loginDN = "dc=example,dc=com";
            var password = "";

            LdapConnection conn = new LdapConnection();
            conn.Connect(Globals.Host, Globals.DefaultPort);
            conn.Bind(loginDN, password);
            LdapSearchResults lsc = conn.Search(
                    "dc=example,dc=com",
                    LdapConnection.SCOPE_SUB,
                    "objectclass=*",
                    null,
                    false);

            while (lsc.hasMore())
            {
                LdapEntry nextEntry = lsc.next();
                Assert.False(string.IsNullOrEmpty(nextEntry.DN));
            }

            conn.Disconnect();
        }

        [Fact]
        public void Ldap_Search_Should_Return_Not_More_Results_Than_Defined_In_Ldap_Search_Constraints()
        {
            var loginDN = "dc=example,dc=com";
            var password = "";
            int maxResults = 1;

            LdapConnection conn = new LdapConnection();
            conn.Connect(Globals.Host, Globals.DefaultPort);
            conn.Bind(loginDN, password);
            LdapSearchResults lsc = conn.Search(
                    "dc=example,dc=com",
                    LdapConnection.SCOPE_SUB,
                    "objectclass=*",
                    null,
                    false,
                    new LdapSearchConstraints { MaxResults = maxResults});

            int counter = 0;
            var exception = Record.Exception(() =>
            {
                while (lsc.hasMore())
                {
                    LdapEntry nextEntry = lsc.next();
                    ++counter;
                }
            });

            Assert.IsType<LdapException>(exception);
            Assert.Equal(exception.Message, "Sizelimit Exceeded");
            Assert.InRange(counter, 0, maxResults);

            conn.Disconnect();
        }
    }
}
