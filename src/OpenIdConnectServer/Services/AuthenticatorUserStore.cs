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

namespace OpenIdConnectServer.Services {
    /// <summary>
    /// Provides methods allowing to manage the users stored in a database.
    /// </summary>
    /// <typeparam name="TUser">The type of the User entity.</typeparam>
    /// <typeparam name="TApplication">The type of the Application entity.</typeparam>
    /// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
    /// <typeparam name="TRole">The type of the Role entity.</typeparam>
    /// <typeparam name="TToken">The type of the Token entity.</typeparam>
    /// <typeparam name="TContext">The type of the Entity Framework database context.</typeparam>
    /// <typeparam name="TKey">The type of the entity primary keys.</typeparam>
    public class AuthenticatorUserStore<TUser, TApplication, TAuthorization, TRole, TToken, TContext, TKey> :
        OpenIddictUserStore<TUser, TApplication, TAuthorization, TRole, TToken, TContext, TKey>,
        IUserAuthenticatorStore<TUser>
        where TUser : OpenIddictUser<TKey, TAuthorization, TToken>, IAuthenticatorUser, new()
        where TApplication : OpenIddictApplication<TKey, TToken>
        where TAuthorization : OpenIddictAuthorization<TKey, TToken>
        where TRole : IdentityRole<TKey>
        where TToken : OpenIddictToken<TKey>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey> 
    {
        public AuthenticatorUserStore(TContext context)
            : base(context) 
        {
        }

        public virtual Task<AuthenticatorParams> GetAuthenticatorParamsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            user.CheckArgumentNull(nameof(user));

            var authenticatorParams = new AuthenticatorParams();
            if (!string.IsNullOrEmpty(user.AuthenticatorSecretEncrypted))
                authenticatorParams.Secret = Convert.FromBase64String(user.AuthenticatorSecretEncrypted);
            else
                authenticatorParams.Secret = null;
            authenticatorParams.HashAlgorithm = user.AuthenticatorHashAlgorithm;
            authenticatorParams.NumberOfDigits = user.AuthenticatorNumberOfDigits;
            authenticatorParams.PeriodInSeconds = user.AuthenticatorPeriodInSeconds;

            return Task.FromResult(authenticatorParams);
        }

        public virtual Task SetAuthenticatorParamsAsync(TUser user, AuthenticatorParams authenticatorParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            user.CheckArgumentNull(nameof(user));
            authenticatorParams.CheckArgumentNull(nameof(authenticatorParams));

            if (authenticatorParams.Secret != null)
                user.AuthenticatorSecretEncrypted = Convert.ToBase64String(authenticatorParams.Secret);
            else
                user.AuthenticatorSecretEncrypted = null;
            user.AuthenticatorHashAlgorithm = authenticatorParams.HashAlgorithm;
            user.AuthenticatorNumberOfDigits = authenticatorParams.NumberOfDigits;
            user.AuthenticatorPeriodInSeconds = authenticatorParams.PeriodInSeconds;

            return Task.FromResult(0);
        }
    }
}
