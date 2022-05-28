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
            //create new logger
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("logs//" + DateTime.Today.ToString("yyyy") + "//" + DateTime.Today.ToString("MM") + "//" + DateTime.Today.ToString("dd") + "//Breaker-" + DateTime.Now.ToString("HHmmss") + ".txt")
                .MinimumLevel.Debug()
                .CreateLogger();

            RegisterServices();//register services for DI function

            logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Start");
            runMyStuff();//run my code
        }
        public void Stop()
        {
            logger.LogInformation("End");
            DisposeServices();
            System.Environment.Exit(0);
        }

        public void runMyStuff()
        {            
            if (_serviceProvider.GetRequiredService<BasicConfiguration>().isMeasure)//if the user wants to run experiments
            {
                var bs = _serviceProvider.GetRequiredService<Measuring>();//get the experiment class
                bs.root();//run the experiment class
            }
            else
            {
                var bs = _serviceProvider.GetRequiredService<BasicService>();//get the basic decryption class
                bs.root();//run the basic decryption class
            }
            Stop();
        }

        private static void RegisterServices()
        {
            //read json file
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _serviceCollection = new ServiceCollection();
            //get objects from json and register them
            var BasicSettings = new BasicConfiguration();
            configuration.Bind("BasicSettings", BasicSettings);
            _serviceCollection.AddSingleton(BasicSettings);
            var FitnessSettings = new FitnessConfiguration();
            configuration.Bind("FitnessSettings", FitnessSettings);
            _serviceCollection.AddSingleton(FitnessSettings);
            var physicalSettings = new PhysicalConfiguration();
            configuration.Bind("PhysicalConfiguration", physicalSettings);
            _serviceCollection.AddSingleton(physicalSettings);
            //register all the classes
            _serviceCollection.AddSingleton<Program>();
            _serviceCollection.AddLogging(cfg => cfg.AddSerilog()).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug);
            _serviceCollection.AddSingleton<BasicService>();
            _serviceCollection.AddSingleton<EncodingService>();
            _serviceCollection.AddSingleton<Measuring>();
            _serviceCollection.AddSingleton<CSVReaderService<Models.WeightFile>>();
            //register fitness functions
            _serviceCollection.AddSingleton<SharedUtilities>();
            _serviceCollection.AddTransient<indexOfCoincidence>();
            _serviceCollection.AddTransient<singleCharFitness>();
            _serviceCollection.AddTransient<doubleCharFitness>();
            _serviceCollection.AddTransient<tripleCharFitness>();
            _serviceCollection.AddTransient<fourCharFitness>();
            _serviceCollection.AddTransient<ruleFitness>();
            _serviceCollection.AddTransient<weightFitness>(); 
            //create IFitness resolver
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
