using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspNetCore.Identity.DynamoDB.OpenIddict;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using System.Threading;

namespace OpenIdConnectServer.Services
{
    public class ApplicationAuthorizationManager<TAuthorization> : OpenIddictAuthorizationManager<TAuthorization>
        where TAuthorization : DynamoIdentityAuthorization
    {
        private readonly DynamoAuthorizationStore<TAuthorization> _store;

        public ApplicationAuthorizationManager(IOpenIddictAuthorizationStore<TAuthorization> store, ILogger<OpenIddictAuthorizationManager<TAuthorization>> logger) : base(store, logger)
        {
            _store = store as DynamoAuthorizationStore<TAuthorization>;
        }

        public async Task UpdateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            await _store.UpdateAsync(authorization, cancellationToken);
        }

        public async Task RevokeAuthorizationAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            await _store.RevokeAsync(authorization, cancellationToken);
        }
    }
}
