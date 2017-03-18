using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public enum DirectorySearchMode
    {
        NetBiosDomain,
        DistinguishedName
    }

    public class DirectorySettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string NetBiosDomain { get; set; }
        public string DistinguishedNameSearchBase { get; set; }
        public DirectorySearchMode DirectorySearchMode { get; set; } = DirectorySearchMode.DistinguishedName;
        public bool SecureSocketLayer { get; set; } = false;
        public string LoginEmailDomain { get; set; }
    }
}
