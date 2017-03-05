using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public class DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope>
        where TApplication : DynamoIdentityApplication
        where TAuthorization : DynamoIdentityAuthorization
        where TToken : DynamoIdentityToken
        where TScope : DynamoIdentityScope
    {
        public IServiceCollection Services { get; set; }
        public Type ApplicationType { get; set; }
        public Type AuthorizationType { get; set; }
        public Type TokenType { get; set; }
        public Type ScopeType { get; set; }

        public DynamoDBOpenIddictIdentityBuilder(IServiceCollection services)
        {
            Services = services;
            ApplicationType = typeof(TApplication);
            AuthorizationType = typeof(TAuthorization);
            TokenType = typeof(TToken);
            ScopeType = typeof(TScope);
        }

        private DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> AddScoped(Type serviceType, Type concreteType)
        {
            Services.AddScoped(serviceType, concreteType);
            return this;
        }

        private DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> AddSingleton(Type serviceType, Type concreteType)
        {
            Services.AddSingleton(serviceType, concreteType);
            return this;
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope>
            AddApplicationStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictApplicationStore<>).MakeGenericType(ApplicationType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> 
            AddApplicationStore()
        {
            return AddApplicationStore<DynamoApplicationStore<TApplication, TToken>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> 
            AddAuthorizationStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictAuthorizationStore<>).MakeGenericType(AuthorizationType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope>
            AddAuthorizationStore()
        {
            return AddAuthorizationStore<DynamoAuthorizationStore<TAuthorization>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> 
            AddScopeStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictScopeStore<>).MakeGenericType(ScopeType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope> 
            AddScopeStore()
        {
            return AddScopeStore<DynamoScopeStore<TScope>>();
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope>
            AddTokenStore<T>() where T : class
        {
            return AddSingleton(typeof(IOpenIddictTokenStore<>).MakeGenericType(TokenType), typeof(T));
        }

        public DynamoDBOpenIddictIdentityBuilder<TApplication, TAuthorization, TToken, TScope>
            AddTokenStore()
        {
            return AddTokenStore<DynamoTokenStore<TToken>>();
        }
    }
}
