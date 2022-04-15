using EnigmaBreaker.Configuration.Models;
using EnigmaBreaker.Services;
using EnigmaBreaker.Services.Fitness;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnigmaBreaker
{
    public class Startup
    {
        private static ServiceCollection _serviceCollection;
        private static ServiceProvider _serviceProvider;
        private Microsoft.Extensions.Logging.ILogger logger;
        public void Start()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("logs//" + DateTime.Today.ToString("yyyy") + "//" + DateTime.Today.ToString("MM") + "//" + DateTime.Today.ToString("dd") + "//Breaker-" + DateTime.Now.ToString("HHmmss") + ".txt")
                .MinimumLevel.Debug()
                .CreateLogger();

            RegisterServices();

            logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Start");
            runMyStuff();
        }
        public void Stop()
        {
            logger.LogInformation("End");
            DisposeServices();
            System.Environment.Exit(0);
        }

        public void runMyStuff()
        {
            var bs = _serviceProvider.GetRequiredService<Measuring>();
            bs.root();
            Stop();
        }

        private static void RegisterServices()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _serviceCollection = new ServiceCollection();

            var BasicSettings = new BasicConfiguration();
            configuration.Bind("BasicSettings", BasicSettings);
            _serviceCollection.AddSingleton(BasicSettings);
            var FitnessSettings = new FitnessConfiguration();
            configuration.Bind("FitnessSettings", FitnessSettings);
            _serviceCollection.AddSingleton(FitnessSettings);
            var physicalSettings = new PhysicalConfiguration();
            configuration.Bind("PhysicalConfiguration", physicalSettings);
            _serviceCollection.AddSingleton(physicalSettings);

            _serviceCollection.AddSingleton<Program>();
            _serviceCollection.AddLogging(cfg => cfg.AddSerilog()).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug);
            _serviceCollection.AddSingleton<BasicService>();
            _serviceCollection.AddSingleton<EncodingService>();
            _serviceCollection.AddSingleton<Measuring>();
            _serviceCollection.AddSingleton<CSVReaderService<Models.WeightFile>>();

            _serviceCollection.AddTransient<indexOfCoincidence>();
            _serviceCollection.AddTransient<singleCharFitness>();
            _serviceCollection.AddTransient<doubleCharFitness>();
            _serviceCollection.AddTransient<tripleCharFitness>();
            _serviceCollection.AddTransient<fourCharFitness>();
            _serviceCollection.AddTransient<ruleFitness>();
            _serviceCollection.AddTransient<weightFitness>();

            _serviceCollection.AddTransient<IFitness.FitnessResolver>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "IOC":
                        return serviceProvider.GetService<indexOfCoincidence>();
                    case "S":
                        return serviceProvider.GetService<singleCharFitness>();
                    case "BI":
                        return serviceProvider.GetService<doubleCharFitness>();
                    case "TRI":
                        return serviceProvider.GetService<tripleCharFitness>();
                    case "QUAD":
                        return serviceProvider.GetService<fourCharFitness>();
                    case "RULE":
                        return serviceProvider.GetService<ruleFitness>();
                    case "WEIGHT":
                        return serviceProvider.GetService<weightFitness>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            _serviceProvider = _serviceCollection.BuildServiceProvider(true);
        }
        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
