using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenIddict;
using PaulMiami.AspNetCore.Authentication.Authenticator;
using AspNetCore.Identity.DynamoDB;

namespace OpenIdConnectServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : DynamoIdentityUser, IAuthenticatorUser, IUser
    {
        public string AuthenticatorSecretEncrypted { get; set; }

        public byte AuthenticatorNumberOfDigits { get; set; }

        public byte AuthenticatorPeriodInSeconds { get; set; }

        public HashAlgorithmType AuthenticatorHashAlgorithm { get; set; }
    }
}
