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
    public class DynamoTokenStore<TToken> : IOpenIddictTokenStore<TToken>
        where TToken : DynamoIdentityToken
    {
        private IDynamoDBContext _context;

        public async Task<TToken> CreateAsync(TToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(token);

            return token;
        }

        public Task<TToken> CreateAsync(string type, string subject, CancellationToken cancellationToken)
        {
            var token = (TToken)Activator.CreateInstance(typeof(TToken));

            token.Type = type;
            token.Subject = subject;

            return CreateAsync(token, cancellationToken);
        }

        public Task<TToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            return _context.LoadAsync<TToken>(identifier);
        }

        public async Task<TToken[]> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TToken>(new QueryOperationConfig
            {
                IndexName = "Subject-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "Subject = :subject",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":subject", subject }
                    }
                },
                Limit = 1
            });
            var tokens = await search.GetRemainingAsync(cancellationToken);
            return tokens.ToArray();
        }

        public Task<string> GetIdAsync(TToken token, CancellationToken cancellationToken)
        {
            return Task.FromResult(token.Id);
        }

        public Task<string> GetSubjectAsync(TToken token, CancellationToken cancellationToken)
        {
            return Task.FromResult(token.Subject);
        }

        public Task<string> GetTokenTypeAsync(TToken token, CancellationToken cancellationToken)
        {
            return Task.FromResult(token.Type);
        }

        public async Task RevokeAsync(TToken token, CancellationToken cancellationToken)
        {
            await _context.DeleteAsync(token.Id);
        }

        public Task SetAuthorizationAsync(TToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Authorization = identifier;

            return Task.FromResult(0);
        }

        public Task SetClientAsync(TToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Application = identifier;

            return Task.FromResult(0);
        }

        public async Task UpdateAsync(TToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _context.SaveAsync(token);
        }

        public async Task<IList<TToken>> FindByApplicationAsync(string identifier, CancellationToken cancellationToken)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var search = _context.FromQueryAsync<TToken>(new QueryOperationConfig
            {
                IndexName = "Application-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "Application = :application",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":application", identifier }
                    }
                },
                Limit = 1
            });
            return await search.GetRemainingAsync(cancellationToken);
        }

        public Task EnsureInitializedAsync(IAmazonDynamoDB client, IDynamoDBContext context,
            string tokenTableName = Constants.DefaultTokenTableName)
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

            if (tokenTableName != Constants.DefaultTokenTableName)
            {
                AWSConfigsDynamoDB.Context.AddAlias(new TableAlias(tokenTableName, Constants.DefaultTokenTableName));
            }

            return EnsureInitializedImplAsync(client, tokenTableName);
        }

        private async Task EnsureInitializedImplAsync(IAmazonDynamoDB client, string tokenTableName)
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
                    IndexName = "Subject-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Subject", KeyType.HASH)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "Application-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Application", KeyType.HASH)
                    },
                    ProvisionedThroughput = defaultProvisionThroughput,
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "Authorization-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("Authorization", KeyType.HASH)
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

            if (!tableNames.Contains(tokenTableName))
            {
                await CreateTableAsync(client, tokenTableName, defaultProvisionThroughput, globalSecondaryIndexes);
                return;
            }

            var response = await client.DescribeTableAsync(new DescribeTableRequest { TableName = tokenTableName });
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
                await UpdateTableAsync(client, tokenTableName, indexUpdates);
            }
        }

        private async Task CreateTableAsync(IAmazonDynamoDB client, string tokenTableName,
            ProvisionedThroughput provisionedThroughput, List<GlobalSecondaryIndex> globalSecondaryIndexes)
        {
            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tokenTableName,
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
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Authorization",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                GlobalSecondaryIndexes = globalSecondaryIndexes
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't create table {tokenTableName}");
            }

            await DynamoUtils.WaitForActiveTableAsync(client, tokenTableName);
        }

        private async Task UpdateTableAsync(IAmazonDynamoDB client, string tokenTableName,
            List<GlobalSecondaryIndexUpdate> indexUpdates)
        {
            await client.UpdateTableAsync(new UpdateTableRequest
            {
                TableName = tokenTableName,
                GlobalSecondaryIndexUpdates = indexUpdates
            });

            await DynamoUtils.WaitForActiveTableAsync(client, tokenTableName);
        }
    }
}
