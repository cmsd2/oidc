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
    public class OpenLdapUserManager<TUser> : UserManager<TUser>, IAuthenticatorUserManager<TUser> where TUser : class, IUser
    {
        private readonly IDirectory _directory;
        private readonly AuthenticatorUserManager<TUser> _authenticatorUserManager;

        public OpenLdapUserManager(
            IUserStore<TUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<TUser> passwordHasher, 
            IEnumerable<IUserValidator<TUser>>  userValidators, 
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<TUser>> logger,
            IDirectory directory,
            IDataProtectionProvider dataProtectionProvider,
            IAuthenticatorService authenticatorService) 

            : base(store, optionsAccessor, passwordHasher, userValidators, 
                  passwordValidators, keyNormalizer, errors, services, logger)
        {
            _directory = directory;

            _authenticatorUserManager = new AuthenticatorUserManager<TUser>(
                store, optionsAccessor, passwordHasher, userValidators,
                passwordValidators, keyNormalizer, errors, services,
                logger, dataProtectionProvider, authenticatorService
            );
        }

        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TUser> store, TUser user, string password)
        {
            Logger.LogInformation("verifying password for user {UserName}", user.UserName);

            var suffix = "@mendeley.com";

            var ldapUserName = user.UserName.EndsWith(suffix)
                ? user.UserName.Substring(0, user.UserName.Length - suffix.Length)
                : user.UserName;
        
            var verificationResult = await _directory.VerifyUserPassword(ldapUserName, password);

            if (verificationResult.Success)
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
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
            return GetAuthenticatorParamsAsync(user);
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
