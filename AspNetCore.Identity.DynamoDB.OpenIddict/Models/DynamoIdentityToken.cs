using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Models
{
    [DynamoDBTable(Constants.DefaultTokenTableName)]
    public class DynamoIdentityToken
    {
        public DynamoIdentityToken()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
        }

        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("Subject-index")]
        public string Subject { get; set; }
        
        [DynamoDBGlobalSecondaryIndexHashKey("Application-index")]
        public string Application { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("Authorization-index")]
        public string Authorization { get; set; }
        
        public string Type { get; set; }
    }
}
