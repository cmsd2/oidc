using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public class ApplicationTypeDescriptor : CustomTypeDescriptor
    {
        public ApplicationTypeDescriptor(ICustomTypeDescriptor parent)
            : base(parent)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return Wrap(base.GetProperties());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return Wrap(base.GetProperties(attributes));
        }

        private static PropertyDescriptorCollection Wrap(PropertyDescriptorCollection src)
        {
            var wrapped = src.Cast<PropertyDescriptor>()
                             .Select(pd => (PropertyDescriptor)new ApplicationPropertyDescriptor(pd))
                             .ToArray();

            return new PropertyDescriptorCollection(wrapped);
        }
    }
}
