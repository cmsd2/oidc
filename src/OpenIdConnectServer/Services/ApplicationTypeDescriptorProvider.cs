using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public class ApplicationTypeDescriptorProvider : TypeDescriptionProvider
    {
        private readonly TypeDescriptionProvider _defaultProvider;

        public ApplicationTypeDescriptorProvider(TypeDescriptionProvider defaultProvider)
        {
            this._defaultProvider = defaultProvider;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new ApplicationTypeDescriptor(this._defaultProvider.GetTypeDescriptor(objectType, instance));
        }
    }
}
