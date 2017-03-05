using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    [DynamoDBTable(Constants.DefaultApplicationTableName)]
    public class DynamoIdentityApplication
    {

        public DynamoIdentityApplication()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
            ClientId = Guid.NewGuid().ToString();
            ClientSecret = Guid.NewGuid().ToString();
        }

        public DynamoIdentityApplication(string name) : this()
        {
            DisplayName = name;
        }

        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey("ClientId-DeletedOn-index",
            "LogoutRedirectUri-DeletedOn-index",
            Converter = typeof(DateTimeOffsetConverter))]
        public DateTimeOffset DeletedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("LogoutRedirectUri-DeletedOn-index")]
        public string LogoutRedirectUri { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("ClientId-DeletedOn-index")]
        public string ClientId { get; set; }

        [DynamoDBVersion]
        public int? VersionNumber { get; set; }

        [DynamoDBIgnore]
        public List<string> Tokens { get; set; }

        [DynamoDBIgnore]
        public List<string> Authorizations { get; set; }

        public void Delete()
        {
            if (DeletedOn != default(DateTimeOffset))
            {
                throw new InvalidOperationException($"Role '{Id}' has already been deleted.");
            }

            DeletedOn = DateTimeOffset.Now;
        }
        
        public string ClientSecret { get; set; }
        
        public string DisplayName { get; set; }
        
        public string RedirectUri { get; set; }
        
        public string Type { get; set; }
    }
}
