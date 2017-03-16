using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OpenIdConnectServer.ViewModels.Shared
{
    public class ErrorViewModel
    {
        [Display(Name = "Error")]
        [JsonProperty("error")]
        public string Error { get; set; }

        [Display(Name = "Description")]
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
