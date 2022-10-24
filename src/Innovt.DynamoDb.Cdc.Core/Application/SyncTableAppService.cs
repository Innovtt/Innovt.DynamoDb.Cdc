using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Innovt.Core.Collections;
using Innovt.Core.CrossCutting.Log;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Cdc.Core.Application;

public class SyncTableAppService : ISyncTableAppService
{
    private readonly ISyncTableRepository tableRepository;
    private readonly ILogger logger;
    private readonly IDestinationService destinationService;

    public SyncTableAppService(ISyncTableRepository tableRepository, ILogger logger,
        IDestinationService destinationService)
    {
        this.tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.destinationService = destinationService ?? throw new ArgumentNullException(nameof(destinationService));
    }


    private async Task EnableKinesisStream(SyncTableRequest syncRequest, CancellationToken cancellationToken)
    {
        if (!syncRequest.EnableKinesisStream)
            return;
        logger.Info($"Enabling Kinesis Stream for table {syncRequest.TableName}");
        await tableRepository.EnableKinesisDataStream(syncRequest.TableName, syncRequest.KinesisStreamArn,
            cancellationToken);
        logger.Info($"Kinesis Stream for table {syncRequest.TableName} enabled.");
    }

    private bool CanMoveToNextPage(SyncTableRequest syncRequest, int pageCount,
        Dictionary<string, object> paginationKey)
    {
        if (syncRequest == null) throw new ArgumentNullException(nameof(syncRequest));

        return (paginationKey is { } && paginationKey.Keys.Any() &&
                (syncRequest.PageCount.GetValueOrDefault() > 0 && pageCount < syncRequest.PageCount));
    }

    public async Task Sync(SyncTableRequest syncRequest, CancellationToken cancellationToken)
    {
        if (syncRequest == null) throw new ArgumentNullException(nameof(syncRequest));
        if (cancellationToken.IsCancellationRequested) return;

        logger.Info($"Starting sync for table {syncRequest.TableName}");
        
        var paginationKey = await tableRepository.GetPaginationKey(syncRequest.TableName, cancellationToken);
        //in memory control in case of failure.
        var published = false;
        var paginationSaved = false;
        var pageCount = 0;
        do
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                logger.Info($"Starting sync for table {syncRequest.TableName}.");
                var scanResult =
                    await tableRepository.GetRecordsPaginated(syncRequest, paginationKey, cancellationToken);

                if (scanResult.Item2.IsNullOrEmpty())
                {
                    logger.Info($"Nothing to sync for table {syncRequest.TableName}.");
                    break;
                }

                if (!published)
                {
                    logger.Info($"Publishing records for table {syncRequest.TableName}.");
                    await destinationService.PublishRecords(scanResult.Item2, cancellationToken);
                    published = true;
                }

                if (!paginationSaved)
                {
                    logger.Info($"Saving pagination token for table {syncRequest.TableName}");
                    await tableRepository.SavePaginationKey(syncRequest.TableName, scanResult.Item1,
                        cancellationToken);
                    paginationSaved = true;
                    logger.Info($"Pagination token for table {syncRequest.TableName} saved.");
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(300));
                paginationKey = scanResult.Item1;
                paginationSaved = published = false; //before the
                pageCount++;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Critical error. The worker will sleep temporally.");
                Thread.Sleep(TimeSpan.FromSeconds(2)); //TODO: exponential backoffice here or poly
            }
        } while (CanMoveToNextPage(syncRequest, pageCount, paginationKey));

        try
        {
            await EnableKinesisStream(syncRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error enabling stream delivery for {syncRequest.TableName}.");
        }
       

        logger.Info($"The sync for table {syncRequest.TableName} has finished.");
    }
}