using System.Threading;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Console;

internal class WorkItem
{
    public SyncTableRequest SyncTableRequest { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public WorkItem(SyncTableRequest syncTableRequest, CancellationToken cancellationToken)
    {
        SyncTableRequest = syncTableRequest;
        CancellationToken = cancellationToken;
    }
}