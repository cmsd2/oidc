using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenIdConnectServer.Models;

namespace OpenIdConnectServer.Services
{
    public interface IPasswordVerifier<TUser> where TUser : class, IUser
    {
        Task<PasswordVerificationResult> VerifyPasswordAsync(
            UserManager<TUser> store,
            TUser user,
            string password,
            Func<Task<PasswordVerificationResult>> next);
    }

    public class DefaultPasswordVerifier<TUser> : IPasswordVerifier<TUser> where TUser : class, IUser
    {
        public Task<PasswordVerificationResult> VerifyPasswordAsync(UserManager<TUser> store, TUser user, string password, Func<Task<PasswordVerificationResult>> next)
        {
            return next();
        }
    }
}
