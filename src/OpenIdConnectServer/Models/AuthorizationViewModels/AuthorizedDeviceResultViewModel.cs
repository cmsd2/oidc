using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Models.AuthorizationViewModels
{
    public class AuthorizedDeviceResultViewModel
    {
        public string ApplicationName { get; set; }
        public string Scope { get; set; }
        public bool Authorized { get; set; }
    }
}
