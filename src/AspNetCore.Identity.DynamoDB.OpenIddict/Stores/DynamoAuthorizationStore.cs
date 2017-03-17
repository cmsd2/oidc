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

namespace AspNetCore.Identity.DynamoDB.OpenIddict
{
    public class DynamoAuthorizationStore<TAuthorization> : IOpenIddictAuthorizationStore<TAuthorization>
        where TAuthorization : DynamoIdentityAuthorization
    {
        private IDynamoDBContext _context;

        public async Task<TAuthorization> CreateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(authorization);

            return authorization;
        }

        public async Task<TAuthorization> FindAsync(string subject, string client, CancellationToken cancellationToken)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TAuthorization>(new QueryOperationConfig
            {
                IndexName = "Subject-Application-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "Subject = :subject AND Application = :application",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":subject", subject },
                        { ":application", client }
                    }
                },
                Limit = 1
            });
            var applications = await search.GetRemainingAsync(cancellationToken);
            return applications?.FirstOrDefault();
        }

        public async Task RevokeAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.DeleteAsync(authorization, cancellationToken);
        }

        public async Task<TAuthorization> UpdateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(authorization, cancellationToken);

            return authorization;
        }

        public Task<TAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _context.LoadAsync<TAuthorization>(identifier, cancellationToken);
        }

        public Task<string> GetIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            return Task.FromResult(authorization.Id);
        }

        public Task<string> GetSubjectAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            return Task.FromResult(authorization.Subject);
        }

        public Task EnsureInitializedAsync(IAmazonDynamoDB client, IDynamoDBContext context,
            string authorizationTableName = Constants.DefaultAuthorizationTableName)
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

            if (authorizationTableName != Constants.DefaultAuthorizationTableName)
            {
                AWSConfigsDynamoDB.Context.AddAlias(new TableAlias(authorizationTableName, Constants.DefaultAuthorizationTableName));
            }

            return EnsureInitializedImplAsync(client, authorizationTableName);
        }

        private async Task EnsureInitializedImplAsync(IAmazonDynamoDB client, string authorizationTableName)
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
                    IndexName = "Subject-Application-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Subject", KeyType.HASH),
                        new KeySchemaElement("Application", KeyType.RANGE)
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

            if (!tableNames.Contains(authorizationTableName))
            {
                await CreateTableAsync(client, authorizationTableName, defaultProvisionThroughput, globalSecondaryIndexes);
                return;
            }

            var response = await client.DescribeTableAsync(new DescribeTableRequest { TableName = authorizationTableName });
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
                await UpdateTableAsync(client, authorizationTableName, indexUpdates);
            }
        }

        private async Task CreateTableAsync(IAmazonDynamoDB client, string authorizationTableName,
            ProvisionedThroughput provisionedThroughput, List<GlobalSecondaryIndex> globalSecondaryIndexes)
        {
            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = authorizationTableName,
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
                        AttributeName = "Subject",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Application",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                GlobalSecondaryIndexes = globalSecondaryIndexes
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't create table {authorizationTableName}");
            }

            await DynamoUtils.WaitForActiveTableAsync(client, authorizationTableName);
        }

        private async Task UpdateTableAsync(IAmazonDynamoDB client, string authorizationTableName,
            List<GlobalSecondaryIndexUpdate> indexUpdates)
        {
            await client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = authorizationTableName,
                GlobalSecondaryIndexUpdates = indexUpdates
            });

            await DynamoUtils.WaitForActiveTableAsync(client, authorizationTableName);
        }
    }
}
