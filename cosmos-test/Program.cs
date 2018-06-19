using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mongo2CosmosExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var servicesCollection = new ServiceCollection();
            ConfigureServices(servicesCollection);

            var serviceProvider = servicesCollection.BuildServiceProvider();
            
            var app = serviceProvider.GetRequiredService<App>();

            app.Run();
        }
        private static void ConfigureServices(ServiceCollection servicesCollection)
        {

            servicesCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            servicesCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            servicesCollection.AddLogging((loggerBuilder) => {
                loggerBuilder.SetMinimumLevel(LogLevel.Trace);
                loggerBuilder.AddConsole();
                loggerBuilder.AddDebug();
            });

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true);
            
            IConfigurationRoot configuration = builder.Build();
            servicesCollection.AddOptions();
            servicesCollection.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            
            var settings = new AppSettings();
            configuration.GetSection("appSettings").Bind(settings);
            
            servicesCollection.AddTransient<App>();
            servicesCollection.AddTransient<ICreateTestData, Transferer>();
        }
    }
}

