using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Util;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using OpenIddict.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DynamoDB.OpenIddict.Stores
{
    public class DynamoDeviceCodeStore<TCode>
        where TCode : DynamoIdentityDeviceCode
    {
        private IDynamoDBContext _context;

        public async Task<TCode> CreateAsync(TCode code, CancellationToken cancellationToken)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(code);

            return code;
        }

        public Task<TCode> CreateAsync(string application, List<string> scopes, CancellationToken cancellationToken)
        {
            var code = (TCode)Activator.CreateInstance(typeof(TCode));
            
            code.Application = application;
            code.Scopes = scopes;

            return CreateAsync(code, cancellationToken);
        }

        public Task<TCode> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _context.LoadAsync<TCode>(identifier);
        }

        public async Task<TCode> FindByUserCodeAsync(string user_code, CancellationToken cancellationToken)
        {
            if (user_code == null)
            {
                throw new ArgumentNullException(nameof(user_code));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TCode>(new QueryOperationConfig
            {
                IndexName = "UserCode-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "UserCode = :user_code",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":user_code", user_code }
                    }
                },
                Limit = 1
            });
            var codes = await search.GetRemainingAsync(cancellationToken);
            return codes.Find(code => code.ExpiresAt > DateTimeOffset.Now);
        }

        public async Task<TCode> FindByDeviceCodeAsync(string device_code, CancellationToken cancellationToken)
        {
            if (device_code == null)
            {
                throw new ArgumentNullException(nameof(device_code));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TCode>(new QueryOperationConfig
            {
                IndexName = "DeviceCode-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "DeviceCode = :device_code",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":device_code", device_code }
                    }
                },
                Limit = 1
            });
            var codes = await search.GetRemainingAsync(cancellationToken);
            return codes.Find(code => code.ExpiresAt > DateTimeOffset.Now);
        }

        public Task<string> GetIdAsync(TCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Id);
        }

        public Task<string> GetApplicationAsync(TCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Application);
        }

        public Task<string> GetUserCodeAsync(TCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.UserCode);
        }

        public Task<string> GetDeviceCodeAsync(TCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.DeviceCode);
        }

        public Task<List<string>> GetScopesAsync(TCode code, CancellationToken cancellationToken)
        {
            return Task.FromResult(code.Scopes);
        }

        public async Task Revoke(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.DeleteAsync<TCode>(identifier, cancellationToken);
        }

        public Task EnsureInitializedAsync(IAmazonDynamoDB client, IDynamoDBContext context,
            string codesTableName = Constants.DefaultDeviceCodeTableName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;

            if (codesTableName != Constants.DefaultDeviceCodeTableName)
            {
                AWSConfigsDynamoDB.Context.AddAlias(new TableAlias(codesTableName, Constants.DefaultDeviceCodeTableName));
            }

            return EnsureInitializedImplAsync(client, codesTableName);
        }

        private async Task EnsureInitializedImplAsync(IAmazonDynamoDB client, string codesTableName)
        {
            var defaultProvisionThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5
            };
            var globalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "DeviceCode-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("DeviceCode", KeyType.HASH)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "UserCode-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("UserCode", KeyType.HASH)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                }
            };

            var tablesResponse = await client.ListTablesAsync();
            if (tablesResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Couldn't get list of tables");
            }
            var tableNames = tablesResponse.TableNames;

            if (!tableNames.Contains(codesTableName))
            {
                await CreateTableAsync(client, codesTableName, defaultProvisionThroughput, globalSecondaryIndexes);
                return;
            }

            var response = await client.DescribeTableAsync(new DescribeTableRequest { TableName = codesTableName });
            var table = response.Table;

            var indexesToAdd =
                globalSecondaryIndexes.Where(
                    g => !table.GlobalSecondaryIndexes.Exists(gd => gd.IndexName.Equals(g.IndexName)));
            var indexUpdates = indexesToAdd.Select(index => new GlobalSecondaryIndexUpdate
            {
                Create = new CreateGlobalSecondaryIndexAction
                {
                    IndexName = index.IndexName,
                    KeySchema = index.KeySchema,
                    ProvisionedThroughput = index.ProvisionedThroughput,
                    Projection = index.Projection
                }
            }).ToList();

            if (indexUpdates.Count > 0)
            {
                await UpdateTableAsync(client, codesTableName, indexUpdates);
            }
        }

        private async Task CreateTableAsync(IAmazonDynamoDB client, string codesTableName,
            ProvisionedThroughput provisionedThroughput, List<GlobalSecondaryIndex> globalSecondaryIndexes)
        {
            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = codesTableName,
                ProvisionedThroughput = provisionedThroughput,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "DeviceCode",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "UserCode",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                GlobalSecondaryIndexes = globalSecondaryIndexes
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't create table {codesTableName}");
            }

            await DynamoUtils.WaitForActiveTableAsync(client, codesTableName);
        }

        public async Task Authorize(DynamoIdentityDeviceCode deviceCode, string userId, CancellationToken cancellationToken)
        {
            if (deviceCode == null)
            {
                throw new ArgumentNullException(nameof(deviceCode));
            }

            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (deviceCode.AuthorizedOn != default(DateTimeOffset))
            {
                throw new InvalidOperationException("device code has already been authorised");
            }

            cancellationToken.ThrowIfCancellationRequested();

            deviceCode.AuthorizedOn = DateTimeOffset.Now;
            deviceCode.Subject = userId;

            await _context.SaveAsync(deviceCode);
        }

        private async Task UpdateTableAsync(IAmazonDynamoDB client, string codesTableName,
            List<GlobalSecondaryIndexUpdate> indexUpdates)
        {
            await client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = codesTableName,
                GlobalSecondaryIndexUpdates = indexUpdates
            });

            await DynamoUtils.WaitForActiveTableAsync(client, codesTableName);
        }
    }
}
