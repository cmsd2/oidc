using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Models
{
    [DynamoDBTable(Constants.DefaultAuthorizationTableName)]
    public class DynamoIdentityAuthorization
    {
        public DynamoIdentityAuthorization()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
        }

        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey("Subject-Application-index")]
        public string Application { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("Subject-Application-index")]
        public string Subject { get; set; }

        public List<string> Scopes { get; set; } = new List<string>();

        public List<string> Tokens { get; set; } = new List<string>();
    }
}
