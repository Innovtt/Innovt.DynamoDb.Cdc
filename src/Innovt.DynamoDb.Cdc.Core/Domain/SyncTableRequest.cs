namespace Innovt.DynamoDb.Cdc.Core.Domain
{
    public class SyncTableRequest
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public int PageSize { get; set; }
        public bool EnableKinesisStream { get; set; }
        public string KinesisStreamArn { get; set; }
        public int? PageCount { get; set; }
    }
}