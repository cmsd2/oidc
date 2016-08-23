using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public class DirectoryLoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public interface IDirectory
    {
        Task<DirectoryLoginResult> VerifyUserPassword(string username, string password);
    }
}
