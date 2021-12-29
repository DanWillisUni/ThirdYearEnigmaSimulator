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
            var bs = _serviceProvider.GetRequiredService<BasicService>();
            string plaintext = @"One thing was certain, that the white kitten had had nothing to do with it:—it was the black kitten’s fault entirely. For the white kitten had been having its face washed by the old cat for the last quarter of an hour (and bearing it pretty well, considering); so you see that it couldn’t have had any hand in the mischief.
 
The way Dinah washed her children’s faces was this: first she held the poor thing down by its ear with one paw, and then with the other paw she rubbed its face all over, the wrong way, beginning at the nose: and just now, as I said, she was hard at work on the white kitten, which was lying quite still and trying to purr—no doubt feeling that it was all meant for its good.
 
But the black kitten had been finished with earlier in the afternoon, and so, while Alice was sitting curled up in a corner of the great arm-chair, half talking to herself and half asleep, the kitten had been having a grand game of romps with the ball of worsted Alice had been trying to wind up, and had been rolling it up and down till it had all come undone again; and there it was, spread over the hearth-rug, all knots and tangles, with the kitten running after its own tail in the middle.
 
“Oh, you wicked little thing!” cried Alice, catching up the kitten, and giving it a little kiss to make it understand that it was in disgrace. “Really, Dinah ought to have taught you better manners! You ought, Dinah, you know you ought!” she added, looking reproachfully at the old cat, and speaking in as cross a voice as she could manage—and then she scrambled back into the arm-chair, taking the kitten and the worsted with her, and began winding up the ball again. But she didn’t get on very fast, as she was talking all the time, sometimes to the kitten, and sometimes to herself. Kitty sat very demurely on her knee, pretending to watch the progress of the winding, and now and then putting out one paw and gently touching the ball, as if it would be glad to help, if it might.
";//first 4 paragraphs in alice in wonderland
            var es = _serviceProvider.GetRequiredService<EncodingService>();
            List<int> plainArr = es.preProccessCiphertext(plaintext).ToList();
            List<string> allFitnessStr = new List<string>() { "IOC", "BI", "TRI", "QUAD" };
            string successRates = "";
            for(int i = 100; i < plainArr.Count; i += 100)
            {
                foreach (string fitness in allFitnessStr)
                {
                    successRates += fitness + "," + i + "," + bs.testOffset(fitness,plainArr.GetRange(0,i).ToArray()) + "\n";
                }
            }            
            logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation(successRates);
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

            _serviceCollection.AddSingleton<Program>();
            _serviceCollection.AddLogging(cfg => cfg.AddSerilog()).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug);
            _serviceCollection.AddSingleton<BasicService>();
            _serviceCollection.AddSingleton<EncodingService>();

            _serviceCollection.AddTransient<indexOfCoincidence>();
            _serviceCollection.AddTransient<doubleCharFitness>();
            _serviceCollection.AddTransient<tripleCharFitness>();
            _serviceCollection.AddTransient<fourCharFitness>();

            _serviceCollection.AddTransient<IFitness.FitnessResolver>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "IOC":
                        return serviceProvider.GetService<indexOfCoincidence>();
                    case "BI":
                        return serviceProvider.GetService<doubleCharFitness>();
                    case "TRI":
                        return serviceProvider.GetService<tripleCharFitness>();
                    case "QUAD":
                        return serviceProvider.GetService<fourCharFitness>();
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
