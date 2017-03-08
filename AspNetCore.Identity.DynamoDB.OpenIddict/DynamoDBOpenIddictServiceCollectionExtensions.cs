using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public static class DynamoDBOpenIddictServiceCollectionExtensions
    {
        public static DynamoDBOpenIddictIdentityBuilder<
            DynamoIdentityApplication, 
            DynamoIdentityAuthorization,
            DynamoIdentityToken,
            DynamoIdentityScope,
            DynamoIdentityDeviceCode>
            AddDynamoDBOpenIddictIdentity(this IServiceCollection services)
        {
            return new DynamoDBOpenIddictIdentityBuilder<
                DynamoIdentityApplication, 
                DynamoIdentityAuthorization, 
                DynamoIdentityToken,
                DynamoIdentityScope,
                DynamoIdentityDeviceCode>(services);
        }

        public static DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> 
            AddDynamoDBOpenIddictIdentity<TApplication, TAuthorization, TToken, TScope, TDeviceCode>(this IServiceCollection services)
            where TApplication : DynamoIdentityApplication
            where TAuthorization : DynamoIdentityAuthorization
            where TToken : DynamoIdentityToken
            where TScope : DynamoIdentityScope
            where TDeviceCode : DynamoIdentityDeviceCode
        {
            return new DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>(services);
        }
    }
}
