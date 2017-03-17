using AspNetCore.Identity.DynamoDB.OpenIddict;
using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using System.Threading;

namespace OpenIdConnectServer.Services
{
    public class ApplicationApplicationManager : OpenIddictApplicationManager<DynamoIdentityApplication>
    {
        private readonly DynamoApplicationStore<DynamoIdentityApplication, DynamoIdentityToken> _store;

        public ApplicationApplicationManager(
            IOpenIddictApplicationStore<DynamoIdentityApplication> store, 
            ILogger<OpenIddictApplicationManager<DynamoIdentityApplication>> logger) 
            : base(store, logger)
        {
            _store = store as DynamoApplicationStore<DynamoIdentityApplication, DynamoIdentityToken>;
        }

        public Task<IEnumerable<DynamoIdentityApplication>> FindAsync(CancellationToken cancellationToken)
        {
            return _store.FindAsync(cancellationToken);
        }
    }
}
