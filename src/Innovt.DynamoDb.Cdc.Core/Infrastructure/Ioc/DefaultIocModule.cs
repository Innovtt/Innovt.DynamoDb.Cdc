using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovt.Cloud.AWS.Configuration;
using Innovt.Core.CrossCutting.Ioc;
using Innovt.Core.CrossCutting.Log;
using Innovt.CrossCutting.Log.Serilog;
using Innovt.DynamoDb.Cdc.Core.Application;
using Innovt.DynamoDb.Cdc.Core.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Innovt.DynamoDb.Cdc.Core.Infrastructure.Ioc
{
    public  class DefaultIocModule: IOCModule
    {
        public DefaultIocModule()
        {
            var services = GetServices();
            
            services.AddScoped<ISyncTableAppService, SyncTableAppService>();
            services.AddScoped<ILogger, Logger>();
            //services.AddScoped<ITableRepository, Logger>();
            //services.AddScoped<IDestinationService, Logger>();
            
            //Add your profile or credentials here.
            services.AddScoped<IAwsConfiguration>(a=>new DefaultAWSConfiguration("antecipa-prod"));
        }
    }
}
