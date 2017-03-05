using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Models.AuthorizationViewModels
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
        public string State { get; internal set; }
        public string IdTokenHint { get; internal set; }
        public string PostLogoutRedirectUri { get; internal set; }
    }
}
