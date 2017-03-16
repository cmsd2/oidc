using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.ViewModels.AuthorizationViewModels
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [BindNever]
        public string RequestId { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }

        public string ClientId { get; set; }

        public string ResponseType { get; set; }

        public string ResponseMode { get; set; }

        public string RedirectUri { get; set; }

        public string State { get; set; }

        public string Nonce { get; set; }
    }
}
