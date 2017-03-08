using AspNet.Security.OpenIdConnect.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Models.AuthorizationViewModels
{
    public class AuthorizeDeviceCodeViewModel : OpenIdConnectRequest
    {
        public string UserCode { get; set; }
        public string ApplicationName { get; set; }
    }
}
