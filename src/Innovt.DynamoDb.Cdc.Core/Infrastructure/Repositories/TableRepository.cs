// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Innovt.Cloud.AWS.Configuration;
using Innovt.Cloud.AWS.Dynamo;
using Innovt.Core.Collections;
using Innovt.Core.CrossCutting.Log;
using Innovt.DynamoDb.Cdc.Core.Domain;
using Innovt.DynamoDb.Cdc.Core.Infrastructure.Repositories.DataModels;
using QueryRequest = Innovt.Cloud.Table.QueryRequest;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Repositories;

public class TableRepository : Repository, ISyncTableRepository
{
    public TableRepository(ILogger logger, IAwsConfiguration configuration) : base(logger, configuration)
    {
    }

    public TableRepository(ILogger logger, IAwsConfiguration configuration, string region) : base(logger, configuration,
        region)
    {
    }

    public async Task<(Dictionary<string, object>, List<DynamoRecordItem>)> GetRecordsPaginated(
        SyncTableRequest syncTableRequest, Dictionary<string, object> paginationKey,
        CancellationToken cancellationToken)
    {
        if (syncTableRequest == null) throw new ArgumentNullException(nameof(syncTableRequest));

        var client = new AmazonDynamoDBClient(Configuration.GetCredential());

        var scanRequest = new Amazon.DynamoDBv2.Model.ScanRequest(syncTableRequest.TableName)
        {
            Limit = syncTableRequest.PageSize,
            Select = Select.ALL_ATTRIBUTES,
            ConsistentRead = true,
        };
        if (syncTableRequest.IndexName.IsNotNullOrEmpty())
            scanRequest.IndexName = syncTableRequest.IndexName;

        if (paginationKey is { })
            scanRequest.ExclusiveStartKey =
                paginationKey.ToDictionary(a => a.Key, b => ((AttributeValue) b.Value));

        var scanResult = await client.ScanAsync(scanRequest, cancellationToken);

        var documents = new List<DynamoRecordItem>();

        var describe = await client.DescribeTableAsync(new DescribeTableRequest(syncTableRequest.TableName),
            CancellationToken.None);

        foreach (var item in scanResult.Items)
        {
            var doc = new DynamoRecordItem(syncTableRequest.TableName);

            var keySchema = describe.Table.KeySchema.Select(a => a.AttributeName).ToList();

            var keys = new Dictionary<string, object>();
            var newImageItems = new Dictionary<string, object>();

            foreach (var key in item.Keys)
            {
                if (keySchema.Contains(key))
                {
                    keys.Add(key, AttributeHelper.FormatAttributeValue(item[key]));
                }

                newImageItems.Add(key, AttributeHelper.FormatAttributeValue(item[key]));
            }

            doc.dynamodb = new DynamoStreamRecord()
            {
                Keys = keys,
                NewImage = newImageItems
            };

            documents.Add(doc);
        }

        return (scanResult.LastEvaluatedKey?.ToDictionary(a => a.Key, b => ((object) b.Value)), documents);
    }

    public async Task SavePaginationKey(string tableName, Dictionary<string, object> paginationKey,
        CancellationToken cancellation)
    {
        if (tableName == null) throw new ArgumentNullException(nameof(tableName));
        if (paginationKey == null) throw new ArgumentNullException(nameof(paginationKey));

        //you can improve this is the key is not string
        var model = new SyncTableProgressDataModel()
        {
            BoundedContext = "CDC",
            TableName = tableName,
            PaginationKey = paginationKey.ToDictionary(a => a.Key, b => ((AttributeValue) b.Value).S)
        };

        await AddAsync(model, cancellation);
    }

    public async Task<Dictionary<string, object>> GetPaginationKey(string tableName,
        CancellationToken cancellationToken)
    {
        var queryRequest = new QueryRequest
        {
            KeyConditionExpression = "BoundedContext = :pk AND TableName=:sk",
            Filter = new {pk = "CDC", sk = tableName}
        };

        var item = await QueryAsync<SyncTableProgressDataModel>(queryRequest, cancellationToken);

        return item.SingleOrDefault()?.PaginationKey
            ?.ToDictionary(id => id.Key, id => (object) new AttributeValue(id.Value));
    }

    public async Task EnableKinesisDataStream(string tableName, string streamArn, CancellationToken cancellationToken)
    {
        if (tableName == null) throw new ArgumentNullException(nameof(tableName));
        if (streamArn == null) throw new ArgumentNullException(nameof(streamArn));

        var client = new AmazonDynamoDBClient(Configuration.GetCredential());

        await client.EnableKinesisStreamingDestinationAsync(new EnableKinesisStreamingDestinationRequest()
        {
            TableName = tableName,
            StreamArn = streamArn
        }, cancellationToken);
    }
}