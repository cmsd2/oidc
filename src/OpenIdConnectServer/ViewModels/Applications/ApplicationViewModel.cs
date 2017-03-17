using AspNetCore.Identity.DynamoDB.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.ViewModels.Applications
{
    public class ApplicationViewModel
    {
        public ApplicationViewModel()
        {
        }

        public ApplicationViewModel(ApplicationViewModel application)
        {
            Id = application.Id;
            CreatedOn = application.CreatedOn;
            ClientId = application.ClientId;
            DisplayName = application.DisplayName;
            RedirectUri = application.RedirectUri;
            LogoutRedirectUri = application.LogoutRedirectUri;
            Type = application.Type;
        }

        public ApplicationViewModel(DynamoIdentityApplication application)
        {
            Id = application.Id;
            CreatedOn = application.CreatedOn;
            ClientId = application.ClientId;
            DisplayName = application.DisplayName;
            RedirectUri = application.RedirectUri;
            LogoutRedirectUri = application.LogoutRedirectUri;
            Type = application.Type;
        }

        public string Id { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ClientId { get; set; }
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        public string LogoutRedirectUri { get; set; }
        public string Type { get; set; }
    }
}
