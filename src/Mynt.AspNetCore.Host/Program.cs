using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mynt.AspNetCore.WindowsService.Hosting;

namespace Mynt.AspNetCore.WindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

#if NETCOREAPP2_0
            host.Run();
#else
            if (Debugger.IsAttached || args.Contains("--console"))
            {
                host.Run();
            }
            else
            {
                host.RunAsMyntWindowsService();
            }
#endif
        }
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(i => 
                    i.AddJsonFile("appsettings.overrides.json", true))
                .UseStartup<Startup>()
                .Build();
    }
}
