// Innovt Company
// Author: Michel Borges
// Project: Innovt.DynamoDb.Cdc.Core

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Innovt.DynamoDb.Cdc.Core.Domain;

public interface ISyncTableRepository
{
    Task<(Dictionary<string, object>, List<DynamoRecordItem>)> GetRecordsPaginated(SyncTableRequest syncTableRequest, Dictionary<string, object> paginationKey, CancellationToken cancellationToken);

    Task SavePaginationKey(string tableName, Dictionary<string, object> paginationKey, CancellationToken cancellationToken);

    Task<Dictionary<string, object>> GetPaginationKey(string tableName, CancellationToken cancellationToken);
}