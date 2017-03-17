using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIdConnectServer.Models;

namespace OpenIdConnectServer.Services
{
    public class SimpleSignInManager : SignInManager<ApplicationUser>
    {

        public SimpleSignInManager(
            UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<ApplicationUser>> logger) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger)
        {
        }

        public override Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            Logger.LogInformation("attempting password login for user {UserName}", user.UserName);
            return base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            Logger.LogInformation("attempting password login for username {UserName}", userName);

            var applicationUser = new ApplicationUser
            {
                UserName = userName
            };

            var result = await this.UserManager.CheckPasswordAsync(applicationUser, password);

            if (result)
            {
                await this.FindOrCreateUser(applicationUser, password);
            }

            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }

        public async Task<IdentityResult> FindOrCreateUser(ApplicationUser user, string password)
        {
            ApplicationUser foundUser = await UserManager.FindByNameAsync(user.UserName);

            if (foundUser != null)
            {
                return IdentityResult.Success;
            }

            return await UserManager.CreateAsync(user, password);
        }
    }
}
