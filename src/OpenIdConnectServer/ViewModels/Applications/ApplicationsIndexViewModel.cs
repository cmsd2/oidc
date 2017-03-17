using AspNetCore.Identity.DynamoDB.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.ViewModels.Applications
{
    public class ApplicationsIndexViewModel
    {
        public IEnumerable<DynamoIdentityApplication> Applications { get; set; }
    }
}
