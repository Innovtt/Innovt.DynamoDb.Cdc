using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.Runtime;
using Innovt.Cloud.AWS.Configuration;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Services;

//You can create another service and include a configuration class to your 
public class KinesisService : IDestinationService
{
    private readonly AmazonKinesisClient kinesisClient;

    public KinesisService(IAwsConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        kinesisClient = new AmazonKinesisClient(configuration.GetCredential(), new AmazonKinesisConfig()
        {
            ThrottleRetries = true,
            RetryMode = RequestRetryMode.Standard,
            MaxErrorRetry = 3
        });
    }

    public async Task PublishRecords(IList<DynamoRecordItem> records, CancellationToken cancellationToken)
    {
        var dataStreams = records.ToList();

        var request = new PutRecordsRequest
        {
            StreamName = "CdcStream", //Your CDC stream
            Records = CreatePutRecords(dataStreams)
        };

        var results = await kinesisClient.PutRecordsAsync(request, cancellationToken).ConfigureAwait(false);

        if (results.FailedRecordCount > 0)
        {
            throw new Exception("Publish failure.");
        }
    }

    private List<PutRecordsRequestEntry> CreatePutRecords(IEnumerable<DynamoRecordItem> dataList)
    {
        var request = new List<PutRecordsRequestEntry>();

        foreach (var data in dataList)
        {
            var dataAsBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            using (var ms = new MemoryStream(dataAsBytes))
            {
                request.Add(new PutRecordsRequestEntry
                {
                    Data = ms,
                    PartitionKey = "migration" // you can change here or add another configurarion class 
                });
            }
        }

        return request;
    }
}