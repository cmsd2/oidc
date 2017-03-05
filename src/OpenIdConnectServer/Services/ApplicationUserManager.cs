using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIdConnectServer.Models;
using PaulMiami.AspNetCore.Authentication.Authenticator;
using PaulMiami.AspNetCore.Identity.Authenticator;

namespace OpenIdConnectServer.Services
{
    public class ApplicationUserManager<TUser> : UserManager<TUser>, IAuthenticatorUserManager<TUser> 
        where TUser : class, IUser
    {
        private readonly IPasswordVerifier _passwordVerifier;
        private readonly AuthenticatorUserManager<TUser> _authenticatorUserManager;

        public ApplicationUserManager(
            IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<TUser>> logger,
            IDataProtectionProvider dataProtectionProvider,
            IAuthenticatorService authenticatorService,
            IPasswordVerifier passwordVerifier)

            : base(store, optionsAccessor, passwordHasher, userValidators,
                  passwordValidators, keyNormalizer, errors, services, logger)
        {
            _authenticatorUserManager = new AuthenticatorUserManager<TUser>(
                store, optionsAccessor, passwordHasher, userValidators,
                passwordValidators, keyNormalizer, errors, services,
                logger, dataProtectionProvider, authenticatorService
            );

            _passwordVerifier = passwordVerifier;
        }

        protected override Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TUser> store, TUser user, string password)
        {
            Logger.LogInformation("verifying password for user {UserName}", user.UserName);
            return _passwordVerifier.VerifyPasswordAsync(store, user, password, 
                () => base.VerifyPasswordAsync(store, user, password));
        }

        public override Task<TUser> FindByEmailAsync(string email)
        {
            Logger.LogInformation("find user by email {Email}", email);
            return base.FindByEmailAsync(email);
        }

        public override Task<TUser> FindByNameAsync(string userName)
        {
            Logger.LogInformation("find user by name {Name}", userName);
            return base.FindByNameAsync(userName);
        }

        public Task<bool> GetAuthenticatorEnabledAsync(TUser user)
        {
            return _authenticatorUserManager.GetAuthenticatorEnabledAsync(user);
        }

        public Task<AuthenticatorParams> GetAuthenticatorParamsAsync(TUser user)
        {
            return _authenticatorUserManager.GetAuthenticatorParamsAsync(user);
        }

        public Task<bool> EnableAuthenticatorAsync(TUser user, Authenticator authenticator, string code, CancellationToken cancellationToken)
        {
            return _authenticatorUserManager.EnableAuthenticatorAsync(user, authenticator, code, cancellationToken);
        }

        public Task<bool> DisableAuthenticatorAsync(TUser user, string code, CancellationToken cancellationToken)
        {
            return _authenticatorUserManager.DisableAuthenticatorAsync(user, code, cancellationToken);
        }

        public Task<Authenticator> CreateAuthenticatorAsync(TUser user, CancellationToken cancellationToken)
        {
            return _authenticatorUserManager.CreateAuthenticatorAsync(user, cancellationToken);
        }
    }
}
