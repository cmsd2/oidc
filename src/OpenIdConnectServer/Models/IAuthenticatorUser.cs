using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenIddict;
using PaulMiami.AspNetCore.Authentication.Authenticator;

namespace OpenIdConnectServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public interface IAuthenticatorUser
    {
        string AuthenticatorSecretEncrypted { get; set; }

        byte AuthenticatorNumberOfDigits { get; set; }

        byte AuthenticatorPeriodInSeconds { get; set; }

        HashAlgorithmType AuthenticatorHashAlgorithm { get; set; }
    }
}
