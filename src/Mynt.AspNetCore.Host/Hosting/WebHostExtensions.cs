#if !NETCOREAPP2_0
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

namespace Mynt.AspNetCore.Host.Hosting
{
    public static class WebHostExtensions
    {
        public static void RunAsMyntWindowsService(this IWebHost host)
        {
            var webHostService = new MyntWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}
#endif