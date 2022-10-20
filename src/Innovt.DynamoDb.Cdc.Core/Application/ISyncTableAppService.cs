// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core
using System.Threading;
using System.Threading.Tasks;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Cdc.Core.Application;

public interface ISyncTableAppService
{
    Task Handle(SyncTableRequest syncRequest, CancellationToken cancellationToken);
}