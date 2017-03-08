using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenIdConnectServer
{
    public class DynamoDbSettings
    {
        public string ServiceUrl { get; set; }
        public string UsersTableName { get; set; }
        public string RolesTableName { get; set; }
        public string RoleUsersTableName { get; set; }
        public string ApplicationsTableName { get; set; }
        public string AuthorizationsTableName { get; set; }
        public string ScopesTableName { get; set; }
        public string TokensTableName { get; set; }
        public string DeviceCodesTableName { get; set; }
    }
}
