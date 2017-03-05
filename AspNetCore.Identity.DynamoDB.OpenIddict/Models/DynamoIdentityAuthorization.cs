using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Models
{
    [DynamoDBTable(Constants.DefaultAuthorizationTableName)]
    public class DynamoIdentityAuthorization : OpenIddictAuthorization<string, string, string>
    {
        public DynamoIdentityAuthorization()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
        }

        [DynamoDBHashKey]
        public override string Id { get => base.Id; set => base.Id = value; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey("Subject-Application-index")]
        public override string Application { get => base.Application; set => base.Application = value; }

        [DynamoDBGlobalSecondaryIndexHashKey("Subject-Application-index")]
        public override string Subject { get => base.Subject; set => base.Subject = value; }
    }
}
