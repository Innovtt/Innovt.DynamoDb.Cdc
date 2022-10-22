// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Innovt.DynamoDb.Cdc.Core.Domain;

public interface IDestinationService
{
    Task PublishRecords(IList<DynamoRecordItem> records, CancellationToken cancellationToken);
}