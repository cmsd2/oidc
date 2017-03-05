using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict;
using PaulMiami.AspNetCore.Identity.Authenticator;
using PaulMiami.AspNetCore.Authentication.Authenticator;
using OpenIdConnectServer.Models;
using AspNetCore.Identity.DynamoDB;

namespace OpenIdConnectServer.Services {
    /// <summary>
    /// Provides methods allowing to manage the users stored in a database.
    /// </summary>
    /// <typeparam name="TUser">The type of the User entity.</typeparam>
    /// <typeparam name="TRole">The type of the Role entity.</typeparam>
    public class ApplicationUserStore<TUser, TRole> :
        DynamoUserStore<TUser, TRole>,
        IUserAuthenticatorStore<TUser>
        where TUser : DynamoIdentityUser, IAuthenticatorUser
        where TRole : DynamoIdentityRole
    {
        public ApplicationUserStore(DynamoRoleUsersStore<TRole, TUser> roleUsersStore): base(roleUsersStore)
        {
        }

        public virtual Task<AuthenticatorParams> GetAuthenticatorParamsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var authenticatorParams = new AuthenticatorParams();

            if (!string.IsNullOrEmpty(user.AuthenticatorSecretEncrypted))
            {
                authenticatorParams.Secret = Convert.FromBase64String(user.AuthenticatorSecretEncrypted);
            }
            else
            {
                authenticatorParams.Secret = null;
            }

            authenticatorParams.HashAlgorithm = user.AuthenticatorHashAlgorithm;
            authenticatorParams.NumberOfDigits = user.AuthenticatorNumberOfDigits;
            authenticatorParams.PeriodInSeconds = user.AuthenticatorPeriodInSeconds;

            return Task.FromResult(authenticatorParams);
        }

        public virtual Task SetAuthenticatorParamsAsync(TUser user, AuthenticatorParams authenticatorParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (authenticatorParams == null)
            {
                throw new ArgumentNullException(nameof(authenticatorParams));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (authenticatorParams.Secret != null)
            {
                user.AuthenticatorSecretEncrypted = Convert.ToBase64String(authenticatorParams.Secret);
            }
            else
            {
                user.AuthenticatorSecretEncrypted = null;
            }

            user.AuthenticatorHashAlgorithm = authenticatorParams.HashAlgorithm;
            user.AuthenticatorNumberOfDigits = authenticatorParams.NumberOfDigits;
            user.AuthenticatorPeriodInSeconds = authenticatorParams.PeriodInSeconds;

            return Task.FromResult(0);
        }
    }
}
