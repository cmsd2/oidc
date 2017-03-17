using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.ViewModels.Applications
{
    public class ApplicationWithSecretViewModel : ApplicationViewModel
    {
        public ApplicationWithSecretViewModel()
        {
        }

        public ApplicationWithSecretViewModel(ApplicationViewModel model, string clientSecret) : base(model)
        {
            ClientSecret = clientSecret;
        }

        public string ClientSecret { get; set; }
    }
}
