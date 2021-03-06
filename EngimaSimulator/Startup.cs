using EngimaSimulator.Configuration.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using EngimaSimulator.Services;
using SharedCL;

namespace EngimaSimulator
{
    public class Startup
    {
        public Startup()
        {
            //create new configuration loading the appsettings json
            var builder = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)              
              .AddEnvironmentVariables();

            //create a new logger
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("logs//" + DateTime.Today.ToString("yyyy") + "//" + DateTime.Today.ToString("MM") + "//" + DateTime.Today.ToString("dd") + "//WebApp-" + DateTime.Now.ToString("HHmmss") + ".txt", restrictedToMinimumLevel: LogEventLevel.Information)
                .MinimumLevel.Information()
                .CreateLogger();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //Adds the objects that are stored in appsettings folder and registers each object
            var physicalSettings = new PhysicalConfiguration();
            Configuration.Bind("PhysicalConfiguration", physicalSettings);
            services.AddSingleton(physicalSettings);
            var basicSettings = new BasicConfiguration();
            Configuration.Bind("BasicSettings", basicSettings);
            services.AddSingleton(basicSettings);

            //if the temp file exists delete it on startup
            if(File.Exists(Path.Combine(basicSettings.tempConfig.dir, basicSettings.tempConfig.fileName))) { 
                //File.Delete(Path.Combine(basicSettings.tempConfig.dir, basicSettings.tempConfig.fileName));
            }
            services.AddLogging(cfg => cfg.AddSerilog()).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
            services.AddSingleton<EncodingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Enigma}/{action=Index}/{id?}");
            });
        }
    }
}
