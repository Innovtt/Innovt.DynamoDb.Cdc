using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
                System.Console.BackgroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"File {SyncRequestFileName} not found");
                return;
            }

            System.Console.BackgroundColor = ConsoleColor.White;
            var cancellationTokenSource = new CancellationTokenSource(); //Todo: cancel if press some key

            try
            {
                var syncRequests = ReadSyncRequests();

                new Worker(SetupIoc()).Run(syncRequests, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                System.Console.BackgroundColor = System.Console.ForegroundColor;
                System.Console.WriteLine(ex.Message);
            }

            System.Console.WriteLine($"The Program has ended.");
        }
    }
}