using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Innovt.Core.CrossCutting.Ioc;
using Innovt.Core.Utilities;
using Innovt.CrossCutting.IOC;
using Innovt.DynamoDb.Cdc.Core.Application;
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

            return  System.Text.Json.JsonSerializer.Deserialize<List<SyncTableRequest>>( syncRequests );
        }


        private static void SetupIoc()
        {
            var container = new Container();

            container.AddModule(new DefaultIocModule());

            container.CheckConfiguration();

            IOCLocator.Initialize(container);
        }
        

        static async Task Main(string[] args)
        {
            if (!File.Exists(SyncRequestFileName))
            {
                System.Console.BackgroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"File {SyncRequestFileName} not found");
                return;
            }

            System.Console.BackgroundColor = ConsoleColor.White;
            try
            {
                var syncRequests = ReadSyncRequests();

                SetupIoc();

                var syncTableAppService = IOCLocator.Resolve<ISyncTableAppService>();
                
                await syncTableAppService.Handle(syncRequests);

            }
            catch (Exception e)
            {
                System.Console.BackgroundColor = System.Console.ForegroundColor;
                System.Console.WriteLine($"File {SyncRequestFileName} not found");
            }

            System.Console.WriteLine($"The Program has ended.");

        }
    }
}