using Innovt.Cloud.AWS.Configuration;
using Innovt.Core.CrossCutting.Ioc;
using Innovt.Core.CrossCutting.Log;
using Innovt.CrossCutting.Log.Serilog;
using Innovt.DynamoDb.Cdc.Core.Application;
using Innovt.DynamoDb.Cdc.Core.Domain;
using Innovt.DynamoDb.Cdc.Core.Infrastructure.Repositories;
using Innovt.DynamoDb.Cdc.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Ioc
{
    public class DefaultIocModule : IOCModule
    {
        public DefaultIocModule()
        {
            var services = GetServices();

            services.AddTransient<ISyncTableAppService, SyncTableAppService>();
            services.AddSingleton<ILogger, Logger>();
            services.AddTransient<ISyncTableRepository, TableRepository>();
            services.AddTransient<IDestinationService, KinesisService>();
            //Add your profile or credentials here.
            services.AddSingleton<IAwsConfiguration>(a => new DefaultAWSConfiguration("antecipa-prod"));
        }
    }
}