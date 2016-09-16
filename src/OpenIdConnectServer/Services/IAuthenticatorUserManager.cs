using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIdConnectServer.Models;
using PaulMiami.AspNetCore.Identity.Authenticator;

namespace OpenIdConnectServer.Services
{
    public interface IAuthenticatorUserManager<TUser> where TUser : class
    {
        Task<bool> GetAuthenticatorEnabledAsync(TUser user);
        Task<AuthenticatorParams> GetAuthenticatorParamsAsync(TUser user);

        Task<bool> EnableAuthenticatorAsync(TUser user, Authenticator authenticator, string code, CancellationToken cancellationToken);

        Task<bool> DisableAuthenticatorAsync(TUser user, string code, CancellationToken cancellationToken);

        Task<Authenticator> CreateAuthenticatorAsync(TUser user, CancellationToken cancellationToken);
    }
}