using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.LDAP.Tests
{
    public class LdapAttributeSetTests
    {
        [Fact]
        public void Ldap_Attribute_Set_Should_Be_Empty()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            Assert.True(attributeSet.IsEmpty());
        }

        [Fact]
        public void Ldap_Attribute_Set_Should_Not_Be_Empty()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            attributeSet.Add(new LdapAttribute("objectclass", "inetOrgPerson"));
            Assert.False(attributeSet.IsEmpty());
        }

        [Fact]
        public void Ldap_Attribute_Set_Count_Should_One()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            attributeSet.Add(new LdapAttribute("objectclass", "inetOrgPerson"));
            Assert.Equal(1, attributeSet.Count);
        }

        [Fact]
        public void Ldap_Attribute_Set_Should_Contain_Attribute()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            var attr = new LdapAttribute("objectclass", "inetOrgPerson");
            attributeSet.Add(attr);
            Assert.True(attributeSet.Contains(attr));
        }

        [Fact]
        public void Ldap_Attribute_Set_Should_Be_Cleared()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            var attr = new LdapAttribute("objectclass", "inetOrgPerson");
            attributeSet.Add(attr);
            attributeSet.Clear();
            Assert.True(attributeSet.IsEmpty());
        }

        [Fact]
        public void Ldap_Attribute_Set_Attribute_Should_Be_Removed()
        {
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            var attr = new LdapAttribute("objectclass", "inetOrgPerson");
            attributeSet.Add(attr);
            attributeSet.Remove(attr);
            Assert.False(attributeSet.Contains(attr));
        }

        [Fact]
        public void Ldap_Attribute_Set_Attribute_Should_Be_Taken_By_Name()
        {
            var attrName = "objectclass";
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            var attr = new LdapAttribute(attrName, "inetOrgPerson");
            attributeSet.Add(attr);
            var attrFromContainer = attributeSet.getAttribute(attrName);
            Assert.Equal(attrName, attrFromContainer.Name);
        }

        [Fact]
        public void Ldap_Attribute_Set_Should_Be_Cloned()
        {
            var attrName = "objectclass";
            LdapAttributeSet attributeSet = new LdapAttributeSet();
            var attr = new LdapAttribute(attrName, "inetOrgPerson");
            attributeSet.Add(attr);
            
            var attributeSetClone = (LdapAttributeSet)attributeSet.Clone();

            bool equals = attributeSet == attributeSetClone;
            Assert.False(equals);

            var attrFromContainer = attributeSet.getAttribute(attrName);
            var attrFromCloneContainer = attributeSetClone.getAttribute(attrName);
            bool equalsAttrs = attrFromContainer == attrFromCloneContainer;
            Assert.False(equalsAttrs);
        }
    }
}
