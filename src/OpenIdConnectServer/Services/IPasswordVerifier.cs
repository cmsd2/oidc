using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenIdConnectServer.Models;

namespace OpenIdConnectServer.Services
{
    public interface IPasswordVerifier
    {
        Task<PasswordVerificationResult> VerifyPasswordAsync<TUser>(
            IUserPasswordStore<TUser> store, 
            TUser user, 
            string password,
            Func<Task<PasswordVerificationResult>> next) 
            where TUser : class, IUser;
    }

    public class DefaultPasswordVerifier : IPasswordVerifier
    {
        Task<PasswordVerificationResult> IPasswordVerifier.VerifyPasswordAsync<TUser>(IUserPasswordStore<TUser> store, TUser user, string password, Func<Task<PasswordVerificationResult>> next)
        {
            return next();
        }
    }
}
