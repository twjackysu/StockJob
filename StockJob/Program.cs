using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using StockLib;
using System;
using System.Threading.Tasks;

namespace StockJob
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Stock Sync Start");
            try
            {
                var config = new ConfigurationBuilder()
                   .SetBasePath(System.IO.Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();

                var servicesProvider = BuildDi(config);
                using (servicesProvider as IDisposable)
                {
                    var runner = servicesProvider.GetRequiredService<StockRunner>();

                    await runner.OneTimeCrawler(new DateTime(2019, 1, 1));
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                logger.Info("Stock Sync End");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
            
        }
        private static IServiceProvider BuildDi(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            return new ServiceCollection()
               .AddTransient<StockRunner>() // Runner is the custom class
               .AddTransient<IHistoryBuilder, HistoryBuilder>()
               .AddTransient<IStockInfoBuilder, StockInfoBuilder>()
               .AddTransient<ITSEOTCListBuilder, TSEOTCListBuilder>()
               .AddDbContext<StockDBContext>()
               .AddLogging(loggingBuilder =>
               {
                   // configure Logging with NLog
                   loggingBuilder.ClearProviders();
                   loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                   loggingBuilder.AddNLog(config);
               })
               .BuildServiceProvider();
        }

    }
}
