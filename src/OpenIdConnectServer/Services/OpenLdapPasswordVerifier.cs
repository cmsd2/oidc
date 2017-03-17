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
    public class OpenLdapPasswordVerifier<TUser> : IPasswordVerifier<TUser> where TUser : ApplicationUser
    {
        private readonly IDirectory _directory;
        private readonly ILogger _logger;

        public OpenLdapPasswordVerifier(
            ILoggerFactory loggerFactory,
            IDirectory directory)
        {
            _directory = directory;

            _logger = loggerFactory.CreateLogger<OpenLdapPasswordVerifier<TUser>>();
        }

        public async Task<PasswordVerificationResult> VerifyPasswordAsync(
            IUserStore<TUser> userStore, TUser user, string password,
            Func<Task<PasswordVerificationResult>> next)
        {
            _logger.LogInformation("verifying password for user {UserName}", user.UserName);

            var suffix = "@mendeley.com";

            if (user.UserName.EndsWith(suffix))
            {
                var ldapUserName = user.UserName.Substring(0, user.UserName.Length - suffix.Length);

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
            else
            {
                return await next();
            }
        }
    }
}
