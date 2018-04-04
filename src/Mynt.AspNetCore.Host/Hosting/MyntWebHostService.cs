#if !NETCOREAPP2_0
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mynt.AspNetCore.Host.Hosting
{
    internal class MyntWebHostService : WebHostService
    {
        private readonly ILogger<MyntWebHostService> _logger;
        public MyntWebHostService(IWebHost host) 
            : base(host)
        {
            _logger = host.Services.GetRequiredService<ILogger<MyntWebHostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogDebug("Starting {WindowsService}.", nameof(MyntWebHostService));
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            _logger.LogInformation("{WindowsService} started.", nameof(MyntWebHostService));
        }

        protected override void OnStopping()
        {
            _logger.LogDebug("Stopping {WindowsService}.", nameof(MyntWebHostService));
            base.OnStopping();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            _logger.LogInformation("Stopped {WindowsService}.", nameof(MyntWebHostService));
        }
    }
}
#endif