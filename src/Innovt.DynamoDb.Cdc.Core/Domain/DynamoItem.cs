// Innovt Company
// Author: Michel Borges
// Project: ConsoleAppTest

using System;

namespace Innovt.DynamoDb.Cdc.Core.Domain;

public class DynamoRecordItem
{
    public string awsRegion => "us-east-1";
    public string eventID => Guid.NewGuid().ToString();
    public string eventName => "INSERT";
    public string userIdentity => null;
    public string recordFormat => "application/json";
    public string eventSource => "dynamobulkinsert";//"aws:dynamodb";
    public string tableName { get; set; }
    public DynamoStreamRecord dynamodb { get; set; }

    public DynamoRecordItem(string tableName)
    {
        this.tableName = tableName;
    }
}
public class DynamoStreamRecord
{
    public long ApproximateCreationDateTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public object Keys { get; set; }
    public object NewImage { get; set; }
}
