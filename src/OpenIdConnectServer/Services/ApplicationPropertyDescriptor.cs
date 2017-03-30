using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer.Services
{
    public class ApplicationPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _descr;
        private readonly string _name;

        public ApplicationPropertyDescriptor(PropertyDescriptor descr)
            : base(descr)
        {
            this._descr = descr;

            var customBindingName = this._descr.Attributes[typeof(CustomBindingNameAttribute)] as CustomBindingNameAttribute;
            this._name = customBindingName != null ? customBindingName.PropertyName : this._descr.Name;
        }

        public override string Name
        {
            get { return this._name; }
        }

        protected override int NameHashCode
        {
            get { return this.Name.GetHashCode(); }
        }

        public override bool CanResetValue(object component)
        {
            return this._descr.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            return this._descr.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            this._descr.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            this._descr.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return this._descr.ShouldSerializeValue(component);
        }

        public override Type ComponentType
        {
            get { return this._descr.ComponentType; }
        }

        public override bool IsReadOnly
        {
            get { return this._descr.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return this._descr.PropertyType; }
        }
    }
}
