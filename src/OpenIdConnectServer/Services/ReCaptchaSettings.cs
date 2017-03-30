using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public class ReCaptchaSettings
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Uri { get; set; }
    }
}
