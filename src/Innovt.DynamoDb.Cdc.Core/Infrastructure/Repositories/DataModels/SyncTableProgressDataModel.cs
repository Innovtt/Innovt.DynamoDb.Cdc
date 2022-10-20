// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core

using Amazon.DynamoDBv2.DataModel;
using Innovt.Cloud.Table;
using System.Collections.Generic;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Repositories.DataModels;

[DynamoDBTable("LegacyMigrator")]
public class SyncTableProgressDataModel: ITableMessage
{
    [DynamoDBIgnore]
    public string Id { get; set; }

    [DynamoDBHashKey]
    public string BoundedContext { get; set; }

    [DynamoDBRangeKey] 
    public string TableName { get; set; }

    public Dictionary<string, string> PaginationKey { get; set; }

}