using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Novell.Directory.LDAP.Tests
{
    public class Globals
    {
        public static string Host { get; } = "192.168.1.32";
        public static int DefaultPort { get; } = 389;
        public static int SslPort { get; } = 636;
        public static string LoginDN { get; } = "cn=igor.shmukler,dc=ldap,dc=vqcomms,dc=com";
        public static string Password { get; } = "abc123";
    }
}
