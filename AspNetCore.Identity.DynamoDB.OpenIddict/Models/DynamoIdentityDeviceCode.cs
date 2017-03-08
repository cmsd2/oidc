using Amazon.DynamoDBv2.DataModel;
using AspNetCore.Identity.DynamoDB.Converters;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Models
{
    [DynamoDBTable(Constants.DefaultDeviceCodeTableName)]
    public class DynamoIdentityDeviceCode
    {
        public DynamoIdentityDeviceCode()
        {
            Id = Guid.NewGuid().ToString();
            CreatedOn = DateTimeOffset.Now;
            ExpiresAt = CreatedOn.AddSeconds(300);
            UserCode = NewCode(9);
            DeviceCode = Guid.NewGuid().ToString();
        }

        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedOn { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset ExpiresAt { get; set; }
        
        public string Application { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("DeviceCode-index")]
        public string DeviceCode { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("UserCode-index")]
        public string UserCode { get; set; }

        [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset AuthorizedOn { get; set; }

        public string Subject { get; set; }

        public List<string> Scopes { get; set; }

        public static string NewCode(int length)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[length];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }
    }
}
