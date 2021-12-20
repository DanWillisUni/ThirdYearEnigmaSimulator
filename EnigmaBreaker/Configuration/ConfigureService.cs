using System;
using System.Collections.Generic;
using System.Text;
using Topshelf;

namespace EnigmaBreaker.Configuration
{
    internal static class ConfigureService
    {
        internal static void Configure()
        {
            try
            {
                HostFactory.Run(configure =>
                {
                    configure.Service<Startup>(service =>
                    {
                        service.ConstructUsing(s => new Startup());
                        service.WhenStarted(s => s.Start());
                        service.WhenStopped(s => s.Stop());
                    });
                    configure.RunAsLocalSystem();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
