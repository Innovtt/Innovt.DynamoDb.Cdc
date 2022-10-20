using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Innovt.Cloud.AWS.Configuration;
using Innovt.Core.Collections;
using Innovt.Core.CrossCutting.Log;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Cdc.Core.Application;

public class SyncTableAppService : ISyncTableAppService
{
    private readonly ISyncTableRepository tableRepository;
    private readonly ILogger logger;
    private readonly IDestinationService destinationService;

    public SyncTableAppService(ISyncTableRepository tableRepository, ILogger logger, IDestinationService destinationService)
    {
        this.tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.destinationService = destinationService ?? throw new ArgumentNullException(nameof(destinationService));
    }


    public async Task Handle(SyncTableRequest syncRequest, CancellationToken cancellationToken)
    {
        if (syncRequest == null) throw new ArgumentNullException(nameof(syncRequest));
        if (cancellationToken.IsCancellationRequested) return;

        logger.Info($"Starting sync for table {syncRequest.TableName}");

        var startKey = await tableRepository.GetPaginationKey(syncRequest.TableName, cancellationToken);

        try
        {
            do
            {
                try
                {
                    logger.Info($"Starting sync for table {syncRequest.TableName}");
                    var scanResult = await tableRepository.GetRecordsPaginated(syncRequest, startKey, cancellationToken);

                    if (scanResult.Item2.IsNullOrEmpty())
                        break;


                    if (!published)
                    {
                        await cdc.PublishRecords(res.Item2, container.Resolve<IAwsConfiguration>(), CancellationToken.None);


                        published = true;
                        Console.WriteLine($"Records for table {tableName} published");
                    }

                    logger.Info($"Saving pagination token for table {syncRequest.TableName}");
                    await tableRepository.SavePaginationKey(syncRequest.TableName, scanResult.Item1, cancellationToken);
                    logger.Info($"Pagination token for table {syncRequest.TableName} saved.");

                    //move to the next page
                    startKey = scanResult.Item1;

                    Thread.Sleep(TimeSpan.FromMilliseconds(300));


                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error critical");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

            } while (startKey.Keys.Any());

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}