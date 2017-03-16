using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;

namespace OpenIdConnectServer.ViewModels.AuthorizationViewModels
{
    public class AuthorizeDeviceCodeRequest : OpenIdConnectRequest
    {
        public string UserCode { get; set; }
    }
}
