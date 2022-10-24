using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Innovt.Core.Utilities;
using Innovt.CrossCutting.IOC;
using Innovt.DynamoDb.Cdc.Core.Domain;
using Innovt.DynamoDb.Cdc.Core.Infrastructure.Ioc;

namespace Innovt.DynamoDb.Console
{
    public class Program
    {
        const string SyncRequestFileName = "SyncRequest.json";

        private static IList<SyncTableRequest> ReadSyncRequests()
        {
            var syncRequests = File.OpenText(SyncRequestFileName).ReadToEnd();

            if (syncRequests.IsNullOrEmpty())
                throw new Exception($"The {SyncRequestFileName} is empty or null.");

            return System.Text.Json.JsonSerializer.Deserialize<List<SyncTableRequest>>(syncRequests);
        }


        private static Container SetupIoc()
        {
            var container = new Container();

            container.AddModule(new DefaultIocModule());

            container.CheckConfiguration();

            return container;
        }

        static void Main(string[] args)
        {
            if (!File.Exists(SyncRequestFileName))
            {
                System.Console.WriteLine($"File {SyncRequestFileName} not found");
                return;
            }
            
            var cancellationTokenSource = new CancellationTokenSource(); //Todo: cancel if press some key

            try
            {
                var syncRequests = ReadSyncRequests();
                var worker = new Worker(SetupIoc());

                var t = new Task(() => worker.Run(syncRequests, cancellationTokenSource.Token));

                t.Start();

                t.WaitAsync(cancellationTokenSource.Token);
                
                do
                {
                    System.Console.WriteLine("Press C to cancel");

                    var key = System.Console.ReadKey();

                    if (key.Key == ConsoleKey.C)
                    {
                        cancellationTokenSource.Cancel();
                    }else
                    {
                        System.Console.WriteLine("Invalid Key Input.");
                    }

                } while (!cancellationTokenSource.IsCancellationRequested);
                System.Console.WriteLine($"The Program has ended.");

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            System.Console.WriteLine($"The Program has ended.");
        }
    }
}