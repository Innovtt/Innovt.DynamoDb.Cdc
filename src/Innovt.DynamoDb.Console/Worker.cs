using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Innovt.Core.CrossCutting.Log;
using Innovt.Core.Exceptions;
using Innovt.CrossCutting.IOC;
using Innovt.DynamoDb.Cdc.Core.Application;
using Innovt.DynamoDb.Cdc.Core.Domain;

namespace Innovt.DynamoDb.Console;

public class Worker
{
    private readonly Container container;
    private readonly Semaphore semaphore;
    private readonly ILogger logger;

    public Worker(Container container)
    {
        this.container = container ?? throw new ArgumentNullException(nameof(container));
        this.logger = container.Resolve<ILogger>();
        this.semaphore = new Semaphore(5, 10); // you can change here.
    }


    private void ProcessSync(object workItemRequest)
    {
        if (workItemRequest == null) throw new ArgumentNullException(nameof(workItemRequest));

        var workItem = (WorkItem)workItemRequest;

        try
        {
            logger.Info($"Starting thread for sync table {workItem.SyncTableRequest.TableName}");

            var syncTableService = container.Resolve<ISyncTableAppService>();

            if (syncTableService is null)
                throw new CriticalException("Impossible to instantiate the ISyncTableAppService.");

            (syncTableService.Sync(workItem.SyncTableRequest, workItem.CancellationToken)).Wait(
                workItem.CancellationToken);

        }
        catch(Exception ex) 
        {
            logger.Error(ex, $"Thread failure  for table.{workItem.SyncTableRequest.TableName}");
        }
        finally
        {
            semaphore.Release();
            logger.Info($"Releasing Semaphore. Table {workItem.SyncTableRequest.TableName}");
        }
    }

    public void Run(IList<SyncTableRequest> syncTableRequests, CancellationToken cancellationToken)
    {
        if (syncTableRequests == null) throw new ArgumentNullException(nameof(syncTableRequests));

        foreach (var syncTableRequest in syncTableRequests)
        {
            try
            {
                semaphore.WaitOne(Timeout.Infinite, true);
                ThreadPool.QueueUserWorkItem(ProcessSync,new WorkItem(syncTableRequest, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing table sync {syncTableRequest.TableName}.", ex);
            }
        }
    }
}