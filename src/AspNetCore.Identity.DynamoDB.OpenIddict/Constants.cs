using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    internal static class Constants
    {
        public const string DefaultApplicationTableName = "applications";
        public const string DefaultAuthorizationTableName = "authorizations";
        public const string DefaultTokenTableName = "tokens";
        public const string DefaultScopeTableName = "scopes";
        public const string DefaultDeviceCodeTableName = "deviceCodes";
    }
}
