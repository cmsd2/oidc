using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Models
{
    public class DynamoIdentityToken : OpenIddictToken<string, string, string>
    {
        public DynamoIdentityToken()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
        }

        [DynamoDBHashKey]
        public override string Id { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("Subject-index")]
        public override string Subject { get => base.Subject; set => base.Subject = value; }
        
        [DynamoDBGlobalSecondaryIndexHashKey("Application-index")]
        public override string Application { get => base.Application; set => base.Application = value; }

        [DynamoDBGlobalSecondaryIndexHashKey("Authorization-index")]
        public override string Authorization { get => base.Authorization; set => base.Authorization = value; }
    }
}
