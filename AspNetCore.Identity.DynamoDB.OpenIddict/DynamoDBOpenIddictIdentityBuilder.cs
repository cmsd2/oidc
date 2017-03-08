using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using AspNetCore.Identity.DynamoDB.OpenIddict.Stores;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public class DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
        where TApplication : DynamoIdentityApplication
        where TAuthorization : DynamoIdentityAuthorization
        where TToken : DynamoIdentityToken
        where TScope : DynamoIdentityScope
        where TDeviceCode : DynamoIdentityDeviceCode
    {
        public IServiceCollection Services { get; set; }
        public Type ApplicationType { get; set; }
        public Type AuthorizationType { get; set; }
        public Type TokenType { get; set; }
        public Type ScopeType { get; set; }
        public Type DeviceCodeType { get; set; }

        public DynamoDBOpenIddictIdentityBuilder(IServiceCollection services)
        {
            Services = services;
            ApplicationType = typeof(TApplication);
            AuthorizationType = typeof(TAuthorization);
            TokenType = typeof(TToken);
            ScopeType = typeof(TScope);
            DeviceCodeType = typeof(TDeviceCode);
        }

        private DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> AddScoped(Type serviceType, Type concreteType)
        {
            Services.AddScoped(serviceType, concreteType);
            return this;
        }

        private DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> AddSingleton(Type serviceType, Type concreteType)
        {
            Services.AddSingleton(serviceType, concreteType);
            return this;
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddApplicationStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictApplicationStore<>).MakeGenericType(ApplicationType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> 
            AddApplicationStore()
        {
            return AddApplicationStore<DynamoApplicationStore<TApplication, TToken>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> 
            AddAuthorizationStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictAuthorizationStore<>).MakeGenericType(AuthorizationType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddAuthorizationStore()
        {
            return AddAuthorizationStore<DynamoAuthorizationStore<TAuthorization>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> 
            AddScopeStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictScopeStore<>).MakeGenericType(ScopeType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode> 
            AddScopeStore()
        {
            return AddScopeStore<DynamoScopeStore<TScope>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddDeviceCodeStore<T>() where T : class
        {
            return AddSingleton(typeof(DynamoDeviceCodeStore<>).MakeGenericType(DeviceCodeType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddDeviceCodeStore()
        {
            return AddDeviceCodeStore<DynamoDeviceCodeStore<TDeviceCode>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddTokenStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictTokenStore<>).MakeGenericType(TokenType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope, TDeviceCode>
            AddTokenStore()
        {
            return AddTokenStore<DynamoTokenStore<TToken>>();
        }
    }
}
