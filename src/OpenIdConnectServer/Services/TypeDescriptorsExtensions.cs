using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public static class TypeDescriptorsExtensions
    {
        public static void AddTypeDescriptors(this IServiceCollection app)
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;

            // Assume, this code and all models are in one assembly
            var types = assembly
                .GetTypes()
                .Where(t => t.GetProperties().Any(p => p.IsDefined(typeof(CustomBindingNameAttribute))));

            foreach (var type in types)
            {
                var defaultProvider = TypeDescriptor.GetProvider(type);
                TypeDescriptor.AddProvider(new ApplicationTypeDescriptorProvider(defaultProvider), type);
            }
        }
    }
}
