using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public class DynamoScopeStore<TScope> : IOpenIddictScopeStore<TScope>
        where TScope : class
    {
        public Task EnsureInitializedAsync(AmazonDynamoDBClient client, DynamoDBContext context, string scopesTableName)
        {
            return Task.FromResult(0);
        }
    }
}
