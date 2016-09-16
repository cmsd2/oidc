using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenIdConnectServer.Models;
using PaulMiami.AspNetCore.Authentication.Authenticator;
using PaulMiami.AspNetCore.Identity.Authenticator;

namespace OpenIdConnectServer.Services {
    public class AuthenticatorTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : class, IUser
    {
        private IAuthenticatorService _authenticationService;

        public AuthenticatorTokenProvider(IAuthenticatorService authenticationService)
        {
            authenticationService.CheckArgumentNull(nameof(authenticationService));

            _authenticationService = authenticationService;
        }

        public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return await GetAuthenticatorUserManager(manager).GetAuthenticatorEnabledAsync(user);
        }

        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            throw new InvalidOperationException();
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            int code;
            if (!token.TryParseAndRemoveWhiteSpace(out code))
            {
                return false;
            }

            var authenticatorParams = await GetAuthenticatorUserManager(manager).GetAuthenticatorParamsAsync(user);
            var execpectedCode = _authenticationService.GetCode(
                authenticatorParams.HashAlgorithm, 
                authenticatorParams.Secret, 
                authenticatorParams.NumberOfDigits, 
                authenticatorParams.PeriodInSeconds);

            return code == execpectedCode;
        }

        private IAuthenticatorUserManager<TUser> GetAuthenticatorUserManager(UserManager<TUser> manager)
        {
            var cast = manager as IAuthenticatorUserManager<TUser>;
            if (cast == null)
            {
                throw new NotSupportedException(PaulMiami.AspNetCore.Identity.Authenticator.Resources.UserManagerBadCast);
            }
            return cast;
        }
    }
}