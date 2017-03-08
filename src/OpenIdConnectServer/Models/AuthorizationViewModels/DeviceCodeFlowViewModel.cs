using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Models.AuthorizationViewModels
{
    public class DeviceCodeFlowViewModel
    {
        public string VerificationUri { get; set; }
        public string UserCode { get; set; }
        public string DeviceCode { get; set; }
        public int Interval { get; set; }
    }
}
